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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


using UnifiedAutomation.UaBase;
using UnifiedAutomation.UaClient;

using eu.skillpro.opcua.client.CS;


/// <summary>
/// Interaction logic for BasicWindowOPCUAClient.xaml
/// </summary>
public partial class BasicWindowOPCUAClient : Window
{
    //####################################### ATTRIBUTES #################################################################""
    //####################################################################################################################"


    private SkillProCsharpOPCUAClient m_OPCUA_client = null;



    //####################################### UI #########################################################################""
    //####################################################################################################################"

    #region window

    public BasicWindowOPCUAClient()
    {
        InitializeComponent();

        //m_application = new ApplicationInstance();
        //m_application.AutoCreateCertificate = true;
        //// Create the certificate if it does not exist yet
        //m_application.Start();

        m_OPCUA_client = new SkillProCsharpOPCUAClient();
        m_OPCUA_client.SettingsRequest.OperationTimeout = 10000;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (m_OPCUA_client.IsConnected)
        {
            try
            {
                m_OPCUA_client.disconnect();
            }
            catch (Exception exception)
            {
                ExceptionDlg.Show(null, exception);
                //throw exception;
            }
        }
    }



    #endregion //window

    #region linked on events

    /// <summary>
    /// receive updates about session state.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 
    private void Session_ServerConnectionStatusUpdate(Session sender, ServerConnectionStatusUpdateEventArgs e)
    {

        // Previously, this was : "InvokeRequired". But I don't see the best way to replace it in WPF : there is a problem with disconnection
        // The actual way with dispatch is the good way
        if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
        {
            Dispatcher.BeginInvoke(new ServerConnectionStatusUpdateEventHandler(Session_ServerConnectionStatusUpdate), sender, e);
            return;
        }

        //// check that the current session matches the session that raised the event.
        //if (!Object.ReferenceEquals(m_OPCUA_client.Session, sender))
        //{
        //    return;
        //}

        lock (this)
        {
            bool allowEditing = true;

            switch (e.Status)
            {
                case ServerConnectionStatus.Disconnected:
                    // update status label
                    allowEditing = false;
                    lbl_ConnectionState.Content = "Disconnected";
                    // update buttons
                    StopButtonBT(bt_connect_play);
                    StopButtonBT(bt_statusConnection);



                    //TODO
                    //btnMonitor.Text = "Monitor";
                    break;
                case ServerConnectionStatus.Connected:
                    // update status label
                    allowEditing = true;
                    lbl_ConnectionState.Content = "Connected";
                    // update buttons
                    ActiveButtonBT(bt_connect_play);
                    ActiveButtonBT(bt_statusConnection);




                    break;
                case ServerConnectionStatus.ConnectionWarningWatchdogTimeout:
                    // update status label
                    lbl_ConnectionState.Content = "ConnectionWarningWatchdogTimeout";
                    break;
                case ServerConnectionStatus.ConnectionErrorClientReconnect:
                    // update status label
                    lbl_ConnectionState.Content = "ConnectionErrorClientReconnect";
                    break;
                case ServerConnectionStatus.ServerShutdownInProgress:
                    // update status label
                    lbl_ConnectionState.Content = "ServerShutdownInProgress";
                    break;
                case ServerConnectionStatus.ServerShutdown:
                    // update status label
                    lbl_ConnectionState.Content = "ServerShutdown";
                    break;
                case ServerConnectionStatus.SessionAutomaticallyRecreated:
                    // update status label
                    lbl_ConnectionState.Content = "SessionAutomaticallyRecreated";
                    break;
                case ServerConnectionStatus.Connecting:
                    // update status label
                    lbl_ConnectionState.Content = "Connecting";
                    break;
                case ServerConnectionStatus.LicenseExpired:
                    // update status label
                    lbl_ConnectionState.Content = "LicenseExpired";
                    break;
            }

            // Toggle Textboxes
            txb_ServerURL.IsEnabled = !m_OPCUA_client.IsConnected;


            // Toggle action buttons
            btn_Read.IsEnabled = allowEditing;


            //TODO
            //btnReadAsync.Enabled = allowEditing;
            //btnMonitor.Enabled = allowEditing;
            //btnWrite.Enabled = allowEditing;
            //btnWriteAsync.Enabled = allowEditing;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Subscribtion_DataChanged(Subscription sender, DataChangedEventArgs e)
    {
        // This method is called on the UI thread because it updates UI controls.
        if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
        {
            Dispatcher.BeginInvoke(new DataChangedEventHandler(Subscribtion_DataChanged), sender, e);
            return;
        }

        try
        {
            //// Check that the subscription has not changed.
            //if (!Object.ReferenceEquals(m_OPCUA_client.Subscription, sender))
            //{
            //    return;
            //}

            foreach (DataChange change in e.DataChanges)
            {
                // Get text box for displaying value from user data
                TextBox textBox = change.MonitoredItem.UserData as TextBox;

                if (textBox != null)
                {
                    // Print result for variable - check first the result code
                    if (StatusCode.IsGood(change.Value.StatusCode))
                    {
                        // The node succeeded - print the value as string
                        textBox.Text = change.Value.WrappedValue.ToString();
                        textBox.Background = Brushes.White;
                    }
                    else
                    {
                        // The node failed - print the symbolic name of the status code
                        textBox.Text = change.Value.StatusCode.ToString();
                        textBox.Background = Brushes.Red;
                    }
                }
            }
        }
        catch (Exception exception)
        {
            ExceptionDlg.Show("Error in DataChanged callback", exception);
            //throw exception;
        }
    }

    #endregion //Linked on events

    #region button clicks

    private void btn_connect_Click(object sender, RoutedEventArgs e)
    {
        List<string> allNameSpaces;

        if (m_OPCUA_client.IsConnected)
        {
            try
            {
                m_OPCUA_client.disconnect();
                List<string> str = new List<string>();
                lbx_Namspaces.ItemsSource = str;
            }
            catch (Exception exception)
            {
                ExceptionDlg.Show(null, exception);
                //throw exception;
            }
        }
        else
        {
            try
            {
                m_OPCUA_client.connect(txb_ServerURL.Text, Session_ServerConnectionStatusUpdate, out allNameSpaces);
                lbx_Namspaces.ItemsSource = allNameSpaces;
            }
            catch (Exception exception)
            {
                ExceptionDlg.Show("Connect failed", exception);
                //throw exception;
            }
        }

    }

    private void bt_connect_play_Click(object sender, RoutedEventArgs e)
    {
        List<string> allNameSpaces;

        if (m_OPCUA_client.IsConnected)
        {
            try
            {
                m_OPCUA_client.disconnect();
                List<string> str = new List<string>();
                lbx_Namspaces.ItemsSource = str;
            }
            catch (Exception exception)
            {
                ExceptionDlg.Show(null, exception);
                //throw exception;
            }
        }
        else
        {
            try
            {
                m_OPCUA_client.connect(txb_ServerURL.Text, Session_ServerConnectionStatusUpdate, out allNameSpaces);
                lbx_Namspaces.ItemsSource = allNameSpaces;
            }
            catch (Exception exception)
            {
                ExceptionDlg.Show("Connect failed", exception);
                //throw exception;
            }
        }
    }

    private void btn_Read_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!testBasicConditions())
            {
                return;
            }

            txb_ReadCode.Text = "";
            txb_ReadValue.Text = "";

            List<object> allReturnedValues;
            List<uint> allStatusCodes;


            if (!(bool)ck_nodeIsNumeric.IsChecked)
            {

                // The nodeId.IdentifierType is string => call the appropriate method
                List<string> nodeIds = new List<string>();
                nodeIds.Add(txb_NodeId.Text);


                if (!m_OPCUA_client.readNodeValue(nodeIds, (ushort)lbx_Namspaces.SelectedIndex, out allReturnedValues, out allStatusCodes))
                {
                    System.Windows.MessageBox.Show("internal error??", "error : select namespace", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
            }
            else
            {

                // The nodeId.IdentifierType is numeric => call the appropriate method

                List<uint> nodeIds = new List<uint>();
                nodeIds.Add(uint.Parse(txb_NodeId.Text));


                if (!m_OPCUA_client.readNodeValue(nodeIds, (ushort)lbx_Namspaces.SelectedIndex, out allReturnedValues, out allStatusCodes))
                {
                    System.Windows.MessageBox.Show("internal error??", "error : select namespace", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
            }

            txb_ReadCode.Text = allStatusCodes[0].ToString();

            if (allStatusCodes[0] == 0)
            {
                ActiveButtonBT(btn_display_statusCode);
                txb_ReadValue.Text = allReturnedValues[0].ToString();
            }
            else
            {
                StopButtonBT(btn_display_statusCode);
                txb_ReadValue.Text = "Error occured, see error code";
            }

        }
        catch (Exception ex)
        {
            ExceptionDlg.Show("reading the node failed", ex);
        }
    }

    private void btn_write_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!testBasicConditions())
            {
                return;
            }

            int OperationTimeout = 1000;

            List<uint> allStatusCode;
            List<string> valuesToWrite = new List<string>();
            valuesToWrite.Add(txb_write.Text);

            if (!(bool)ck_nodeIsNumeric.IsChecked)
            {

                // The nodeId.IdentifierType is string => call the appropriate method
                List<string> NodesIdentifiers = new List<string>();
                NodesIdentifiers.Add(txb_NodeId.Text);

                m_OPCUA_client.writeNodeValue(NodesIdentifiers, valuesToWrite, (ushort)lbx_Namspaces.SelectedIndex, OperationTimeout, out allStatusCode);

            }
            else
            {
                // The nodeId.IdentifierType is numeric => call the appropriate method
                List<uint> NodesIdentifiers = new List<uint>();
                NodesIdentifiers.Add(uint.Parse(txb_NodeId.Text));

                m_OPCUA_client.writeNodeValue(NodesIdentifiers, valuesToWrite, (ushort)lbx_Namspaces.SelectedIndex, OperationTimeout, out allStatusCode);

            }

            txb_ReadCode.Text = allStatusCode[0].ToString();

            if (allStatusCode[0] == 0)
                ActiveButtonBT(btn_display_statusCode);
            else
                StopButtonBT(btn_display_statusCode);

        }
        catch (Exception ex)
        {
            ExceptionDlg.Show("writing the node failed", ex);
        }



    }

    private void btn_Monitor_Click(object sender, RoutedEventArgs e)
    {

        if (!testBasicConditions())
        {
            return;
        }

        if (m_OPCUA_client.Subscription == null)
        {
            List<uint> allStatusCode;
            double samplingInterval = 100;


            List<object> textBoxes = new List<object>();
            //TextBox dummy = new TextBox();
            textBoxes.Add(txb_Monitoring);

            // create the subscription
            try
            {
                if (!(bool)ck_nodeIsNumeric.IsChecked)
                {

                    // The nodeId.IdentifierType is string => call the appropriate method
                    List<string> NodesIdentifiers = new List<string>();
                    NodesIdentifiers.Add(txb_NodeId.Text);

                    m_OPCUA_client.StartMonitoring(NodesIdentifiers, textBoxes, Subscribtion_DataChanged, (ushort)lbx_Namspaces.SelectedIndex, samplingInterval, out allStatusCode);

                }
                else
                {
                    // The nodeId.IdentifierType is numeric => call the appropriate method

                    List<uint> NodesIdentifiers = new List<uint>();
                    NodesIdentifiers.Add(uint.Parse(txb_NodeId.Text));

                    m_OPCUA_client.StartMonitoring(NodesIdentifiers, textBoxes, Subscribtion_DataChanged, (ushort)lbx_Namspaces.SelectedIndex, samplingInterval, out allStatusCode);

                }

                txb_ReadCode.Text = allStatusCode[0].ToString();

                if (allStatusCode[0] == 0)
                    ActiveButtonBT(btn_display_statusCode);
                else
                    StopButtonBT(btn_display_statusCode);

                ActiveButtonBT(btn_Monitor);
            }
            catch (Exception exception)
            {
                ExceptionDlg.Show("Create subscription failed", exception);

                m_OPCUA_client.Subscription = null;
                ActiveButtonBT(btn_Monitor);

            }
        }
        else
        {
            try
            {
                m_OPCUA_client.StopMonitoring();
                StopButtonBT(btn_Monitor);
                txb_Monitoring.Text = "";
            }
            catch (Exception exception)
            {
                ExceptionDlg.Show("Stopping  monitoring failed", exception);
            }
        }
    }

    private void btn_callMethod_Click(object sender, RoutedEventArgs e)
    {
        //check the basic conditions
        if (!testBasicConditions())
        {
            return;
        }

        //check that a type is selected
        if (cb_inputParamType.SelectedIndex == -1)
        {
            MessageBox.Show("You must select a type for your input parameter");
            return;
        }

        ComboBoxItem item = (ComboBoxItem)cb_inputParamType.SelectedItem;
        string Content = item.Content.ToString();

        object objectId;
        object methodId;


        //get the ID of the node
        bool ObIdIsString = (bool)ck_nodeIsNumeric_callMethod.IsChecked;
        ObIdIsString = !ObIdIsString;

        if (ObIdIsString)
        {
            objectId = (object)txb_NodeId_callMethod.Text;
        }
        else
        {
            objectId = (object)uint.Parse(txb_NodeId_callMethod.Text);
        }


        //get the ID of the method
        bool slctdMthdIsString = (bool)ck_methodIsNumeric_callMethod.IsChecked;
        slctdMthdIsString = !slctdMthdIsString;

        if (slctdMthdIsString)
        {
            methodId = (object)txb_methodId_callMethod.Text;
        }
        else
        {
            methodId = (object)uint.Parse(txb_methodId_callMethod.Text);
        }


        //extract the inputs
        string inputsInString = txb_inputParams_callMethod.Text;
        string[] AllInputsInString = inputsInString.Split(';');


        //Parse and fill the values
        List<Variant> inputs = new List<Variant>();

        foreach (string currentInput in AllInputsInString)
        {
            switch (Content)
            {
                case "No input parameter":
                    {
                        break;
                    }
                case "Float":
                    {
                        float theParsedValue = float.Parse(currentInput);
                        inputs.Add((Variant)theParsedValue);
                        break;
                    }
                case "String":
                    {
                        inputs.Add((Variant)currentInput);
                        break;
                    }
                default:
                    {
                        MessageBox.Show("Type for input parameter is not implemented");
                        break;
                    }
            }
        }

        bool callSucceed;
        List<Variant> outputs;
        List<uint> statusCodeOut;
        uint errorCode;

        callSucceed = m_OPCUA_client.CallSynchronously(objectId, ObIdIsString, methodId, slctdMthdIsString, (ushort)lbx_Namspaces.SelectedIndex, inputs, 10000, out statusCodeOut, out outputs, out errorCode);

        uint LastError = 0;
        foreach (uint scInput in statusCodeOut)
        {
            if (scInput != 0)
            {
                LastError = scInput;
            }
        }

        if (callSucceed)
        {
            if (LastError == 0)
            {
                ActiveButtonBT(btn_display_statusCode_callMethod);
                txb_statusCode_callMethod.Text = errorCode.ToString();

                string outputString = "";
                foreach (Variant var in outputs)
                {
                    outputString += var.ToString() + ';';
                }

                txb_outputParams_callMethod.Text = outputString.Substring(0, outputString.Length - 1);
            }
            else
            {
                //At least one input argument was bad
                StopButtonBT(btn_display_statusCode_callMethod);
                txb_statusCode_callMethod.Text = LastError.ToString();
                txb_outputParams_callMethod.Text = "Not relevant";
            }

        }
        else
        {
            StopButtonBT(btn_display_statusCode_callMethod);
            txb_statusCode_callMethod.Text = errorCode.ToString();
            txb_outputParams_callMethod.Text = "Not relevant";
        }
    }

    #endregion //button clicks

    #region button images

    private void ActiveButtonBT(Button _bt)
    {
        switch (_bt.Name)
        {
            //case "bt_stop":
            //    img_bt_stop.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/AkeoplusVisionDemo;component/img/Stop_fill.png") as ImageSource;
            //    break;

            case "bt_connect_play":
                img_bt_connect_play.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/OPCUA_csharp_client_midterm;component/images/stop.png") as ImageSource;
                break;

            case "bt_statusConnection":
                img_bt_statusConnection.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/OPCUA_csharp_client_midterm;component/images/mesure_on.png") as ImageSource;
                break;

            case "btn_Monitor":
                img_bt_monitor.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/OPCUA_csharp_client_midterm;component/images/stop.png") as ImageSource;
                break;

            case "btn_display_statusCode":
                img_bt_display_statusCode.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/OPCUA_csharp_client_midterm;component/images/Check_c.png") as ImageSource;
                break;

            case "btn_display_statusCode_callMethod":
                img_bt_display_statusCode_callMethod.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/OPCUA_csharp_client_midterm;component/images/Check_c.png") as ImageSource;
                break;

                
            default: break;
        }
    }

    private void StopButtonBT(Button _bt)
    {
        switch (_bt.Name)
        {
            //case "bt_stop":
            //    img_bt_stop.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/AkeoplusVisionDemo;component/img/Stop_fill.png") as ImageSource;
            //    break;

            case "bt_connect_play":
                img_bt_connect_play.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/OPCUA_csharp_client_midterm;component/images/start.png") as ImageSource;
                break;

            case "bt_statusConnection":
                img_bt_statusConnection.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/OPCUA_csharp_client_midterm;component/images/mesure_off.png") as ImageSource;
                break;

            case "btn_Monitor":
                img_bt_monitor.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/OPCUA_csharp_client_midterm;component/images/start.png") as ImageSource;
                break;

            case "btn_display_statusCode":
                img_bt_display_statusCode.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/OPCUA_csharp_client_midterm;component/images/Cancel_c.png") as ImageSource;
                break;

            case "btn_display_statusCode_callMethod":
                img_bt_display_statusCode_callMethod.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/OPCUA_csharp_client_midterm;component/images/Cancel_c.png") as ImageSource;
                break;

            default: break;
        }
    }


    #endregion //button images

    //####################################### PRIVATE METHODES ################################################################""
    //####################################################################################################################"

    private bool testBasicConditions()
    {
        if (m_OPCUA_client == null)
        {
            System.Windows.MessageBox.Show("you must connect your client first", "error : select namespace", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return false;
        }

        if (!m_OPCUA_client.IsConnected)
        {
            System.Windows.MessageBox.Show("you must connect your client first", "error : select namespace", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return false;
        }

        if (lbx_Namspaces.SelectedIndex == -1)
        {
            System.Windows.MessageBox.Show("you must select a namespace first", "error : select namespace", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return false;
        }

        return true;
    }

}


