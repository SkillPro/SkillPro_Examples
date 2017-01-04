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
using System.Web.Script.Serialization;

namespace simpleSEE
{
    public class HelloWorldAndQuestionsSee : See
    {
        public static JavaScriptSerializer JSS = new JavaScriptSerializer();

        public HelloWorldAndQuestionsSee(AMLSkillExecutionEngine AmlSee, SkillBasedResourceController aSkillBasedResourceController)
            : base(AmlSee, aSkillBasedResourceController)
        {
        }

        protected override void ProvideSkillToExecute(ref ExecutableSkill skillWithNoMethod)
        {
            switch (skillWithNoMethod.AmlSkillDescription.Execution.Type)
            {
                default:
                    //In our case, whatever the skill type, we will execute the same method
                    skillWithNoMethod.SkillToExecute = new Skill(MethodHelloWorldAndQuestions, MethodStop);
                    break;
            }
        }


        protected override int ProvideInputDataForSkillExecution(ExecutableSkill SkillToExecute, out InputParams ParametersForTheSkill)
        {

            switch (SkillToExecute.AmlSkillDescription.Execution.Type)
            {
                default:
                    DateTime Now = DateTime.Now;
                    ValuesForSee v = new ValuesForSee()
                    {
                        MessageToDisplay = "Hello world ! Executing Skill " + SkillToExecute.AmlSkillDescription.ID + ". Time is " + Now.ToString("yyyy_MM_dd_hh_mm_ss_") + Now.Millisecond.ToString(),
                        AlternativePostConditionAvailable = SkillToExecute.AmlSkillDescription.AltPostCondition != null
                    };
                    ParametersForTheSkill = new InputParams(0, (object)v);
                    break;
            }

            return 0;
        }

        protected override void GetOutputsOfExecutedSkill(ExecutableSkill SkillExecuted, OutputParams OutputOfExecution)
        {
            switch (SkillExecuted.AmlSkillDescription.Execution.Type)
            {
                default:
                    //In our case, whatever the skill type, we will execute the same method
                    Console.WriteLine("==========================================================================================================");
                    Console.WriteLine("Returned code of skill " + SkillExecuted.AmlSkillDescription.ID + " is " + OutputOfExecution.ReturnCode);
                    Console.WriteLine("==========================================================================================================");
                    break;
            }

        }


        private OutputParams MethodHelloWorldAndQuestions(InputParams inputs, ref bool SkillIsPausable)
        {
            SkillIsPausable = false;
            ValuesForSee value;

            //First cast the inputs
            try
            {
                value = (ValuesForSee)inputs.Values;
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

                //Let's say the skill needs 3000ms to execute (because of the question)
                inputs.RemainingDuration = 3000;

                Console.WriteLine("==========================================================================================================");
                Console.WriteLine(inputs.ToString());
                Console.WriteLine("==========================================================================================================");

                OutputParams outputs = InterativeAskReturnedCode(inputs, -301);

                                //Check if you have an alternative post condition
                if (value.AlternativePostConditionAvailable && outputs.ReturnCode == 0)
                {
                    //Ask user if he wants to go to alternative post condition
                    System.Windows.MessageBoxResult res = System.Windows.MessageBox.Show("Do you want to go to alternative post condition?", "Alternative", System.Windows.MessageBoxButton.YesNo);

                    switch (res)
                    {
                        case System.Windows.MessageBoxResult.Yes:
                            outputs.GoToAlternativePostCondition = true;
                            break;
                        default:
                            outputs.GoToAlternativePostCondition = false;
                            break;
                    }

                }

                //you don't have to Update remaining time to "0" => it is already done in the SEE mother class

                return outputs;

            }
            catch (OperationCanceledException ex)
            {
                //External element asked for "emergency stop".
                //The "Closing Method" will be called (second method passed to the constructor of the skill)

                //You can do some memory freeing here.
                Console.WriteLine("==========================================================================================================");
                Console.WriteLine("emergency stop");
                Console.WriteLine("==========================================================================================================");


                //And then, rethrow the exception
                throw;

            }
            catch (Exception ex)
            {
                //Your user code, which starts at -300
                return new OutputParams(-301, null, false, new SkillProException(ex.Message, ex));
            }

        }

        private OutputParams MethodStop(InputParams inputs, ref bool SkillIsPausable)
        {
            //This is the implementation of the "second" Method provided to the Skills (the "closing method")
            //Which was more or less designed to cancel/stop/quit an execution

            //Skill is currently not pausable, and will never be pausable => so let SkillIsPausable set to false
            SkillIsPausable = false;

            //First cast the inputs
            try
            {
                //I don't need the inputs
            }
            catch (System.InvalidCastException ex)
            {
                //If you can't cast the inputs, then there is a special error code for this
                return new OutputParams(Skill.ERROR_VALUES_CLOSING_INVALID, null);
            }

            try
            {

                Console.WriteLine("MethodStop");

                //Your user code, which starts at -300
                return new OutputParams(-302, null);
            }
            catch (Exception ex)
            {
                //Your user code, which starts at -300
                return new OutputParams(-302, null, false, new SkillProException(ex.Message, ex));
            }
        }

        private static OutputParams InterativeAskReturnedCode(InputParams value, int errorCode)
        {
            //Here ask the user the returned code.
            // YES : do as if the execution worked well
            //NO : simulate that there were an error in the process 
            System.Windows.MessageBoxResult res = System.Windows.MessageBox.Show("Press \"YES\" to return error code == 0 (no error during execution)" + Environment.NewLine + " or " + Environment.NewLine + "press \"NO\" to return error code == " + errorCode + " (simulate that an error occured during execution)", "Question", System.Windows.MessageBoxButton.YesNo);

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
    }

    public class ValuesForSee
    {
        public string MessageToDisplay = "";
        public bool AlternativePostConditionAvailable = false;

        public override string ToString()
        {
            return HelloWorldAndQuestionsSee.JSS.Serialize(this);
        }

    }


}
