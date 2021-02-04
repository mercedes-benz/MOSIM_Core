// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICoSimulation;
using MMICSharp.Access;
using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMICSharp.MMIStandard.Utils;
using MMIStandard;
using System;
using System.Collections.Generic;


namespace CarryMMUNested
{
    /// <summary>
    ///Implementation of a carry MMU which internally utilizes a co-simulation.
    ///The MMU useses the other MMU "move" in order to simulate a carry behavior.
    ///Postbuild: "../../../MMUDescriptionAutoGenerator\bin\Debug\MMUDescriptionAutoGenerator.exe" "$(TargetDir)$(ProjectName).dll"
    /// </summary>
    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "CarryMMUNested", "Object/Carry/carryNested", "","A carry MMU", "A carry MMU realized as nested co-simulation")]
    public class CarryMMUNestedImpl:MMUBase
    {
        #region private fields

        /// <summary>
        /// The utilizied CoSimulator
        /// </summary>
        private MMICoSimulator coSimulator;

        /// <summary>
        /// Flag which indicates whether the full scene needs to be transferred
        /// </summary>
        private bool transmitFullScene = true;

        /// <summary>
        /// The unique session id
        /// </summary>
        private string sessionId;

        /// <summary>
        /// The MMU Access
        /// </summary>
        private MMUAccess mmuAccess;

        /// <summary>
        /// The carry object
        /// </summary>
        private MSceneObject carryObject;

        /// <summary>
        /// The virtual move target
        /// </summary>
        private MSceneObject moveTarget;

        /// <summary>
        /// Representation of a virtual scene
        /// </summary>
        private MMIScene virtualScene;

        private string currentInstructionID;

        private MInstruction instruction;

        #endregion


        /// <summary>
        /// Basic initialization method
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {
            //Call the base class (important for automatic conversion from MSimulationState to SimulationState)
            base.Initialize(avatarDescription, properties);

            this.SkeletonAccess = new IntermediateSkeleton();
            this.SkeletonAccess.InitializeAnthropometry(avatarDescription);


            //Create a new session uuid
            this.sessionId = Guid.NewGuid().ToString();

            //Create a new virtual scene
            this.virtualScene = new MMIScene();

            //Create a unique id for the new virtual object representing the move target
            string moveTargetID = Guid.NewGuid().ToString();

            //Create a new virtual object representing the move target
            moveTarget = new MSceneObject(moveTargetID, "MoveTarget", new MTransform()
            {
                ID = moveTargetID,
                Position = new MVector3(0, 0, 0),
                Rotation = new MQuaternion(0, 0, 0, 1)
            });

            //Add the virtual move target to the scene
            this.virtualScene.Apply(new MSceneUpdate()
            {
                AddedSceneObjects = new List<MSceneObject>()
                 {
                     moveTarget
                 }
            });


            //Full scene transmission initial required
            this.transmitFullScene = true;

            //Setup the mmu access
            this.mmuAccess = new MMUAccess(this.sessionId)
            {         
                //IntermediateAvatarDescription = avatarDescription,
                SceneAccess = this.virtualScene
            };

            Console.WriteLine("Try to connect to mmu access...");

            //Connect to mmu access and load mmus
            bool connected = this.mmuAccess.Connect(this.AdapterEndpoint, this.AvatarDescription.AvatarID);


            if (connected)
            {
                //Get all loadable MMUs within the current session
                List<MMUDescription> loadableMMUs = this.mmuAccess.GetLoadableMMUs();


                MMUDescription moveMMU = loadableMMUs.Find(s => s.MotionType == "move");

                Console.WriteLine("Got loadable MMUs:");


                //Load the relevant MMUs
                bool loaded = this.mmuAccess.LoadMMUs(new List<MMUDescription>() { moveMMU}, TimeSpan.FromSeconds(10));

                if (!loaded)
                {
                    Console.WriteLine("Error at loading MMU");

                    return new MBoolResponse(false)
                    {
                        LogData = new List<string>()
                         {
                            { "Error at loading mmu" }
                         }
                    };
                }

                //Initialize all MMUs
                this.mmuAccess.InitializeMMUs(TimeSpan.FromSeconds(10),this.AvatarDescription.AvatarID);

                //Instantiate the cosimulator
                this.coSimulator = new MMICoSimulator(mmuAccess.MotionModelUnits);


                return new MBoolResponse(true);

            }
            else
            {
                Console.WriteLine("Connection to MMUAccess/MMIRegister failed");
                return new MBoolResponse(false)
                {
                    LogData = new List<string>() { "Connection to MMUAccess/MMIRegister failed" }
                };
            }
        }


        /// <summary>
        /// Basic assign instruction mehtod
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        [MParameterAttribute("TargetID", "ID", "The id of the object which should be carried", true)]
        [MParameterAttribute("Hand", "{Left,Right}", "The hand of the carry motion", true)]
        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            this.instruction = instruction;

            //Get the carry object (if available)
            this.carryObject = this.SceneAccess.GetSceneObjectByID(instruction.Properties["TargetID"]);


            //Add the carry object to the virtual scene
            this.virtualScene.Apply(new MSceneUpdate()
            {
                AddedSceneObjects = new List<MSceneObject>()
                 {
                     carryObject
                 }
            },true);

            //Create a new id for the instruction
            this.currentInstructionID = MInstructionFactory.GenerateID();

            //Create a new subinstruction utilizing the  moving target
            MInstruction subInstruction = new MInstruction(currentInstructionID, "NestedMove", "move")
            {
                Properties = PropertiesCreator.Create("TargetID", moveTarget.ID, "SubjectID", instruction.Properties["TargetID"], "Hand", instruction.Properties["Hand"])
            };

            //Assign the instruction at the co-simulation and create a new wrapper instruction
            return this.coSimulator.AssignInstruction(subInstruction, simulationState);

        }


        /// <summary>
        /// Do step routine which performs the actual computation of the postures
        /// </summary>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        [MSimulationEventAttribute("PositioningFinished", "PositioningFinished")]
        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            //Set the position of the move target
            this.SkeletonAccess.SetChannelData(simulationState.Current);
            this.moveTarget.Transform.Position = this.SkeletonAccess.GetGlobalJointPosition(simulationState.Current.AvatarID, MJointType.RightWrist);


            //Update the scene objects within the virtual scene
            this.virtualScene.Apply(new MSceneUpdate()
            {
                //Add the changed objects
                ChangedSceneObjects = new List<MSceneObjectUpdate>()
                {
                    //Add the move target (just existing in the local co-simulation)
                    new MSceneObjectUpdate()
                    {
                        ID = this.moveTarget.ID,
                         Transform = new MTransformUpdate()
                         {
                              Position = this.moveTarget.Transform.Position.GetValues(),
                              Rotation = this.moveTarget.Transform.Rotation.GetValues()
                         }
                    },
                    //Add the carry object (externally modified)
                    new MSceneObjectUpdate()
                    {
                        ID = this.carryObject.ID,
                         Transform = new MTransformUpdate()
                         {
                              Position = this.carryObject.Transform.Position.GetValues(),
                              Rotation = this.carryObject.Transform.Rotation.GetValues()
                         }
                    },
                }
            });


            //Transmit the  virtual scene (if first frame-> transmit full scene otherwise just deltas)
            this.mmuAccess.PushScene(this.transmitFullScene);

            //Full transmission only required at first frame
            this.transmitFullScene = false;

            //Execute the co-simulation
            MSimulationResult result = this.coSimulator.DoStep(time, simulationState);

            //Check if the present instruction is finished
            if(result.Events !=null && result.Events.Count > 0)
            {
                if(result.Events.Exists(s=>s.Reference == this.currentInstructionID && s.Type == mmiConstants.MSimulationEvent_End))
                {

                    //Create a new id for the instruction
                    this.currentInstructionID = MInstructionFactory.GenerateID();

                    //Create a new subinstruction utilizing the  moving target
                    MInstruction subInstruction = new MInstruction(currentInstructionID, "NestedMove", "move")
                    {
                        Properties = PropertiesCreator.Create("TargetID", moveTarget.ID, "SubjectID", instruction.Properties["TargetID"], "Hand", instruction.Properties["Hand"])
                    };

                    //Assign the new instruction to the co-simulator
                    this.coSimulator.AssignInstruction(subInstruction, simulationState);
                }
            }

            return result;
        }

        public override MBoolResponse Abort(string instructionID = null)
        {
            //Dispose the mmu access
            this.mmuAccess.Dispose();

            return base.Abort(instructionID);
        }
    }


    

}
