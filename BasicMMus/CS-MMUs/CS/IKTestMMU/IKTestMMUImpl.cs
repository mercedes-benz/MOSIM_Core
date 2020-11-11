// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMIStandard;
using System;
using System.Collections.Generic;

namespace IKTestMMU
{
    /// <summary>
    /// Class used for debugging the MMU
    /// </summary>
    class Debug
    {
        static void Main(string[] args)
        {
            using (var debugAdapter = new DebugAdapter.DebugAdapter(typeof(IKTestMMUImpl)))
            {
                Console.ReadLine();
            }
        }
    }

    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "IKTestMMU", "Test/IK", "", "A move MMU which models both, one handed and both handed motions.", "A move MMU realized using inverse kinematics. It moreover supports concurrent motions.")]

    public class IKTestMMUImpl : MMUBase
    {
        public MSceneObject LeftHandTarget;
        public MSceneObject RightHandTarget;


        public IKTestMMUImpl()
        {
        }

        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            base.AssignInstruction(instruction, simulationState);


            if (instruction.Properties.ContainsKey("leftTarget"))
                this.LeftHandTarget = this.SceneAccess.GetSceneObjectByID(instruction.Properties["leftTarget"]);

            if (instruction.Properties.ContainsKey("rightTarget"))
                this.RightHandTarget = this.SceneAccess.GetSceneObjectByID(instruction.Properties["rightTarget"]);

            return new MBoolResponse(true);
        }

        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            //Create a new result
            MSimulationResult result = new MSimulationResult()
            {
                Events = simulationState.Events ?? new List<MSimulationEvent>(),
                DrawingCalls = new List<MDrawingCall>(),
                SceneManipulations = simulationState.SceneManipulations ?? new List<MSceneManipulation>(),
                Posture = simulationState.Current,
                Constraints = simulationState.Constraints ?? new List<MConstraint>()
            };

            List<MConstraint> constraints = new List<MConstraint>();


            //Apply ik
            if (LeftHandTarget != null)
            {
                constraints.Add(new MConstraint(System.Guid.NewGuid().ToString())
                {
                    JointConstraint = new MJointConstraint()
                    {
                        GeometryConstraint = new MGeometryConstraint("")
                        {
                            ParentToConstraint = new MTransform(System.Guid.NewGuid().ToString(), LeftHandTarget.Transform.Position, LeftHandTarget.Transform.Rotation),
                            WeightingFactor = 1.0f,
                        },
                        JointType = MJointType.LeftWrist
                    }
                });
            }

            if(RightHandTarget != null)
            {
                constraints.Add(new MConstraint(System.Guid.NewGuid().ToString())
                {
                    JointConstraint = new MJointConstraint()
                    {
                        GeometryConstraint = new MGeometryConstraint("")
                        {
                            ParentToConstraint = new MTransform(System.Guid.NewGuid().ToString(), RightHandTarget.Transform.Position, RightHandTarget.Transform.Rotation),
                            WeightingFactor = 1.0f
                        },
                        JointType = MJointType.RightWrist

                    },
                });
            }

            if (constraints.Count > 0)
            {
                MIKServiceResult ikResult = this.ServiceAccess.IKService.CalculateIKPosture(simulationState.Current, constraints, new Dictionary<string, string>());
                result.Posture = ikResult.Posture;
            }

            return result;
        }
    }
}
