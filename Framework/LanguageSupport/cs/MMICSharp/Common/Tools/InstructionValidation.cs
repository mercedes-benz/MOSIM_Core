// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;
using System.Linq;
namespace MMICSharp.Common.Tools
{
    public class InstructionValidation
    {

        /// <summary>
        /// List of validation functions that can be used.
        /// By default the constraints and required parameters are investigated
        /// </summary>
        public List<Func<MInstruction, MMUDescription, MBoolResponse>> ValidationFunctions = new List<Func<MInstruction, MMUDescription, MBoolResponse>>();


        /// <summary>
        /// Basic constructor
        /// </summary>
        public InstructionValidation()
        {
            this.ValidationFunctions.Add(this.ValidateParameters);
        }

        /// <summary>
        /// Validates whether the instruction is correct regarding the available MMUs
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="availableMMUs"></param>
        /// <returns></returns>
        public virtual MBoolResponse Validate(MInstruction instruction, List<MMUDescription> availableMMUs)
        {
            //Check if the motion type is present
            List<MMUDescription> matchingDescriptions = availableMMUs.Where(s => s.MotionType == instruction.MotionType).ToList();

            if(matchingDescriptions.Count == 0)
            {
                return new MBoolResponse(false)
                {
                    LogData = new List<string>()
                     {
                         "No matching MMU available (motionType)"
                     }
                };
            }

            //Create a new container to provide the result
            MBoolResponse result = new MBoolResponse(true)
            {
                LogData = new List<string>()
            };

            //Next check the parameters
            foreach (MMUDescription description in matchingDescriptions)
            {
                foreach(var validationFunction in this.ValidationFunctions)
                {
                    MBoolResponse currentResult = validationFunction(instruction, description);

                    //Invalid instruction
                    if (!currentResult.Successful)
                    { 
                        //Set successful to false
                        result.Successful = false;

                        //Add the log data
                        result.LogData.AddRange(currentResult.LogData);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Validates the parameters of a particular instruction
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        protected virtual MBoolResponse ValidateParameters(MInstruction instruction, MMUDescription description)
        {
            foreach (MParameter parameter in description.Parameters)
            {
                if (parameter.Required)
                {
                    if (!instruction.Properties.ContainsKey(parameter.Name))
                    {
                        return new MBoolResponse(false)
                        {
                            LogData = new List<string>() { "Required parameter: " + parameter.Name + " not set." }
                        };
                    }
                }

                //Check if parameter is constraint
                if (IsConstraintType(parameter.Type))
                {
                    if (instruction.Properties.ContainsKey(parameter.Name))
                    {
                        string id = instruction.Properties[parameter.Name];

                        //Check if a constraint with the given id is available

                        if (instruction.Constraints == null)
                            return new MBoolResponse(false)
                            {
                                LogData = new List<string>() { "Constraints are null:" + parameter.Name }
                            };

                        if (!instruction.Constraints.Exists(s => s.ID == id))
                        {
                            return new MBoolResponse(false)
                            {
                                LogData = new List<string>() { "Constraint with requested id not defined: " + parameter.Name + " " + id }
                            };
                        }
                    }
                }
            }

            if (instruction.Properties != null)
            {
                foreach (KeyValuePair<string, string> entry in instruction.Properties)
                {
                    if (entry.Key == null)
                        return new MBoolResponse(false)
                        {
                            LogData = new List<string>() { "Parameter with key == null existent." }
                        };
                    if(entry.Value == null)
                    {
                        return new MBoolResponse(false)
                        {
                            LogData = new List<string>() { $"Parameter {entry.Key} with value == null existent." }
                        };
                    }
                }
            }

            return new MBoolResponse(true);
        }


        /// <summary>
        /// Checks whether the parameter type is of type constraint
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual bool IsConstraintType(string type)
        {

            switch (type)
            {
                case "MJointConstraint":
                    return true;

                case "MPostureConstraint":
                    return true;

                case "MGeometryConstraint":
                    return true;

                case "MPathConstraint":
                    return true;

                case "MJointPathConstraint":
                    return true;
            }

            return false;

        }


    }
}
