/*****************************************************************************
 *
 * Copyright 2012-2016 SkillPro Consortium
 *
 * Author: Boris Bocquet, email: b.bocquet@akeoplus.com
 *
 * Date of creation: 2016
 *
 * +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
 *
 * This file is part of the SkillPro Framework. The SkillPro Framework
 * is developed in the SkillPro project, funded by the European FP7
 * programme (Grant Agreement 287733).
 *
 * The SkillPro Framework is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * The SkillPro Framework is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * You should have received a copy of the GNU Lesser General Public License
 * along with the SkillPro Framework.  If not, see <http://www.gnu.org/licenses/>.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using eu.skillpro.see.CS;
using eu.skillpro.see.CS.AML;
using eu.skillpro.see.CS.AmsService;
using simpleSEE.UI;
using System.Diagnostics;

using System.Threading;

using UnifiedAutomation.UaBase;
using UnifiedAutomation.UaClient;

namespace simpleSEE
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public const string SKILL_PAUSE = "Pause Skill Execution";
        public const string SKILL_RESUME = "Resume Skill Execution";

        //####################################### ATTRIBUTES ####################################################################
        //####################################################################################################################### 
        
        private Visibility _VisibilityConfigured = Visibility.Hidden;

        private Visibility _VisibilityShutdown = Visibility.Hidden;

        private Visibility _VisibilityRecover = Visibility.Hidden;

        See _DedicatedSee = null;

        WindowSkillExecution _windowSkillExe = null;

        SkillProException _LastException = null;

        private string _PauseResumeButtonName = SKILL_PAUSE;

        private ManualResetEventSlim _MreMouse = new ManualResetEventSlim(false);

        private int _MsWaitOnGrid = 500;

        //####################################### PROPERTIES ####################################################################
        //####################################################################################################################### 

        public Visibility VisibilityConfigured
        {
            get
            {

                if (_DedicatedSee == null)
                    return System.Windows.Visibility.Hidden;

                bool Usable = _DedicatedSee.EXT_CONFIGURED_FEASIBLE;

                if (Usable)
                    _VisibilityConfigured = System.Windows.Visibility.Visible;
                else
                    _VisibilityConfigured = System.Windows.Visibility.Hidden;

                bt_ext_configured.Visibility = _VisibilityConfigured;

                return _VisibilityConfigured;
            }
            set
            {
                _VisibilityConfigured = value;
                bt_ext_configured.Visibility = _VisibilityConfigured;
            }
        }

        public Visibility VisibilityShutdown
        {
            get
            {

                if (_DedicatedSee == null)
                    return System.Windows.Visibility.Hidden;

                bool Usable = _DedicatedSee.EXT_SHUTDOWN_FEASIBLE;

                if (Usable)
                    _VisibilityShutdown = System.Windows.Visibility.Visible;
                else
                    _VisibilityShutdown = System.Windows.Visibility.Hidden;

                bt_ext_shutdown.Visibility = _VisibilityShutdown;

                return _VisibilityShutdown;
            }
            set
            {
                _VisibilityShutdown = value;
                bt_ext_shutdown.Visibility = _VisibilityShutdown;
            }
        }

        public Visibility VisibilityRecover
        {
            get
            {

                if (_DedicatedSee == null)
                    return System.Windows.Visibility.Hidden;

                bool Usable = _DedicatedSee.EXT_RECOVER_FEASIBLE;

                if (Usable)
                    _VisibilityRecover = System.Windows.Visibility.Visible;
                else
                    _VisibilityRecover = System.Windows.Visibility.Hidden;

                bt_ext_recover.Visibility = _VisibilityRecover;

                return _VisibilityRecover;
            }
            set
            {
                _VisibilityRecover = value;
                bt_ext_recover.Visibility = _VisibilityRecover;
            }
        }

        public string PauseResumeButtonName
        {
            get { return _PauseResumeButtonName; }
            set
            {
                _PauseResumeButtonName = value;
                bt_PauseResumeExe.Content = value;
            }
        }

        private System.Collections.ObjectModel.ObservableCollection<string> _CallableSkills;

        public System.Collections.ObjectModel.ObservableCollection<string> CallableSkills
        {
            get { return _CallableSkills; }
            set { _CallableSkills = value; }
        }

        private System.Collections.ObjectModel.ObservableCollection<string> _SkillsNames;

        public System.Collections.ObjectModel.ObservableCollection<string> SkillsNames
        {
            get { return _SkillsNames; }
            set { _SkillsNames = value; }
        }

        //####################################### METHODS ################################################################
        //#######################################################################################################################

        #region constructor / destructor

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            CallableSkills = new System.Collections.ObjectModel.ObservableCollection<string>();
            CallableSkills.Add("nothing");

            SkillsNames = new System.Collections.ObjectModel.ObservableCollection<string>();
            SkillsNames.Add("nothing");

            //If you want to log the SEE's console message into a file, then do this

            string pathOfAkpsLog = @"logfileSEE.txt";

            if (System.IO.File.Exists(pathOfAkpsLog))
            {
                System.IO.File.Delete(pathOfAkpsLog);
            }

            System.IO.StreamWriter s = new System.IO.StreamWriter(pathOfAkpsLog, false);
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Debug.Listeners.Add(new TextWriterTraceListener(s));
            Debug.AutoFlush = true;

            this.DataContext = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_DedicatedSee != null)
            {
                _DedicatedSee.Dispose();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        #endregion //constructor / destructor

        #region loading and init

        private void loadingAndInit()
        {
            Cursor Backup = Mouse.OverrideCursor;

            try
            {

                Mouse.OverrideCursor = Cursors.Wait;

                #region STEP 1 : instanciate your Skill Based Resource Controller (probalby by loading aml files)

                //create or deserialize all the conditions
                SkillBasedResourceController sbrc;
                AMLSkillExecutionEngine amlSee;
                createSBRC(out sbrc, out amlSee);

                #endregion //STEP 1 : instanciate your Skill Based Resource Controller (probalby by loading aml files)

                #region STEP 2 : instanciate your SEE

                //Instanciate your dedicated See

                if (rb_PopupWindowSee.IsChecked == true)
                {
                    _DedicatedSee = new DedicatedPopUpSee(amlSee, sbrc);
                }
                else
                {
                    if (rb_helloWorldSee.IsChecked == true)
                    {
                        _DedicatedSee = new HelloWorldSee(amlSee, sbrc);
                    }
                    else
                    {
                        _DedicatedSee = new HelloWorldAndQuestionsSee(amlSee, sbrc);
                    }
                }

                #endregion //STEP 2 : instanciate your SEE

                #region optional : check OPC-UA connection

                //check connection of OPC-UA client
                //Wait for licence expired
                Thread.Sleep(250);

                if (_DedicatedSee.OpcUaClient.IsConnected && _DedicatedSee.OpcUaClient.Session.ConnectionStatus != ServerConnectionStatus.LicenseExpired)
                {
                    Action refresh = new Action(() =>
                    {
                        ToggleOnInsideInformationGroupbox(grid_status_OPCUA);
                    });
                    Dispatcher.BeginInvoke(refresh, System.Windows.Threading.DispatcherPriority.Render, null);
                }
                else
                {
                    Action refresh = new Action(() =>
                    {
                        ToggleOffInsideInformationGroupbox(grid_status_OPCUA);
                    });
                    Dispatcher.BeginInvoke(refresh, System.Windows.Threading.DispatcherPriority.Render, null);
                }

                #endregion //optional : check OPC-UA connection

                #region STEP 3 : attach all events to SEE

                subscribeEventsDedicatedSee();

                #endregion //STEP 3: attach all events to SEE

                //Refresh UI with SEE informations
                lbl_seeNodeId.Content = _DedicatedSee.OpcUaConfiguration.SeeNodeId.ToString();
                lbl_AmsUri.Content = _DedicatedSee.ConfigurationAms.Uri;
                lbl_OpcuaUrl.Content = _DedicatedSee.OpcUaConfiguration.ServerUrl;
                lbl_seeId.Content = _DedicatedSee.SBRC.SeeAmlDescription.ID;
                lbl_seeName.Content = _DedicatedSee.SBRC.SeeAmlDescription.Name;
                lbl_spacenameIndex.Content = _DedicatedSee.OpcUaConfiguration.SpaceNameIndex;

                //Refresh buttons
                UpdateButtonVisibility();

                //Refresh callable skills
                bt_reloadCallableSkills_Click(null, null);

                Mouse.OverrideCursor = Backup;

            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = Backup;
                DisplayException(ex);
            }

        }

        private void createSBRC(out SkillBasedResourceController Sbrc, out AMLSkillExecutionEngine amlSee)
        {
            string expectedPath = @tb_AmlFile.Text;

            AMLDocument DocAml = AMLSerializer.DeserializeFromFile(expectedPath);

            if (DocAml.SkillExecutionEngine.Count != 1)
                throw new Exception("You must have one and only one SEE in your aml file");

            amlSee = DocAml.SkillExecutionEngine[0];
            Sbrc = new SkillBasedResourceController(DocAml.SkillExecutionEngine[0], DocAml.ExecutableSkills);

        }

        #endregion

        #region Clicks

        private void bt_instanciateSee_Click(object sender, RoutedEventArgs e)
        {
            if (_DedicatedSee != null)
            {
                System.Windows.MessageBox.Show("SEE already instanciated => dispose first");
                return;
            }
            loadingAndInit();
            UpdateButtonVisibility();
        }

        private void bt_disposeSee_Click(object sender, RoutedEventArgs e)
        {
            if (_DedicatedSee == null)
            {
                System.Windows.MessageBox.Show("SEE already disposed => instanciate first");
                return;
            }

            Cursor Backup = Mouse.OverrideCursor;

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                _DedicatedSee.Dispose();

                ToggleOffInsideInformationGroupbox(grid_status_OPCUA);

                _DedicatedSee = null;

                Mouse.OverrideCursor = Backup;

            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = Backup;

                DisplayException(ex);   
            }
        }

        private void bt_login_Click(object sender, RoutedEventArgs e)
        {
            if (bt_adminstration.IsEnabled)
            {
                bt_adminstration.IsEnabled = false;
                bt_adminstration.Foreground = Brushes.Gray;
                bt_login.Header = "Login";

            }
            else
            {
                simpleSEE.UI.InputBox _input = new simpleSEE.UI.InputBox();
                _input.ShowDialog();

                if (_input.mdp != "skillpro")
                {
                    MessageBox.Show("Wrong password.");
                }
                else
                {
                    bt_adminstration.IsEnabled = true;
                    bt_adminstration.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF436EFF"));
                    bt_login.Header = "Logout";
                }
            }
        }

        private void bt_quit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void bt_reloadCallableSkills_Click(object sender, RoutedEventArgs e)
        {
            //Refresh callable skills
            try
            {
                if (_DedicatedSee == null)
                    return;


                //Display callable skills whith IDs
                List<string> LinesList = _DedicatedSee.SBRC.GetCallableStrings();
                LinesList.Reverse();
                LinesList.Add("nothing");
                LinesList.Reverse();
                CallableSkills = new System.Collections.ObjectModel.ObservableCollection<string>(LinesList);

                //Take care to have exactly the same order, because the index will then be used
                List<string> sNames = _DedicatedSee.SBRC.AllExecutableSkills.Select(elem => elem.AmlSkillDescription.Name).ToList();
                sNames.Reverse();
                sNames.Add("nothing");
                sNames.Reverse();
                SkillsNames = new System.Collections.ObjectModel.ObservableCollection<string>(sNames);

                if (ckb_displaySkillNames.IsChecked == null || !(bool)ckb_displaySkillNames.IsChecked)
                {
                    lb_CallableSkill.ItemsSource = CallableSkills;
                }
                else
                {
                    lb_CallableSkill.ItemsSource = SkillsNames;
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void lb_CallableSkill_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //OKay, so normally you are not supposed to do that, so the next following lines are not commented, not robust.
        
            if (lb_CallableSkill.SelectedIndex < 0)
            {
                return;
            }

            if (_DedicatedSee == null)
            {
                return;
            }

            int MsWait = 0;

            try
            {
                MsWait = int.Parse(tb_MsTimestamp.Text);
            }
            catch (Exception ex)
            {
            }


            //Yes : the index is the same either you display the Names or the Ids of the skill
            string Value = CallableSkills[lb_CallableSkill.SelectedIndex];

            if(MsWait>0)
            {
                UInt64 TimeStamp = SkillProDefinitions.GetSkillProTimestampFuture(MsWait);
                Value += ":" + TimeStamp.ToString();
            }

            _DedicatedSee.OpcUaSetCallVariable(Value, true);

        }

        private void bt_setCurrentConfiguration_Click(object sender, RoutedEventArgs e)
        {
            //OKay, so normally you are not supposed to do that, so the next following lines are not commented, not robust.

            if (_DedicatedSee == null)
                return;

            try
            {
                lock (_DedicatedSee.StateMachine)
                {
                    if (_DedicatedSee.StateMachine.CurrentState == SeeResourceMode.Error || _DedicatedSee.StateMachine.CurrentState == SeeResourceMode.Idle || _DedicatedSee.StateMachine.CurrentState == SeeResourceMode.PreOperational)
                    {
                        List<string> configs = new List<string>(_DedicatedSee.SBRC.AllKnownConfigurations.Keys.ToList());
                        configs.RemoveAll(elem => elem == SkillProDefinitions.SAME_CONFIGURATION || elem == SkillProDefinitions.ANY_CONFIGURATIONS);
                        WindowSetYourConfiguration win = new WindowSetYourConfiguration(configs);
                        win.ShowDialog();

                        string selected = win.SelectedConfiguration;

                        if (string.IsNullOrEmpty(selected))
                            return;

                        string def;
                        bool selectedExist = _DedicatedSee.SBRC.AllKnownConfigurations.TryGetValue(selected, out def);

                        if (!selectedExist)
                            def = selected;

                        _DedicatedSee.SBRC.CurrentConfiguration = new KeyValuePair<string, string>(selected, def);

                        _DedicatedSee.OpcUaSetConfigurationCondition(selected, true);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);   
            }
        }

        private void bt_setCurrentProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_DedicatedSee == null)
                return;

            try
            {
                lock (_DedicatedSee.StateMachine)
                {
                    if (_DedicatedSee.StateMachine.CurrentState == SeeResourceMode.Error || _DedicatedSee.StateMachine.CurrentState == SeeResourceMode.Idle || _DedicatedSee.StateMachine.CurrentState == SeeResourceMode.PreOperational)
                    {
                        WindowSetYourProduct win = new WindowSetYourProduct(_DedicatedSee.SBRC.AllKnownProductsAndQuantities, _DedicatedSee.SBRC.CurrentProductsAndQuantities);
                        win.ShowDialog();

                        if (win.Cancelled)
                            return;

                        List<KeyValuePair<string, string>> l = new List<KeyValuePair<string, string>>();

                        for (int i = 0; i < win.DataGridSelectedProductsAndQuantity.Count; i++)
                        {

                            l.Add(win.DataGridSelectedProductsAndQuantity.ElementAt(i));
                        }

                        _DedicatedSee.SBRC.CurrentProductsAndQuantities = l;

                        string ToWrite = _DedicatedSee.SBRC.CurrentProductsToOpcUaString();

                        _DedicatedSee.OpcUaSetProductCondition(ToWrite, true);
                    }
                    else
                    {
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }

        }

        private void bt_retrieveAllSkillsFromAms_Click(object sender, RoutedEventArgs e)
        {
            if (_DedicatedSee == null)
            {
                DisplayException(new Exception("Instanciate your SEE first"));
                return;
            }

            if (!_DedicatedSee.ConnectionAmsServiceOK)
            {
                DisplayException(new Exception("Can\'t connect to AMS service"));
                return;
            }

            AnswerStatusMessage StatusMessage;
           AnswerRetrieveExecutableSkill[] AllExeSkills =   AmsServiceWebClient.RetrieveAllExecutableSkills(_DedicatedSee.ConfigurationAms, out StatusMessage);

            if (StatusMessage != null)
            {
                DisplayException(new Exception(StatusMessage.message));
                return;
            }

            string Text = AnswerRetrieveExecutableSkill.ToAmlFile(AllExeSkills, "AnswerFromAmsService.aml", "SkillPro-Catalogue", _DedicatedSee.OpcUaConfiguration.SeeId);

            System.IO.File.WriteAllText("AnswerFromAmsService.aml", Text);
        }

        #endregion //Clicks

        #region events

        private void subscribeEventsDedicatedSee()
        {
            if (_DedicatedSee != null)
            {
                //STANDARD
                if (_DedicatedSee.OpcUaClient != null)
                {
                    _DedicatedSee.ConnectionStatusUpdate -= _DedicatedSee_ConnectionStatusUpdate;
                    _DedicatedSee.CallInvokedByServer -= _DedicatedSee_CallInvokedByServer;
                    _DedicatedSee.StateMachine.StateChanged -= StateMachine_StateChanged;
                    _DedicatedSee.NewExceptionOccured -= _DedicatedSee_LastExceptionValueChanged;

                    _DedicatedSee.ConnectionStatusUpdate += _DedicatedSee_ConnectionStatusUpdate;
                    _DedicatedSee.CallInvokedByServer += _DedicatedSee_CallInvokedByServer;
                    _DedicatedSee.StateMachine.StateChanged += StateMachine_StateChanged;
                    _DedicatedSee.NewExceptionOccured += _DedicatedSee_LastExceptionValueChanged;
                }

                //SPECIFIC
                if (_DedicatedSee.GetType() == typeof(DedicatedPopUpSee))
                {
                    var pop = (DedicatedPopUpSee)_DedicatedSee;
                    pop.DisplayOnSkillWindow -= _DedicatedSee_DisplayOnSkillWindow;
                    pop.DisplayOnSkillWindow += _DedicatedSee_DisplayOnSkillWindow;
                }


            }

        }

        void _DedicatedSee_LastExceptionValueChanged(object sender, SkillProException newValue)
        {
            if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                Dispatcher.BeginInvoke(new See.NewExceptionOccuredDelegate(_DedicatedSee_LastExceptionValueChanged), this, newValue);
                return;
            }

            lbl_Last_Exception.Content = newValue.Message;
            _LastException = newValue;

            UpdateButtonVisibility();

        }

        void StateMachine_StateChanged(object sender, StateChangedEventArgs args)
        {
            if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                Dispatcher.BeginInvoke(new SeeStateMachine.StateChangedDelegate(StateMachine_StateChanged), this, args);
                return;
            }

            // check that the current session matches the session that raised the event.
            //if (!Object.ReferenceEquals(this, sender))
            //{
            //    return;
            //}

            lbl_SEE_SATE.Content = "SEE STATE :  " + args.NewState.ToString();

            if (_DedicatedSee != null && _DedicatedSee.ExceptionQueue != null && _DedicatedSee.ExceptionQueue.Count != 0)
            {
                //List<SkillProException> l = _DedicatedSee.ExceptionQueue.ToList();
                //lbl_Last_Exception.Content = "Last exception : " + l[0].Message;
            }
            else
            {
                _LastException = null;
                lbl_Last_Exception.Content = "Last exception :";
            }

            switch (args.NewState)
            {
                case SeeResourceMode.PreOperational:
                    bt_PauseResumeExe.Visibility = System.Windows.Visibility.Hidden;
                    break;
                case SeeResourceMode.ExecutingSkill:
                    bt_PauseResumeExe.Visibility = System.Windows.Visibility.Visible;
                    PauseResumeButtonName = SKILL_PAUSE;
                    break;
                case SeeResourceMode.ExecutingSkillPausing:
                    bt_PauseResumeExe.Visibility = System.Windows.Visibility.Hidden;
                    break;
                case SeeResourceMode.ExecutingSkillResumable:
                    bt_PauseResumeExe.Visibility = System.Windows.Visibility.Visible;
                    PauseResumeButtonName = SKILL_RESUME;
                    break;
                case SeeResourceMode.Idle:
                    bt_PauseResumeExe.Visibility = System.Windows.Visibility.Hidden;
                    break;
                case SeeResourceMode.IdleQueuedSkill:
                    bt_PauseResumeExe.Visibility = System.Windows.Visibility.Hidden;
                    break;
                case SeeResourceMode.Error:
                    bt_PauseResumeExe.Visibility = System.Windows.Visibility.Hidden;
                    break;
                default:
                    break;
            }

            if (args.NewState == SeeResourceMode.IdleQueuedSkill)
                bt_ClearQueuedSkill.Visibility = System.Windows.Visibility.Visible;
            else
                bt_ClearQueuedSkill.Visibility = System.Windows.Visibility.Hidden;

            if (args.NewState == SeeResourceMode.ExecutingSkill)
                bt_emercencyStop.Visibility = System.Windows.Visibility.Visible;
            else
                bt_emercencyStop.Visibility = System.Windows.Visibility.Hidden;

            updateSeeStateOnWindow(args.NewState);
            UpdateButtonVisibility();
        }

        private void updateSeeStateOnWindow(SeeResourceMode NewState)
        {
            UpdateButtonVisibility();

            switch (NewState)
            {
                case SeeResourceMode.PreOperational:
                    RectangleToBlue(grid_SEE_STATE);
                    break;
                case SeeResourceMode.ExecutingSkill:
                    RectangleToGreen(grid_SEE_STATE);
                    break;
                case SeeResourceMode.ExecutingSkillPausing:
                    RectangleToGreen(grid_SEE_STATE);
                    break;
                case SeeResourceMode.ExecutingSkillResumable:
                    RectangleToGreen(grid_SEE_STATE);
                    break;
                case SeeResourceMode.Idle:
                    RectangleToGreen(grid_SEE_STATE);
                    break;
                case SeeResourceMode.IdleQueuedSkill:
                    RectangleToGreen(grid_SEE_STATE);
                    break;
                case SeeResourceMode.Error:
                    RectangleToRed(grid_SEE_STATE);
                    break;
                default:
                    break;
            }
        }

        void _DedicatedSee_ConnectionStatusUpdate(UnifiedAutomation.UaClient.Session sender, UnifiedAutomation.UaClient.ServerConnectionStatusUpdateEventArgs e)
        {

            // Previously, this was : "InvokeRequired". But I don't see the best way to replace it in WPF : there is a problem with disconnection
            // The actual way with dispatch is the good way
            if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                Dispatcher.BeginInvoke(new UnifiedAutomation.UaClient.ServerConnectionStatusUpdateEventHandler(_DedicatedSee_ConnectionStatusUpdate), sender, e);
                return;
            }

            // check that the current session matches the session that raised the event.
            //if (!Object.ReferenceEquals(_DedicatedSee.OpcUaClient.Session, sender))
            //{
            //    return;
            //}

            //lock (this)
            //{

            switch (e.Status)
            {
                case UnifiedAutomation.UaClient.ServerConnectionStatus.Disconnected:
                    ToggleOffInsideInformationGroupbox(grid_status_OPCUA);
                    RectangleToRed(grid_status_OPCUA);
                    break;

                case UnifiedAutomation.UaClient.ServerConnectionStatus.Connected:

                    ToggleOnInsideInformationGroupbox(grid_status_OPCUA);
                    RectangleToBlue(grid_status_OPCUA);
                    break;

                case UnifiedAutomation.UaClient.ServerConnectionStatus.ConnectionWarningWatchdogTimeout:
                    // update status label
                    lbl_status_connectionOPCUA.Content = "Connection Warning Watchdog Timeout";
                    RectangleToRed(grid_status_OPCUA);
                    break;
                case UnifiedAutomation.UaClient.ServerConnectionStatus.ConnectionErrorClientReconnect:
                    // update status label
                    lbl_status_connectionOPCUA.Content = "ConnectionErrorClientReconnect";
                    RectangleToRed(grid_status_OPCUA);
                    break;
                case UnifiedAutomation.UaClient.ServerConnectionStatus.ServerShutdownInProgress:
                    // update status label
                    lbl_status_connectionOPCUA.Content = "ServerShutdownInProgress";
                    RectangleToBlue(grid_status_OPCUA);
                    break;
                case UnifiedAutomation.UaClient.ServerConnectionStatus.ServerShutdown:
                    // update status label
                    lbl_status_connectionOPCUA.Content = "ServerShutdown";
                    RectangleToBlue(grid_status_OPCUA);
                    break;
                case UnifiedAutomation.UaClient.ServerConnectionStatus.SessionAutomaticallyRecreated:
                    // update status label
                    lbl_status_connectionOPCUA.Content = "SessionAutomaticallyRecreated";
                    RectangleToBlue(grid_status_OPCUA);
                    break;
                case UnifiedAutomation.UaClient.ServerConnectionStatus.Connecting:
                    // update status label
                    lbl_status_connectionOPCUA.Content = "Connecting";
                    RectangleToBlue(grid_status_OPCUA);
                    break;
                case UnifiedAutomation.UaClient.ServerConnectionStatus.LicenseExpired:
                    // update status label
                    lbl_status_connectionOPCUA.Content = "LicenseExpired";
                    RectangleToRed(grid_status_OPCUA);
                    break;
            }

            //Refresh buttons
            UpdateButtonVisibility();

            //}

        }

        void _DedicatedSee_DisplayOnSkillWindow(object sender, InputsForFakeWindow args)
        {
            if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                Dispatcher.BeginInvoke(new DedicatedPopUpSee.DisplayOnSkillWindowDelegate(_DedicatedSee_DisplayOnSkillWindow), this, args);
                return;
            }

            // check that the current session matches the session that raised the event.
            //if (!Object.ReferenceEquals(this, sender))
            //{
            //    return;
            //}

            UpdateButtonVisibility();

            if (args.StartANewWindow)
            {

                double timeSec = 1.0 * args.ExecutionTimeMs / 1000.0;
                _windowSkillExe = new WindowSkillExecution(args.NameOfSkill.ToString(), timeSec.ToString(), args.Status);
                _windowSkillExe.Show();

                return;
            }

            if (args.CloseTheWindow)
            {
                if (_windowSkillExe != null)
                {
                    _windowSkillExe.Close();
                }

                return;
            }

            //If code passes here, this mean it was just for a refresh

            if (_windowSkillExe != null)
            {
                if (args.RemainingTimeMs != -1)
                {
                    double timeSec = 1.0 * args.RemainingTimeMs / 1000.0;

                    _windowSkillExe.changeTime(timeSec.ToString());
                    _windowSkillExe.changeStatus(args.Status);
                }
                else
                {
                    //Pausing is called
                    _windowSkillExe.changeStatus(args.Status);
                }

            }

            //Refresh buttons
            UpdateButtonVisibility();

            return;


        }

        void _DedicatedSee_CallInvokedByServer(Subscription sender, DataChangedEventArgs e)
        {
            if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                Dispatcher.BeginInvoke(new UnifiedAutomation.UaClient.DataChangedEventHandler(_DedicatedSee_CallInvokedByServer), sender, e);
                return;
            }

            try
            {

                //Refresh buttons
                UpdateButtonVisibility();

                DataChange change = e.DataChanges[0];

                if (StatusCode.IsGood(change.Value.StatusCode))
                {
                    string calledString = change.Value.WrappedValue.ToString();
                    lbl_call_OPCUA.Content = "Last call : " + Environment.NewLine + calledString;
                }
                else
                {
                    lbl_call_OPCUA.Content = "Last call : " + Environment.NewLine + "OPC-UA status NOK";
                }

                //Refresh buttons
                UpdateButtonVisibility();

            }
            catch (Exception exception)
            {
                //displayException(exception);
                if (e != null)
                {
                    if (e.DataChanges[0] != null)
                    {
                        lbl_call_OPCUA.Content = "Last call : " + Environment.NewLine + e.DataChanges[0].Value.ToString();
                    }
                }
            }
        }

        #endregion //events

        #region UI tricks

        #region information groupbox

        private void ToggleOnInsideInformationGroupbox(Grid theGridThatWillBeChanged)
        {
            switch (theGridThatWillBeChanged.Name)
            {
                case "grid_status_OPCUA":
                    {
                        theGridThatWillBeChanged.Visibility = System.Windows.Visibility.Visible;
                        img_status_connectionOPCUA.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/simpleSEE;component/images/mesure_on.png") as ImageSource;
                        lbl_status_connectionOPCUA.Content = "OPC-UA client connected";
                        RectangleToBlue(theGridThatWillBeChanged);
                        break;
                    }
                default:
                    {
                        MessageBox.Show("Grid not implemented");
                        break;
                    }
            }
        }

        private void RectangleToColor(Grid theGridThatWillBeChanged, Brush Color)
        {
            UIElementCollection childs = theGridThatWillBeChanged.Children;

            for (int i = 0; i < childs.Count; i++)
            {
                if(childs[i].GetType() == typeof(Rectangle))
                {
                    Rectangle rect = (Rectangle)childs[i];
                    rect.Fill = Color;
                }
            }
        }

        private void ToggleOffInsideInformationGroupbox(Grid theGridThatWillBeChanged)
        {

            switch (theGridThatWillBeChanged.Name)
            {
                case "grid_status_OPCUA":
                    {
                        theGridThatWillBeChanged.Visibility = System.Windows.Visibility.Visible;
                        img_status_connectionOPCUA.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/simpleSEE;component/images/mesure_off.png") as ImageSource;
                        lbl_status_connectionOPCUA.Content = "OPC-UA client disconnected";
                        RectangleToRed(theGridThatWillBeChanged);
                        break;
                    }

                default:
                    {
                        MessageBox.Show("Grid not implemented");
                        break;
                    }
            }
        }

        #endregion //information groupbox

        private void UpdateButtonVisibility()
        {
            VisibilityConfigured = VisibilityConfigured;
            VisibilityRecover = VisibilityRecover;
            VisibilityShutdown = VisibilityShutdown;
        }

        private void RectangleToRed(Grid theGridThatWillBeChanged)
        {
            Brush Red = Brushes.Red;
            RectangleToColor(theGridThatWillBeChanged, Red);
        }

        private void RectangleToBlue(Grid theGridThatWillBeChanged)
        {
            var converter = new System.Windows.Media.BrushConverter();
            Brush brush = (Brush)converter.ConvertFromString("#FFF4F4F5");
            RectangleToColor(theGridThatWillBeChanged, brush);
        }

        private void RectangleToGreen(Grid theGridThatWillBeChanged)
        {
            Brush Red = Brushes.Green;
            RectangleToColor(theGridThatWillBeChanged, Red);
        }

        private void lbl_Last_Exception_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_LastException == null)
                return;

            DisplayException(_LastException, "Last Exception");
        }

        #endregion //UI tricks

        #region utils

        private void DisplayException(Exception ex, string caption = "Exception Caught")
        {
            //Version without unified automation
            //string messageToDisplay = ex.Message;
            //MessageBox.Show("Exception was raised : " + messageToDisplay, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);

            UnifiedAutomation.UaBase.ExceptionDlg dlg = new ExceptionDlg();
            dlg.ShowDialog("Exception Caught", ex);
        }

        #endregion //utils

        #region external trigger clicks

        private void bt_ext_configured_Click(object sender, RoutedEventArgs e)
        {
            if (_DedicatedSee == null)
            {
                UpdateButtonVisibility();
                updateSeeStateOnWindow(SeeResourceMode.PreOperational);
                return;
            }

            _DedicatedSee.EXT_CONFIGURED();

            UpdateButtonVisibility();
        }

        private void bt_ext_shutdown_Click(object sender, RoutedEventArgs e)
        {
            if (_DedicatedSee == null)
            {
                UpdateButtonVisibility();
                updateSeeStateOnWindow(SeeResourceMode.PreOperational);
                return;
            }

            _DedicatedSee.EXT_SHUTDOWN();

            UpdateButtonVisibility();
        }

        private void bt_ext_recover_Click(object sender, RoutedEventArgs e)
        {
            if (_DedicatedSee == null)
            {
                UpdateButtonVisibility();
                updateSeeStateOnWindow(SeeResourceMode.PreOperational);
                return;
            }

            bool RecoOk = _DedicatedSee.EXT_RECOVER();

            if (RecoOk)
            {
                lbl_Last_Exception.Content = "Last exception : ";
                _LastException = null;
            }
            UpdateButtonVisibility();
        }

        private void bt_updateExternalTriggers_Click(object sender, RoutedEventArgs e)
        {
            UpdateButtonVisibility();
        }

        #endregion //external trigger clicks

        #region Skill Execution

        private void bt_PauseResumeExe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_DedicatedSee == null)
                    return;

                if (PauseResumeButtonName == SKILL_PAUSE)
                {
                    int ret = _DedicatedSee.PauseSkillExecution();

                    Console.WriteLine("_DedicatedSee.PauseSkillExecution returned " + ret);
                }
                else if(PauseResumeButtonName == SKILL_RESUME)
                {
                    int ret = _DedicatedSee.ResumeSkillExecution();

                    Console.WriteLine("_DedicatedSee.ResumeSkillExecution returned " + ret);
                }

            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void bt_ClearQueuedSkill_Click(object sender, RoutedEventArgs e)
        {
            if (_DedicatedSee != null)
                _DedicatedSee.ClearQueuedSkill();
        }

        private void bt_emercencyStop_Click(object sender, RoutedEventArgs e)
        {
            if (_DedicatedSee != null)
            {
                SkillProException Ex = _DedicatedSee.TryStopSkillExecution();

                if(Ex!=null)
                    DisplayException(new Exception(Ex.Message, Ex.InnerException));
            }
        }


        #endregion //Skill Execution

        #region Mouse interaction

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bt_reloadCallableSkills_Click(null, null);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            bt_reloadCallableSkills_Click(null, null);

        }

        private void grid_call_OPCUA_MouseEnter(object sender, MouseEventArgs e)
        {
            _MreMouse.Reset();

            ThreadStart Ts = new ThreadStart(() =>
                {

                    _MreMouse.Wait(_MsWaitOnGrid);

                    if (_MreMouse.IsSet)
                        return; //Setted by the "leave"

                    Action Refresh = new Action(() =>
                        {
                            lbl_middlePannel.Text = lbl_call_OPCUA.Content.ToString();
                            grid_dataSee.Visibility = System.Windows.Visibility.Hidden;
                            lbl_middlePannel.Visibility = System.Windows.Visibility.Visible;
                        });

                    Dispatcher.BeginInvoke(Refresh);

                });

            Thread T = new Thread(Ts);
            T.Start();
        }

        private void grid_call_OPCUA_MouseLeave(object sender, MouseEventArgs e)
        {
            _MreMouse.Set();
            lbl_middlePannel.Visibility = System.Windows.Visibility.Hidden;
            grid_dataSee.Visibility = System.Windows.Visibility.Visible;
            lbl_middlePannel.Text = "";
        }

        private void grid_Last_Exception_MouseEnter(object sender, MouseEventArgs e)
        {

            _MreMouse.Reset();

            ThreadStart Ts = new ThreadStart(() =>
            {

                _MreMouse.Wait(_MsWaitOnGrid);

                if (_MreMouse.IsSet)
                    return; //Setted by the "leave"

                Action Refresh = new Action(() =>
                {
                    lbl_middlePannel.Text = lbl_Last_Exception.Content + Environment.NewLine + Environment.NewLine + "Clic for more details";
                    grid_dataSee.Visibility = System.Windows.Visibility.Hidden;
                    lbl_middlePannel.Visibility = System.Windows.Visibility.Visible;
                });

                Dispatcher.BeginInvoke(Refresh);

            });

            Thread T = new Thread(Ts);
            T.Start();

        }

        private void grid_Last_Exception_MouseLeave(object sender, MouseEventArgs e)
        {
            _MreMouse.Set();
            lbl_middlePannel.Visibility = System.Windows.Visibility.Hidden;
            grid_dataSee.Visibility = System.Windows.Visibility.Visible;
            lbl_middlePannel.Text = "";
        }

        private void Akeo_Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://akeoplus.com");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SkillPro_Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://skillpro-project.eu");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void OpenFile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog();

            openFileDialog1.FilterIndex = 1; // option selectionnée par default

            bool? answer = openFileDialog1.ShowDialog();

            if (answer == null || answer != true || !openFileDialog1.CheckFileExists || !openFileDialog1.CheckPathExists || openFileDialog1.FileNames.GetLength(0) == 0)
            {
                MessageBox.Show("Cannot load file");
                return;
            }

            tb_AmlFile.Text = openFileDialog1.FileName;
        }

        #endregion //Mouse interaction




    }
}
