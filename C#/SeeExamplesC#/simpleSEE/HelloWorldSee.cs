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

namespace simpleSEE
{
    public class HelloWorldSee : See
    {
        
        public HelloWorldSee(AMLSkillExecutionEngine AmlSee, SkillBasedResourceController aSkillBasedResourceController)
            : base(AmlSee, aSkillBasedResourceController)
        {
            //Here, you just have to instanciate what belongs to your child class 
            //=> all the other "standard" (connections to OPC-UA, AMS etc) is done in the SEE mother class

            //In our case, we don't have any additionnal fields to set/instanciate
        }

        protected override void ProvideSkillToExecute(ref ExecutableSkill skillWithNoMethod)
        {
            //Note that skillWithNoMethod is tested before (in mother class), so it will NOT be null,  
            // and skillWithNoMethod.AmlSkillDescription.Execution.Type is also NOT NULL

            //Here, do a big switch case on the Execution's type of the skill.
            //In our case, all the skills will be the "MethodHelloWorld" (console write)

            switch (skillWithNoMethod.AmlSkillDescription.Execution.Type)
            {
                default:
                    //In our case, whatever the skill type, we will execute the same method
                    skillWithNoMethod.SkillToExecute = new Skill(MethodHelloWorld, MethodStop);
                    break;
            }

            //You can throw an exeception if something does not suits you => the SEE will NOT execute
            //the skill and will go in error state
        }

        protected override int ProvideInputDataForSkillExecution(ExecutableSkill SkillToExecute, out InputParams ParametersForTheSkill)
        {
            //Note that SkillToExecute is tested before (in mother class), so it will NOT be null,  
            // and SkillToExecute.AmlSkillDescription.Execution.Type is also NOT NULL

            //Here, do a big switch case on the Execution's type of the skill.
            //In our case, since we allways execute the "MethodHelloWorld" skill (console write), we always have the same input

            switch (SkillToExecute.AmlSkillDescription.Execution.Type)
            {
                default:
                    //In our case, whatever the skill type, we will execute the same method
                    DateTime Now = DateTime.Now;
                    string message = "Hello world ! Executing Skill " + SkillToExecute.AmlSkillDescription.ID + ". Time is " + Now.ToString("yyyy_MM_dd_hh_mm_ss_") + Now.Millisecond.ToString();
                    ParametersForTheSkill = new InputParams(0, (object)message);
                    break;
            }

            //You can throw an exeception or return !=0 if something does not suits you => the SEE will NOT execute
            //the skill and will go in error state

            return 0;
        }

        protected override void GetOutputsOfExecutedSkill(ExecutableSkill SkillExecuted, OutputParams OutputOfExecution)
        {
            //Here you will receive the outputs of the executed skills (in our case, the outputs of "MethodHelloWorld" or "MethodStop"

            //So here you will do everything required specifically to your SEE 
            //like updating a database, displaying something on a human/machine interface, logging, beeping...

            //but you can also do nothing, if you don't care about the outputs, or maybe you did everyhting in the Skill methods allready

            //Here, do a big switch case on the Execution's type of the skill.
            //In our case, just print the returned code of skill

            switch (SkillExecuted.AmlSkillDescription.Execution.Type)
            {
                default:
                    //In our case, whatever the skill type, we will execute the same method
                    Console.WriteLine("==========================================================================================================");
                    Console.WriteLine("Returned code of skill " + SkillExecuted.AmlSkillDescription.ID + " is " + OutputOfExecution.ReturnCode);
                    Console.WriteLine("==========================================================================================================");
                    break;
            }

            //You can throw an exeception if something does not suits you => the SEE will go in error state

        }

        private OutputParams MethodHelloWorld(InputParams inputs, ref bool SkillIsPausable)
        {
            SkillIsPausable = false;
            string value;

            //First cast the inputs
            try
            {
                value = (string)inputs.Values;
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

                //Let's say the skill needs 2ms to execute
                inputs.RemainingDuration = 2;
                
                //<body> Here is the "body" of you method

                Console.WriteLine("==========================================================================================================");
                Console.WriteLine(value);
                Console.WriteLine("==========================================================================================================");

                //</body>

                //you don't have to Update remaining time to "0" => it is already done in the SEE mother class

                //Take care of the values you give to your outputs params. In our case, we ust forward what we received
                return new OutputParams(0, (object)value);
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

            //Skill is currently not pausable. In most cases, the skill to "stop" (or "close")
            //will never be pausable => so let SkillIsPausable set to false
            SkillIsPausable = false;

            //The skill is not productive. In most cases, the skill to "stop" (or "close")
            //will never be productive => so let inputs.IsProductive set to false
            inputs.IsProductive = false;

            //No need to cast the inputs : I don't need them 
            //try
            //{
            //    //I don't need the inputs
            //}
            //catch (System.InvalidCastException ex)
            //{
            //    //If you can't cast the inputs, then there is a special error code for this
            //    return new OutputParams(Skill.ERROR_VALUES_CLOSING_INVALID, null);
            //}

            try
            {


                //<body> Here is the "body" of you method

                Console.WriteLine("==========================================================================================================");
                Console.WriteLine("MethodStop");
                Console.WriteLine("==========================================================================================================");

                //</body>

                //Your user code, which starts at -300
                return new OutputParams(-302, null);
            }
            catch (Exception ex)
            {
                //Your user code, which starts at -300
                return new OutputParams(-302, null, false, new SkillProException(ex.Message, ex));
            }
        }

    }
}
