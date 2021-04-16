using MMICoSimulation;
using MMICSharp.Common.Attributes;
using MMIStandard;
using System.Collections.Generic;

namespace PickUpMMU
{
    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "PickUPMMU", "Object/PickUP", "", "MMU allows to manipulate the finger joints by means of motion blending.", "MMU allows to manipulate the finger joints by means of motion blending.")]
    public class PickUpMUUImpl : NestedMMUBase
    {
        public PickUpMUUImpl()
        {
        }

        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {

            //Add the required motion types and its corresponding priority
            properties.Add("Pose/Idle", "1");
            properties.Add("Pose/Reach", "2");
            properties.Add("Object/Carry", "2");
            properties.Add("Object/Grasp", "3");


        
            return  base.Initialize(avatarDescription, properties);
        }


        /// <summary>
        /// Basic assign instruction method
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        [MParameterAttribute("TargetID", "ID", "The id of the target location (object) or MGeometryConstraint", true)]
        [MParameterAttribute("Hand", "{Left,Right}", "The hand of the reach motion", true)]
        [MParameterAttribute("HandPose", "PostureConstraint", "The desired hand pose, joint constraints of the finger tips.", true)]
        [MParameterAttribute("UseGlobalCoordinates", "bool", "Specified whether the global coordinates of the fingers are used for establishing the hand pose (by default true).", false)]
        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {

            MInstruction idleInstruction = new MInstruction(MInstructionFactory.GenerateID(), "Idle", "Pose/Idle");

            MInstruction reachInstruction = new MInstruction(MInstructionFactory.GenerateID(), "Reach", "Pose/Reach");


            MInstruction graspInstruction = new MInstruction(MInstructionFactory.GenerateID(), "Grasp", "Object/Grasp")
            {
                StartCondition = reachInstruction.ID + ":" + mmiConstants.MSimulationEvent_End
            };


            MInstruction carryInstruction = new MInstruction(MInstructionFactory.GenerateID(), "Carry", "Object/Carry")
            {
                StartCondition = graspInstruction.ID + ":" + mmiConstants.MSimulationEvent_End
            };


            this.coSimulator.AssignInstruction(idleInstruction, simulationState);
            this.coSimulator.AssignInstruction(reachInstruction, simulationState);
            this.coSimulator.AssignInstruction(graspInstruction, simulationState);
            this.coSimulator.AssignInstruction(carryInstruction, simulationState);


            return new MBoolResponse(false);

        }

        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            //To do -> Rewire the respective events to the presently active instruction

            return base.DoStep(time, simulationState);
        }
    }
}
