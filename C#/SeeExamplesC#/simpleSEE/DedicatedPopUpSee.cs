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

using eu.skillpro.see.CS;
using eu.skillpro.see.CS.AML;

using simpleSEE.UI;

namespace simpleSEE
{
    public class DedicatedPopUpSee : See
    {

        private const int ERROR_DURING_EXECUTION = -301;
        private const int MS_MIN_RANDOM = 5000;
        private const int MS_MAX_RANDOM = 8000;

        private Random _rand;

        public delegate void DisplayOnSkillWindowDelegate(object sender, InputsForFakeWindow args);
        public event DisplayOnSkillWindowDelegate DisplayOnSkillWindow = null;


        /// <summary>
        /// Dedicated constructor, inherited from SEE mother class
        /// </summary>
        /// <param name="AmlSee"></param>
        /// <param name="aSkillBasedResourceController"></param>
        public DedicatedPopUpSee(AMLSkillExecutionEngine AmlSee, SkillBasedResourceController aSkillBasedResourceController)
            : base(AmlSee, aSkillBasedResourceController)
        {
            //Here, you just have to instanciate what belongs to your child class 
            //=> all the other "standard" (connections to OPC-UA, AMS etc) is done in the SEE mother class

            //In our case, we just have one attribute to instanciate
            _rand = new Random();
        }

        protected override void ProvideSkillToExecute(ref ExecutableSkill skillWithNoMethod)
        {
            //You can throw an exception is something is weird in the provided Skill => your SEE will go in error state
            //Note that skillWithNoMethod is tested before (in mother class), so it will NOT be null,  
            // and skillWithNoMethod.AmlSkillDescription.Execution.Type is also NOT NULL

            if (skillWithNoMethod == null)
                throw new Exception("The given skill with no method is null"); 

            //Here, do a big switch case on the Execution's type of the skill.
            //In our case, all the skills will be the "ProcessWindow" (popup)

            switch (skillWithNoMethod.AmlSkillDescription.Execution.Type)
            {
                case "Type1" :
                    //In our case, all the skills will be the "ProcessWindow" (popup)
                    skillWithNoMethod.SkillToExecute = new Skill(ProcessWindow, CloseWindow); //The Skill is the execution of ProcessWindow, followed by CloseWindow
                    break;
                default:
                    //In our case, all the skills will be the "ProcessWindow" (popup)
                    skillWithNoMethod.SkillToExecute = new Skill(ProcessWindow, CloseWindow);//The Skill is the execution of ProcessWindow, followed by CloseWindow
                    break;
            }

        }

        protected override int ProvideInputDataForSkillExecution(ExecutableSkill SkillToExecute, out InputParams ParametersForTheSkill)
        {

            //You can throw an exception, or return an error code is something is weird in the provided Skill => your SEE will go in error state
            //Note that SkillToExecute is tested before (in mother class), so it will NOT be null,  
            // and SkillToExecute.AmlSkillDescription.Execution.Type is also NOT NULL

            if (SkillToExecute == null)
            {
                ParametersForTheSkill = null;
                return -1;
            }

            if (string.IsNullOrEmpty(SkillToExecute.AmlSkillDescription.Name))
            {
                //I need the name of the SKill, because I will display it on the Popup Window.
                //So if the name is not available, I can return a interger!=0
                //or I can fire an exception (this way, I can also add a meaningfull error message)
                
                ParametersForTheSkill = null;

                throw new SkillProException("Required SkillToExecute.AmlSkillDescription.Name was NULL or empty");

            }

            //Here, do a big switch case on the Execution's type of the skill.
            //In our case, since we allways execute the "ProcessWindow" skill (popup), we always have the same input

            int RandomNumber = _rand.Next(MS_MIN_RANDOM, MS_MAX_RANDOM);

            InputsForFakeWindow In = new InputsForFakeWindow() 
            { 
                NameOfSkill = SkillToExecute.AmlSkillDescription.Name, 
                ExecutionTimeMs = RandomNumber, 
                RemainingTimeMs = RandomNumber ,
                AlternativePostConditionAvailable = SkillToExecute.AmlSkillDescription.AltPostCondition != null
            };


            switch (SkillToExecute.AmlSkillDescription.Execution.Type)
            {
                case "Type1":
                    //In our case, we always have the same input params (because one skill)
                    ParametersForTheSkill = new InputParams(0, (object)In);
                    break;
                default:
                    //In our case, all the skills will be the "ProcessWindow" (popup)
                    ParametersForTheSkill = new InputParams(0, (object)In);
                    break;
            }

            //Return 0 if everything is allright
            return 0;
        }

        protected override void GetOutputsOfExecutedSkill(ExecutableSkill SkillExecuted, OutputParams OutputOfExecution)
        {
            //Here you will receive the outputs of the executed skills (in our case, the outputs of "ProcessWindow" or "CloseWindow")

            //So here you will do everything required specifically to your SEE 
            //like updating a database, displaying something on a human/machine interface, logging, beeping...
            
            //but you can also do nothing, if you don't care about the outputs, or maybe you did everyhting in the Skill methods allready

            //So here is a common way of processing your outputs

            //Check that your outputs are OK
            if (SkillExecuted == null)
            {
                System.Windows.MessageBox.Show("SkillExecuted is null");
                return;
            }

            if (SkillExecuted.AmlSkillDescription.Name == null)
            {
                System.Windows.MessageBox.Show("Name of SkillExecuted is null");
                return;
            }

            if (OutputOfExecution == null)
            {
                System.Windows.MessageBox.Show("Outputs of skill "+ SkillExecuted.AmlSkillDescription.Name +"  are null");
                return;
            }

            //Display the return code of the Skill.
            System.Windows.MessageBox.Show("Skill " + SkillExecuted.AmlSkillDescription.Name + "  returned code " + OutputOfExecution.ReturnCode.ToString());
            return;

        }

        private OutputParams ProcessWindow(InputParams inputs, ref bool SkillIsPausable)
        {
            //This is the implementation of the "first" Method provided to the Skills (the "process method")

            //Remark : this method is quite complicated because it has to deal with windows and HMI, so it requires access to the UI thread.

            //Skill is currently not pausable
            SkillIsPausable = false;
            InputsForFakeWindow value;

            //First cast the inputs
            try
            {
                value = (InputsForFakeWindow)inputs.Values;
            }
            catch (System.InvalidCastException ex)
            {
                //If you can't cast the inputs, then there is a special error code for this
                return new OutputParams(Skill.ERROR_VALUES_PROCESS_INVALID, null);
            }

            //Then execute the code

            try
            {
                //Lets say skill is productive (even if it is not that true)
                inputs.IsProductive = true; 

                //Update remaining time
                value.RemainingTimeMs = value.ExecutionTimeMs;

                inputs.RemainingDuration = (UInt32)value.RemainingTimeMs;

                //First Ask (the UI thread) to pop a new window
                InputsForFakeWindow argsFirst = new InputsForFakeWindow()
                {
                    StartANewWindow = true,
                    RemainingTimeMs = value.RemainingTimeMs,
                    CloseTheWindow = false,
                    ExecutionTimeMs = value.ExecutionTimeMs,
                    NameOfSkill = value.NameOfSkill,
                    Status = "EXECUTING"
                };

                OnDisplayOnSkillWindow(argsFirst);


                //Then perform a loop, each second, after each second
                int subdivision = value.ExecutionTimeMs / 1000;

                //You can subscribe to the "Pausing" event, if you want to have notification of the Pausing.
                //The SEE mother class also subscribed to that event, to notify the OPC-UA about the state change
                inputs.Pausing += inputs_Pausing;

                if (subdivision >= 1)
                {
                    //SKill is now pausable => turn SkillIsPausable to true
                    SkillIsPausable = true;

                    //there are more than 1 second of execution time
                    for (int i = 0; i < subdivision; i++)
                    {
                        double remainingTime = (((double)value.ExecutionTimeMs) - i * 1000.0);

                        inputs.StopOrPauseIfRequired(); //You can use this line of code to block or stop the execution, if a Stop or pause was required

                        InputsForFakeWindow argsLoop = new InputsForFakeWindow()
                        {
                            StartANewWindow = false,
                            RemainingTimeMs = (int)remainingTime,
                            CloseTheWindow = false,
                            ExecutionTimeMs = value.ExecutionTimeMs,
                            NameOfSkill = value.NameOfSkill,
                            Status = "EXECUTING"
                        };

                        OnDisplayOnSkillWindow(argsLoop);

                        //Update remaining time before sleeping
                        inputs.RemainingDuration = (UInt32)remainingTime;

                        //Sleep 1s
                        System.Threading.Thread.Sleep(1000);

                        inputs.StopOrPauseIfRequired(); //You can use this line of code to block or stop the execution, if a Stop or pause was required

                    }

                    double LastValue = (((double)value.ExecutionTimeMs) - subdivision * 1000.0);

                    inputs.StopOrPauseIfRequired(); //You can use this line of code to block or stop the execution, if a Stop or pause was required

                    InputsForFakeWindow argsLastLoop = new InputsForFakeWindow()
                    {
                        StartANewWindow = false,
                        RemainingTimeMs = (int)LastValue,
                        CloseTheWindow = false,
                        ExecutionTimeMs = value.ExecutionTimeMs,
                        NameOfSkill = value.NameOfSkill,
                        Status = "EXECUTING"
                    };

                    OnDisplayOnSkillWindow(argsLastLoop);

                    //SKill is no longer pausable => turn SkillIsPausable to false
                    SkillIsPausable = false;
                    inputs.StopOrPauseIfRequired();
                    inputs.Pausing -= inputs_Pausing;

                    //Update remaining time before sleeping
                    inputs.RemainingDuration = (UInt32)LastValue;

                    //Sleep the remaining time
                    System.Threading.Thread.Sleep((int)LastValue);


                }
                else
                {
                    //there are Less than 1 second of execution time

                    //SKill is now pausable => turn SkillIsPausable to true
                    SkillIsPausable = true;
                    inputs.StopOrPauseIfRequired();//You can use this line of code to block or stop the execution, if a Stop or pause was required

                    InputsForFakeWindow argsLess1 = new InputsForFakeWindow()
                    {
                        StartANewWindow = false,
                        RemainingTimeMs = value.ExecutionTimeMs,
                        CloseTheWindow = false,
                        ExecutionTimeMs = value.ExecutionTimeMs,
                        NameOfSkill = value.NameOfSkill,
                        Status = "EXECUTING"
                    };

                    OnDisplayOnSkillWindow(argsLess1);

                    //SKill is no longer pausable => turn SkillIsPausable to false
                    SkillIsPausable = false;
                    inputs.StopOrPauseIfRequired();
                    inputs.Pausing -= inputs_Pausing;

                    //Update remaining time before sleeping
                    inputs.RemainingDuration = (UInt32)value.ExecutionTimeMs;

                    //Sleep
                    System.Threading.Thread.Sleep(value.ExecutionTimeMs);

                }


                //Display that skill execution is about to finish

                InputsForFakeWindow argsAboutToClose = new InputsForFakeWindow()
                {
                    StartANewWindow = false,
                    RemainingTimeMs = 0,
                    CloseTheWindow = false,
                    ExecutionTimeMs = value.ExecutionTimeMs,
                    NameOfSkill = value.NameOfSkill,
                    Status = "ABOUT TO FINISH : ANSWER QUESTION FIRST"
                };

                OnDisplayOnSkillWindow(argsAboutToClose);


                //Then finnaly, you have to prepare the "OutputParams".
                //These "OutputParams" then returned in the overriden method GetOutputsOfExecutedSkill
                //So take care of what you are providing

                OutputParams outp = InterativeAskReturnedCode(value, ERROR_DURING_EXECUTION);

                InputsForFakeWindow argsClose = new InputsForFakeWindow()
                {
                    StartANewWindow = false,
                    RemainingTimeMs = 0,
                    CloseTheWindow = false,
                    ExecutionTimeMs = value.ExecutionTimeMs,
                    NameOfSkill = value.NameOfSkill,
                    Status = "FINISHED"
                };

                OnDisplayOnSkillWindow(argsClose);

                //Check if you have an alternative post condition
                if (value.AlternativePostConditionAvailable && outp.ReturnCode == 0)
                {
                    //Ask user if he wants to go to alternative post condition
                    System.Windows.MessageBoxResult res = System.Windows.MessageBox.Show("Do you want to go to alternative post condition?", "Alternative", System.Windows.MessageBoxButton.YesNo);

                    switch (res)
                    {
                        case System.Windows.MessageBoxResult.Yes:
                            outp.GoToAlternativePostCondition = true;
                            break;
                        default:
                            outp.GoToAlternativePostCondition = false;
                            break;
                    }

                }

                //you don't have to Update remaining time to "0" => it is already done in the SEE mother class

                return outp;
            }
            catch (OperationCanceledException ex)
            {
                //External element asked for "emergency stop".
                //The "Closing Method" will be called (second method passed to the constructor of the skill)

                //You can do some memory freeing here.

                //And then, rethrow the exception
                throw;

            }
            catch (Exception ex)
            {
                return new OutputParams(ERROR_DURING_EXECUTION, null, false, new SkillProException(ex.Message, ex));
            }

        }

        void inputs_Pausing(object sender, EventArgs e)
        {
            InputsForFakeWindow argsPause= new InputsForFakeWindow()
            {
                StartANewWindow = false,
                RemainingTimeMs = -1,
                CloseTheWindow = false,
                ExecutionTimeMs = -1,
                NameOfSkill = "",
                Status = "PAUSED"
            };

            OnDisplayOnSkillWindow(argsPause);
        }

        private OutputParams CloseWindow(InputParams inputs, ref bool SkillIsPausable)
        {
            //This is the implementation of the "second" Method provided to the Skills (the "closing method")
            //Which was more or less designed to cancel/stop/quit an execution

            //Skill is currently not pausable, and will never be pausable => so let SkillIsPausable set to false
            SkillIsPausable = false;

            InputsForFakeWindow value;

            //First cast the inputs
            try
            {
                value = (InputsForFakeWindow)inputs.Values;
            }
            catch (System.InvalidCastException ex)
            {
                //If you can't cast the inputs, then there is a special error code for this
                return new OutputParams(Skill.ERROR_VALUES_CLOSING_INVALID, null);
            }

            try
            {
                //Ask the UI thread to close the window
                InputsForFakeWindow argsClose = new InputsForFakeWindow()
                {
                    StartANewWindow = false,
                    RemainingTimeMs = 0,
                    CloseTheWindow = false,
                    ExecutionTimeMs = value.ExecutionTimeMs,
                    NameOfSkill = value.NameOfSkill,
                    Status = "(EMERGENCY) STOP CALLED => EXECUTION FINISHED"
                };

                OnDisplayOnSkillWindow(argsClose);

                OutputParams outp = new OutputParams(ERROR_DURING_EXECUTION, null, false, new SkillProException("(EMERGENCY) STOP CALLED"));

                return outp;
            }
            catch (Exception ex)
            {
                return new OutputParams(ERROR_DURING_EXECUTION, null, false, new SkillProException(ex.Message, ex));
            }
        }

        private static OutputParams InterativeAskReturnedCode(InputsForFakeWindow value, int errorCode)
        {
            //Here ask the user the returned code.
            // YES : do as if the execution worked well
            //NO : simulate that there were an error in the process 
            System.Windows.MessageBoxResult res = System.Windows.MessageBox.Show("Press \"YES\" to return error code == 0 (no error during execution)" + Environment.NewLine + " or " + Environment.NewLine + "press \"NO\" to return error code == "+errorCode+" (simulate that an error occured during execution)", "Question", System.Windows.MessageBoxButton.YesNo);

            OutputParams outp;

            switch (res)
            {
                case System.Windows.MessageBoxResult.OK:
                    outp = new OutputParams(0, value);
                    break;
                case System.Windows.MessageBoxResult.Yes:
                    outp = new OutputParams(0, value);
                    break;
                default:
                    throw new Exception("User decided to return the error code " + errorCode + " by clicking on the \"NO\" button");
            }
            return outp;
        }


        private void OnDisplayOnSkillWindow(InputsForFakeWindow valuesToDisplay)
        {
            if (DisplayOnSkillWindow != null)
            {
                DisplayOnSkillWindow(this, valuesToDisplay);
            }
        }

    }

    /// <summary>
    /// The class containing the DATA you want to pass to the Skills. Indeed the method to execute uses class "InputParams" which has
    /// attribute "public object Values". So you can put whatever data in this attribute
    /// </summary>
    public class InputsForFakeWindow
    {
        //public WindowSkillExecution Window = null;
        public bool AlternativePostConditionAvailable = false;
        public string NameOfSkill = "UNINIT";
        public int ExecutionTimeMs = 2000;
        public int RemainingTimeMs = 2000;
        public string Status = "UNINIT";
        public bool StartANewWindow = false;
        public bool CloseTheWindow = false;
    }

}
