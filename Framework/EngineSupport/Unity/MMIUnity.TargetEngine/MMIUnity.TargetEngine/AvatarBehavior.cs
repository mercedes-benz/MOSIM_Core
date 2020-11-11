// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICoSimulation;
using MMICSharp.MMIStandard.Utils;
using MMIStandard;
using MMIUnity.TargetEngine.Scene;
using System.Collections.Generic;
using UnityEngine;

namespace MMIUnity.TargetEngine
{
    /// <summary>
    /// The class is responsible for controlling the behavior of the avatar.
    /// The class be optionally replaced in future by a more sophisticated reasoning approach.
    /// </summary>
    public class AvatarBehavior : MonoBehaviour
    {
        /// <summary>
        /// The corresponding avatar
        /// </summary>
        protected MMIAvatar avatar;

        /// <summary>
        /// The co-simulator
        /// </summary>
        protected MMICoSimulator CoSimulator
        {
            get
            {
                return this.avatar.CoSimulator;
            }
        }


        // Use this for initialization
        protected virtual void Start()
        {
            this.avatar = this.GetComponent<MMIAvatar>();
        }


        /// <summary>
        /// Function is called to control the avatar by using predefined buttons in the GUI.
        /// </summary>
        protected virtual void GUIBehaviorInput()
        {
            if (GUI.Button(new Rect(10, 10, 120, 50), "Idle"))
            {
                MInstruction instruction = new MInstruction(MInstructionFactory.GenerateID(), "Idle", "Pose/Idle");
                //MInstruction instruction = new MInstruction(MInstructionFactory.GenerateID(), "MMUTest", "move");
                MSimulationState simstate = new MSimulationState(this.avatar.GetPosture(), this.avatar.GetPosture());

                this.CoSimulator.Abort();
                this.CoSimulator.AssignInstruction(instruction, simstate);
            }


            if (GUI.Button(new Rect(140, 10, 120, 50), "Walk to"))
            {
                MInstruction walkInstruction = new MInstruction(MInstructionFactory.GenerateID(), "Walk", "Locomotion/Walk")
                {
                    Properties = PropertiesCreator.Create("TargetID", UnitySceneAccess.Instance.GetSceneObjectByName("WalkTarget").ID)
                };

                MInstruction idleInstruction = new MInstruction(MInstructionFactory.GenerateID(), "Idle", "Pose/Idle")
                {
                    //Start idle after walk has been finished
                    StartCondition = walkInstruction.ID + ":" + mmiConstants.MSimulationEvent_End //synchronization constraint similar to bml "id:End"  (bml original: <bml start="id:End"/>
                };

                this.CoSimulator.Abort();


                MSimulationState currentState = new MSimulationState() { Initial = this.avatar.GetPosture(), Current = this.avatar.GetPosture() };

                //Assign walk and idle instruction
                this.CoSimulator.AssignInstruction(walkInstruction, currentState);
                this.CoSimulator.AssignInstruction(idleInstruction, currentState);
                this.CoSimulator.MSimulationEventHandler += this.CoSimulator_MSimulationEventHandler;
            }

        }


        /// <summary>
        /// Callback for the co-simulation event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoSimulator_MSimulationEventHandler(object sender, MSimulationEvent e)
        {
            Debug.Log(e.Reference + " " + e.Name + " " + e.Type);
        }


        /// <summary>
        /// Basic on gui routine which is executed for each frame on the main thread
        /// </summary>
        protected void OnGUI()
        {
            //Skip if no co-simulation or MMU access of avatar not initialized
            if (this.CoSimulator == null || !this.avatar.MMUAccess.IsInitialized)
                return;

            //Call for each frame
            this.GUIBehaviorInput();

        }

        /// <summary>
        /// Basic update routine
        /// </summary>
        void Update()
        {
            ///Handle the walk command on mouse click
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
            {

                Vector3 mousePos = Input.mousePosition;


                Ray mouseRay = Camera.main.ScreenPointToRay(mousePos);
                RaycastHit hit = new RaycastHit();

                if (Physics.Raycast(mouseRay, out hit, 10000))
                {
                    //Ray for visual guide from camera to mouse position.
                    Debug.DrawRay(mouseRay.origin, mouseRay.direction * hit.distance, Color.red, 1);

                    GameObject walkTarget = GameObject.Find("WalkTarget");
                    walkTarget.transform.position = new Vector3(hit.point.x, walkTarget.transform.position.y, hit.point.z);
                    walkTarget.GetComponent<MMISceneObject>().UpdateTransform();


                    MInstruction walkInstruction = new MInstruction(MInstructionFactory.GenerateID(), "Walk", "Locomotion/Walk")
                    {
                        Properties = PropertiesCreator.Create("TargetName", "WalkTarget", "UseTargetOrientation", false.ToString())
                    };

                    MInstruction idleInstruction = new MInstruction(MInstructionFactory.GenerateID(), "Idle", "Pose/Idle")
                    {
                        //Start idle after walk has been finished
                        StartCondition = walkInstruction.ID + ":" + mmiConstants.MSimulationEvent_End //synchronization constraint similar to bml "id:End"  (bml original: <bml start="id:End"/>
                    };

                    //Abort all current tasks
                    this.CoSimulator.Abort();

                    MSimulationState currentState = new MSimulationState() { Initial = this.avatar.GetPosture(), Current = this.avatar.GetPosture() };

                    //Assign walk and idle instruction
                    this.CoSimulator.AssignInstruction(walkInstruction, currentState);
                    this.CoSimulator.AssignInstruction(idleInstruction, currentState);

                }

            }

        }
    }

    public static class CoSimExtensions
    {
        public static void AddAction(this MInstruction instruction, string topic, string action, string instructionID)
        {
            if (instruction.Properties == null)
                instruction.Properties = new Dictionary<string, string>();

            instruction.Properties.Add(topic, instructionID + ":" + action);
        }
    }
}
