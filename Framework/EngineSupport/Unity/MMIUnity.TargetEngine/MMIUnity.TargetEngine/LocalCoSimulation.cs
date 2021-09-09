// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICoSimulation;
using MMICoSimulation.Internal;
using MMICoSimulation.Solvers;
using MMICSharp.Access;
using MMICSharp.Common;
using MMIStandard;
using MMIUnity.TargetEngine.Scene;
using System.Collections.Generic;
using UnityEngine;

namespace MMIUnity.TargetEngine
{
    public class LocalCoSimulation : MMICoSimulator
    {
        /// <summary>
        /// Flag which specifies whether the co-simulation is computed asynchronously
        /// </summary>
        public bool ComputeAsync = false;


        #region private variables


        /// <summary>
        /// List contains temporary drawings that should be deactivated at the next frame
        /// </summary>
        private readonly List<GameObject> temporaryDrawings = new List<GameObject>();
        private bool resetThisFrame = false;

        /// <summary>
        /// The referenced avatar
        /// </summary>
        private UnityAvatarBase avatar;


        /// <summary>
        /// The service access 
        /// </summary>
        private IServiceAccess serviceAccess;

        #endregion


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="mmus"></param>
        /// <param name="avatar"></param>
        public LocalCoSimulation(List<IMotionModelUnitAccess> mmus, IServiceAccess serviceAccess, MMIAvatar avatar) : base(mmus)
        {
            this.avatar = avatar;
            this.serviceAccess = serviceAccess;
            this.HierachicalSolving = false;

            this.priorities = new Dictionary<string, float>()
            {
                {"Default", -1 },


                //Level 0
                {"Pose", 0 },
                {"Pose/Idle", 0 },
                {"idle", 0 },

                //Level 1
                {"Locomotion", 1 },
                {"Locomotion/Walk", 1 },
                {"Locomotion/Run", 1 },
                {"Locomotion/Jog", 1 },
                {"Locomotion/Crouch", 1 },
                {"Locomotion/Turn", 1 },
                {"walk", 1 },

                //Level 2
                {"Pose/Reach",2},
                {"positionObject",2},
                {"releaseObject",2},

                {"Object/Release",2},
                {"Object/Carry",2},
                {"Object/Move",2},
                {"Object/Turn",2},

                {"release",2},
                {"carry",2},
                {"move",3},
                {"putDown",2},
                {"pickupMTM-SD",2},
                {"turn",2},
                {"UseTool",2},


                //Level 3
                {"Pose/MoveFingers",3},
                {"moveFingers",3},
                {"grasp",3},

                //Level 4
                {"Pose/Gaze",4},
                {"Pose/EyeTravel",4},

            };

            //Create and add the solvers
            this.Solvers = new List<ICoSimulationSolver>
            {
                new IKSolver(this.serviceAccess, avatar.GetSkeletonAccess(), avatar.MAvatar.ID),
                new LocalPostureSolver(avatar.GetSkeletonAccess())
            };

            this.SortMMUPriority();
        }


        /// <summary>
        /// Extract the posture information before calling the compute frame
        /// </summary>
        public override void PreComputeFrame()
        {
            if (!this.ComputeAsync)
            {
                //Only set the posture if null
                if(this.SimulationState.Initial == null)
                    this.SimulationState.Initial = this.avatar.GetPosture();

                //Only set the posture if null
                if (this.SimulationState.Current == null)
                    this.SimulationState.Current = this.avatar.GetPosture();
            }

            resetThisFrame = false;
        }

        /// <summary>
        /// Assign the posture after compute frame
        /// </summary>
        /// <param name="result"></param>
        public override void PostComputeFrame(MSimulationResult result)
        {
            if (!this.ComputeAsync)
            {
                this.avatar.AssignPostureValues(result.Posture);
            }
        }



        /// <summary>
        /// Method to handle the actual drawing calls
        /// </summary>
        /// <param name="mmuResult"></param>
        /// <param name="instance"></param>
        protected override void HandleDrawingCalls(MSimulationResult mmuResult, MMUContainer instance)
        {

            MainThreadDispatcher.Instance.ExecuteNonBlocking(() =>
            {


                try
                {
                    ////Draw the avatar
                    //if (instance.PostureDraw != null)
                    //{
                    //    GameObject.Destroy(instance.PostureDraw);
                    //}

                    //instance.PostureDraw = DrawingUtils.DrawAvatarPosture(mmuResult.Posture);
                    //instance.PostureDraw.name = instance.MMU.Name;

                    //Disable the history drawings
                    foreach (MotionTask task in instance.History)
                    {
                        if (task.Drawings != null)
                        {
                            foreach (GameObject obj in task.Drawings)
                                if (obj != null)
                                    obj.SetActive(false);
                        }
                    }


                    //Remove(disable all temporary drawings
                    if (!resetThisFrame)
                    {
                        for (int i = this.temporaryDrawings.Count - 1; i >= 0; i--)
                        {
                            this.temporaryDrawings[i].SetActive(false);
                            UnityEngine.Object.Destroy(this.temporaryDrawings[i]);
                            this.temporaryDrawings.RemoveAt(i);
                        }
                        resetThisFrame = true;
                    }



                    if (mmuResult.DrawingCalls == null)
                        return;

                    foreach (MDrawingCall drawingCall in mmuResult.DrawingCalls)
                    {
                        if (drawingCall == null)
                            continue;

                        GameObject drawingObject = null;

                        switch (drawingCall.Type)
                        {
                            case MDrawingCallType.DrawLine2D:
                                GameObject line2D = DrawingUtils.DrawLine2D(drawingCall.Data);
                                line2D.name = instance.MMU.Name + "_" + instance.CurrentTasks[0].Instruction.Name;
                                instance.CurrentTasks[0].Drawings.Add(line2D);

                                //Assign the created object
                                drawingObject = line2D;

                                break;

                            case MDrawingCallType.DrawLine3D:
                                GameObject line3D = DrawingUtils.DrawLine3D(drawingCall.Data);
                                line3D.name = instance.MMU.Name + "_" + instance.CurrentTasks[0].Instruction.Name;
                                instance.CurrentTasks[0].Drawings.Add(line3D);

                                //Assign the created object
                                drawingObject = line3D;

                                break;

                            case MDrawingCallType.DrawPoint3D:
                                GameObject point = DrawingUtils.DrawPoint(drawingCall.Data);
                                point.name = instance.MMU.Name + "_" + instance.CurrentTasks[0].Instruction.Name;
                                instance.CurrentTasks[0].Drawings.Add(point);

                                //Assign the created object
                                drawingObject = point;
                                break;

                            case MDrawingCallType.DrawText:
                                GameObject label = new GameObject(instance.MMU.Name + "_" + instance.CurrentTasks[0].Instruction.Name);
                                TextMesh textMesh = label.AddComponent<TextMesh>();
                                label.transform.localPosition = new Vector3(0, 2.0f, 0);
                                label.transform.parent = this.avatar.transform;
                                label.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                                textMesh.fontSize = 100;

                                if (drawingCall.Properties.ContainsKey("Text"))
                                    textMesh.text = drawingCall.Properties["Text"];
                                else
                                    textMesh.text = instance.MMU.Name + "_" + instance.CurrentTasks[0].Instruction.Name;

                                instance.CurrentTasks[0].Drawings.Add(label);

                                //Assign the created object
                                drawingObject = label;

                                break;

                        }


                        //Check if the drawing call has properties
                        if (drawingObject != null && drawingCall.Properties != null && drawingCall.Properties.ContainsKey("DrawingMode"))
                        {
                            if (drawingCall.Properties["DrawingMode"] == "Frame")
                            {
                                //Remove after next frame
                                this.temporaryDrawings.Add(drawingObject);
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.Log("Exception at handling drawing calls: " + e.Message + " " + e.StackTrace);
                }
            });
        }


        public override MBoolResponse Abort(string instructionId = null)
        {
            base.Abort(instructionId);

            //Clear all constraints
            if (this.SimulationState != null)          
                this.SimulationState.Constraints = new List<MConstraint>();
            



            //Clear all drawing calls -> To do

            return new MBoolResponse(true);
        }



    }

}
