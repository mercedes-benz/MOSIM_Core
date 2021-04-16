// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Harkiran Sahota

using MMICSharp.Common.Attributes;
using MMICSharp.Common.Communication;
using MMIStandard;
using MMIUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityLocomotionMMU
{
    /// <summary>
    /// MMU which is based on the unity mecanim animation systems which utilizes a default locomotion system
    /// </summary>
    [MMUDescriptionAttribute("Felix Gaisbauer", "1.1", "UnityLocomotionMMUImpl", "Locomotion/Walk", "", "A walk MMU.", "A walk MMU utilizing the Mecanim animation system of Unity.")]
    public class UnityLocomotionMMUImpl : UnityMMUBase
    {
        #region public Fields

        /// <summary>
        /// The acceleration of the avatar
        /// </summary>
        public float Acceleration = 0.8f;

        /// <summary>
        /// The maximum velocity of the avatar
        /// </summary>
        public float Velocity = 1.0f;

        /// <summary>
        /// The max angular velocity of the avatar
        /// </summary>
        public float AngularVelocity = 120f;

        /// <summary>
        /// The time horizon to look for new way points
        /// </summary>
        public TimeSpan TimeHorizon = TimeSpan.FromSeconds(0.5);

        #endregion

        #region private fields


        /// <summary>
        /// The accuracy for reaching the goal
        /// </summary>
        private float goalAccuracy = 0.10f;

        /// <summary>
        /// The replanning time (in case of dynamic path planning)
        /// </summary>
        private int replanningTime = 0;

        /// <summary>
        /// Specifies whether the target orientation is used
        /// </summary>
        private bool useTargetOrientation = true;

        /// <summary>
        /// Specifies whether the motion is only stopped if the velocity is below a specific threshold
        /// </summary>
        private bool useVelocityStoppingThreshold = true;

        /// <summary>
        /// The threshold for terminating the motion
        /// </summary>
        private float velocityStoppingThreshold = 0.2f;

        /// <summary>
        /// Specifies whether the motion is aborted if no path is found
        /// </summary>
        private bool useStraightLineIfNoPath = false;

        /// <summary>
        /// Specifies whether the scene objects (close to the avatar) should be filtered out
        /// </summary>
        private bool filterSceneObjects = true;

        /// <summary>
        /// The timespan of the present frame
        /// </summary>
        private TimeSpan timespan = TimeSpan.Zero;

        /// <summary>
        /// The current walk state, important for the state machine
        /// </summary>
        private WalkState state = WalkState.Idle;

        /// <summary>
        /// The utilized trajectory for path following
        /// </summary>
        private MotionTrajectory2D trajectory;

        /// <summary>
        /// Discrete poses derived from the trajectory
        /// </summary>
        private List<TimedPose2D> discretePoses;

        /// <summary>
        /// The current trajectory index
        /// </summary>
        private int currentIndex = 0;

        /// <summary>
        /// The (current) goal position
        /// </summary>
        private Vector2 goalPosition = new Vector2();

        private Vector2 lastGoalPosition;

        /// <summary>
        /// A reference to the animator
        /// </summary>
        private Animator animator;

        /// <summary>
        /// The current simulation state
        /// </summary>
        private MSimulationState simulationState;

        /// <summary>
        /// The elapsed time since the instruction has been started
        /// </summary>
        private TimeSpan elapsedTime;

        /// <summary>
        /// The presently assigned instruction
        /// </summary>
        private MInstruction instruction;

        /// <summary>
        /// Drawing calls for displaying the path in the remote target engine
        /// </summary>
        private List<MDrawingCall> drawingCalls;

        /// <summary>
        /// Flag specifies whether this is the first frame since the assign instruction
        /// </summary>
        private bool firstFrame = false;

        /// <summary>
        /// Flag indicates whether the motion should be aborted
        /// </summary>
        private bool abort = false;




        private readonly AnimationTracker leftFootAnimationTracker = new AnimationTracker();
        private readonly AnimationTracker rightFootAnimationTracker = new AnimationTracker();
        private readonly List<MSimulationEvent> events = new List<MSimulationEvent>();


        #endregion


        /// <summary>
        /// Basic awake routine
        /// </summary>
        protected override void Awake()
        {
            this.Name = "UnityLocomotionMMU";
            this.MotionType = "Locomotion/Walk";

            //We do not need to set the root transform
            this.RootTransform = this.transform;
            this.Pelvis = this.GetComponentsInChildren<Transform>().First(s => s.name == "pelvis");
            base.Awake();
        }


        /// <summary>
        /// Basic initialization method
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {         
            //Assign the vatar description
            this.AvatarDescription = avatarDescription;

            //Execute instructions on main thread
            this.ExecuteOnMainThread(() =>
            {
                base.Initialize(avatarDescription, properties);


                this.animator = this.GetComponent<Animator>();
                this.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                this.animator.enabled = false;
                this.animator.speed = this.Velocity / 2.0f;

                //Update the animator to establish a first posture
                this.animator.Update(0);

            });

            return new MBoolResponse(true);

        }



        /// <summary>
        /// Starts executing the motion command
        /// </summary>
        /// <param name="motionCommand"></param>
        /// <param name="motionController"></param>

        [MParameterAttribute("TargetName", "string", "The name of the target object", true)]
        [MParameterAttribute("TargetID", "string", "The id of the target object", true)]
        //[MParameterAttribute("Target", "MGeometryConstraint", "The target object", true)]

        [MParameterAttribute("Trajectory", "MPathConstraint", "An optional trajectory that is used as reference path.", false)]
        [MParameterAttribute("Velocity", "float", "The desired velocity of the walk motion.", false)]
        [MParameterAttribute("AngularVelocity", "float", "The max angular velocity of the walk motion", false)]
        [MParameterAttribute("ForcePath", "bool", "If set, a straight line path is used if no valid one can be planned.", false)]
        [MParameterAttribute("ReplanningTime", "int", "Defines the interval after which a new path is planned [ms]. ", false)]
        [MParameterAttribute("UseTargetOrientation", "bool", "If set, the target orientation of the target object will be used.", false)]
        [MParameterAttribute("FilterSceneObjects", "bool", "Optionally filters scene objects being close to the avatar for path planning.", false)]
        [MParameterAttribute("UseVelocityStoppingThreshold", "bool", "Optionally defined threshold above which the motion cannot be terminated.", false)]


        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            //Create a response
            MBoolResponse response = new MBoolResponse(true);

            //Set default values
            this.abort = false;
            this.filterSceneObjects = true;

            //Assign the simulation state
            this.simulationState = simulationState;

            //Assign the instruction
            this.instruction = instruction;

            //Reset the elapsed time
            this.elapsedTime = TimeSpan.Zero;

            ///Parse the properties
            bool requiredPropertiesSet = this.ParseProperties(instruction);

            //Check if all required properties have been set
            if (!requiredPropertiesSet)
            {
                response.LogData = new List<string>()
                {
                    "Properties are not defined -> cannot start the MMU"
                };

                response.Successful = false;
                return response;
            }


            //Get the target transform
            MTransform targetTransform = this.GetTarget();

            if (targetTransform == null)
            {
                response.Successful = false;
                response.LogData = new List<string>() { "Problem at fetching the target transform!" };
                return response;
            }


            //Flag indicates wheather a predefined trajectory should be used
            bool usePredefinedTrajectory = false;


            //Execute instructions on main thread
            this.ExecuteOnMainThread(() =>
            {
                //Set the channel data of the current simulation state
                this.SkeletonAccess.SetChannelData(this.simulationState.Current);

                //Get the root position and rotation
                MVector3 rootPosition = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, MJointType.PelvisCentre);
                MQuaternion rootRotation = this.SkeletonAccess.GetGlobalJointRotation(this.AvatarDescription.AvatarID, MJointType.PelvisCentre);

                //Extract the start position
                Vector2 startPosition = new Vector2((float)rootPosition.X, (float)rootPosition.Z);

                //Get the goal position
                this.goalPosition = new Vector2((float)targetTransform.Position.X, (float)targetTransform.Position.Z);

                this.lastGoalPosition = this.goalPosition;

                //Fetch the trajectory if available
                if (instruction.Properties.ContainsKey("Trajectory"))
                {
                    try
                    {
                        //Get the path constraint
                        MPathConstraint pathConstraint = instruction.Constraints.Find(s => s.ID == instruction.Properties["Trajectory"]).PathConstraint;


                        //Get the actual trajectory from the path constraint
                        List<Vector2> pointList = pathConstraint.GetVector2List();


                        //Estimate the distance between the start point and first point of trajectory
                        float distance = (startPosition - pointList[0]).magnitude;

                        //If distance to first point of trajectory is below threshold -> Directly connect the start point with the first point of the trajectory
                        if (distance < 0.5)
                        {
                            pointList.Insert(0, startPosition);
                            trajectory = new MotionTrajectory2D(pointList, this.Velocity);
                        }

                        else
                        {
                            //Compute a path to the trajectory start location
                            this.trajectory = this.ComputePath(startPosition, pointList[0], this.filterSceneObjects);

                            //Add the defined trajectory
                            this.trajectory.Append(pointList);

                            //Finally estimate the timestamps based on the velocity
                            this.trajectory.EstimateTimestamps(this.Velocity);
                        }

                        //Set flag that no more planning is required
                        usePredefinedTrajectory = true;
                    }
                    catch (Exception e)
                    {
                        MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR,"UnityLocomotionMMU: Cannot parse trajectory -> plan new one: " + e.Message + " " + e.StackTrace);
                    }
                }

                //Only plan the path if no predefined trajectory should be used
                if (!usePredefinedTrajectory)
                {
                    //Compute a path which considers the indivudal path goals and the constraints
                    this.trajectory = this.ComputePath(startPosition, this.goalPosition, filterSceneObjects);
                }


                


                if (this.trajectory == null ||this.trajectory.Poses.Count ==0)
                {
                    this.abort = true;

                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, $"No path found in assign instruction. Aborting MMU.");


                    response.LogData = new List<string>()
                    {
                        "No path found"
                    };

                    response.Successful = false;            
                }


                //Valid path found
                if (this.trajectory != null && this.trajectory.Poses.Count > 0)
                { 
                    //Create the visualization data for the trajectory (drawing calls)
                    this.CreateTrajectoryVisualization();

                    //Set the speed of the animator
                    this.animator.speed = this.Velocity / 2.0f;

                    //Reset the goal direction
                    this.goalPosition = new Vector2();

                    //Update the animation
                    this.animator.SetFloat("Velocity", 0);
                    this.animator.SetFloat("Direction", 0.5f);

                    //Reset the current index
                    this.currentIndex = 0;

                    //Set the internal fields
                    this.transform.position = new Vector3((float)rootPosition.X, (float)this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, MJointType.Root).Y, (float)rootPosition.Z);


                    // In the new intermediate skeleton definition, x is pointing backwards, y up, z right. 
                    this.transform.rotation = Quaternion.Euler(0, new Quaternion((float)rootRotation.X, (float)rootRotation.Y, (float)rootRotation.Z, (float)rootRotation.W).eulerAngles.y - 90.0f, 0);

                    //Get the discrete poses representing the trajectory
                    this.discretePoses = this.trajectory.SampleFrames(60);

                    //Set state to starting
                    this.state = WalkState.Starting;

                    //Set flag which indicates the start
                    this.firstFrame = true;

                    //Reset the foot trackers
                    this.leftFootAnimationTracker.Reset();
                    this.rightFootAnimationTracker.Reset();
                }
            });


            return response;
        }


        /// <summary>
        /// Updates the motion
        /// </summary>
        /// <param name="timespan"></param>
        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            //Create a simulation result
            MSimulationResult result = new MSimulationResult()
            {
                Posture = simulationState.Current,
                Constraints = simulationState.Constraints ?? new List<MConstraint>(),
                SceneManipulations = simulationState.SceneManipulations ?? new List<MSceneManipulation>(),
                Events = simulationState.Events
            };

            //Abort the whole MMU if required
            if (abort)
            {
                result.Events.Add(new MSimulationEvent("No path found", MMIStandard.mmiConstants.MSimulationEvent_Abort, this.instruction.ID));
                return result;
            }

            List<MConstraint> constraints = result.Constraints;

            //Get the current target transform
            MTransform targetTransform = this.GetTarget();

            if (targetTransform == null)
            {
                Debug.Log("No target defined -> error");
                result.LogData = new List<string>()
                {
                    "No target defined -> error"
                };
                return result;
            }

            //Update the goal position
            this.goalPosition = new Vector2((float)targetTransform.Position.X, (float)targetTransform.Position.Z);

            //Set the current state
            this.simulationState = simulationState;

            TimeSpan timespan = TimeSpan.FromSeconds(time);

            //Clear all events
            this.events.Clear();

            ///Moreover the drawing calls
            if (!firstFrame)
                this.drawingCalls = null;

            //Set started flag to false
            if (firstFrame)
                firstFrame = false;

            //Execute instructions on main thread
            this.ExecuteOnMainThread(() =>
            {
                try
                {

                    if (this.state != WalkState.Idle)
                    {
                        //Update the tracker for tracking the velocity
                        this.leftFootAnimationTracker.UpdateStats(this.animator.GetBoneTransform(HumanBodyBones.LeftFoot), (float)time);
                        this.rightFootAnimationTracker.UpdateStats(this.animator.GetBoneTransform(HumanBodyBones.RightFoot), (float)time);

                        //Do the local steering
                        this.FollowPathRootTransform((float)time);

                        //Update the animator and sample new animation
                        this.animator.Update((float)time);

                        //Get the posture of the animation at the specified time 
                        this.simulationState.Current = this.GetPosture();

                        //Increment the time
                        this.timespan = timespan;
                        this.elapsedTime += timespan;
                    }


                    result.Posture = this.GetRetargetedPosture();
                    result.Events = this.events;
                    result.DrawingCalls = this.drawingCalls;


                    //Set constraints relevant for the posture
                    //this.constraintManager.SetEndeffectorConstraint(MEndeffectorType.LeftFoot, this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, MJointType.LeftAnkle), this.SkeletonAccess.GetGlobalJointRotation(this.AvatarDescription.AvatarID, MJointType.LeftAnkle));
                    //this.constraintManager.SetEndeffectorConstraint(MEndeffectorType.RightFoot, this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, MJointType.RightAnkle), this.SkeletonAccess.GetGlobalJointRotation(this.AvatarDescription.AvatarID, MJointType.RightAnkle));

                    //this.constraintManager.SetEndeffectorConstraint(MEndeffectorType.LeftHand, result.Posture.GetGlobalPosition(MJointType.LeftWrist), result.Posture.GetGlobalRotation(MJointType.LeftWrist));
                    //this.constraintManager.SetEndeffectorConstraint(MEndeffectorType.RightHand, result.Posture.GetGlobalPosition(MJointType.RightWrist), result.Posture.GetGlobalRotation(MJointType.RightWrist));

                }
                catch (Exception e)
                {
                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, $"Problem within do-step of UnityLocomotionMMU: {e.Message} {e.StackTrace}");
                }
            });

            return result;
        }


        #region private methods


        /// <summary>
        /// Parses the properties attached to the instruction
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private bool ParseProperties(MInstruction instruction)
        {

            //Flag specifies whether a straigh line path is forced if no path can be obtained
            if (instruction.Properties.ContainsKey("ForcePath"))
                this.useStraightLineIfNoPath = bool.Parse(instruction.Properties["ForcePath"]);

            //Flag specifies whether a straigh line path is forced if no path can be obtained
            if (instruction.Properties.ContainsKey("AngularVelocity"))
                this.AngularVelocity = float.Parse(instruction.Properties["AngularVelocity"], System.Globalization.CultureInfo.InvariantCulture);


            //The replanning time in ms
            if (instruction.Properties.ContainsKey("ReplanningTime"))
                this.replanningTime = int.Parse(instruction.Properties["ReplanningTime"]);

            //The desired velocity
            if (instruction.Properties != null && instruction.Properties.ContainsKey("Velocity"))
                this.Velocity = float.Parse(instruction.Properties["Velocity"]);

            //Check if target orientation should be used
            if (instruction.Properties.ContainsKey("UseTargetOrientation"))
                this.useTargetOrientation = bool.Parse(instruction.Properties["UseTargetOrientation"]);

            //Check if scene objects should be automatically filtered
            if (instruction.Properties.ContainsKey("FilterSceneObjects"))
                filterSceneObjects = bool.Parse(instruction.Properties["FilterSceneObjects"]);

            //Check if scene objects should be automatically filtered
            if (instruction.Properties.ContainsKey("UseVelocityStoppingThreshold"))
                this.useVelocityStoppingThreshold = bool.Parse(instruction.Properties["UseVelocityStoppingThreshold"]);

            //Check if mandatory properties are defined -> otherwise skip
            if (instruction.Properties == null || (!instruction.Properties.ContainsKey("TargetName") && !instruction.Properties.ContainsKey("TargetID")))
            {
                return false;
            }

            //Return true by default 
            return true;
        }


        /// <summary>
        /// Method does the local steering
        /// </summary>
        /// <param name="time">The planning time for the present frame</param>
        private void FollowPathRootTransform(float time)
        {
            //Use the root transform
            Vector2 currentPosition = new Vector2(transform.position.x, transform.position.z);
            Quaternion currentRotation = transform.rotation;


            //Check if target position has changed
            float dist = (this.lastGoalPosition - this.goalPosition).magnitude;

            //Estimate the distance to the goal
            float goalDistance = (currentPosition - this.goalPosition).magnitude;


            bool replanPath = false;




            //Check if the target object has changed -> Replanning required
            if (dist > 0.1f)
            {
                this.goalPosition = new Vector2(this.goalPosition.x, this.goalPosition.y);
                replanPath = true;
            }


            //Check if replanning is enforced by replanning time
            else
            {
                replanPath = goalDistance > 0.5f && replanningTime > 0 && elapsedTime.TotalMilliseconds > 0 && (int)elapsedTime.TotalMilliseconds % this.replanningTime == 0;
            }




            //Optionally do reactive replanning
            if (replanPath)
            {
                //Compute a new path
                this.trajectory = this.ComputePath(new Vector2(transform.position.x, transform.position.z), this.goalPosition);

                //Get the discrete poses from the path
                this.discretePoses = this.trajectory.SampleFrames(60);

                //Reset index and first frame flag
                this.currentIndex = 0;           
                this.firstFrame = true;

                //Create a visualization of the trajectory (drawing calls)
                this.CreateTrajectoryVisualization();
            }

            //Get the next waypoint
            Vector2 nextWaypoint = this.GetNextWaypoint(this.trajectory, currentPosition, this.TimeHorizon);

            //Use the goal position directly if below threshold
            if (goalDistance < 0.5)
                nextWaypoint = new Vector2(this.goalPosition.x, this.goalPosition.y);


            switch (this.state)
            {
                //Rotate towards the target before starting
                case WalkState.Starting:

                    //Call the method of the dhm
                    if (this.OrientateTowards(nextWaypoint, this.timespan, 5))
                    {
                        //Change the state if finished
                        this.state = WalkState.Walking;
                    }

                    break;

                //The main state during walking
                case WalkState.Walking:

                    //Determine the next target position
                    Vector2 nextTarget = this.GetNextWaypoint(this.trajectory, currentPosition, this.TimeHorizon);

                    //Check if the goal distance is below a defined threhsold -> Set the next target to goal position
                    if (goalDistance < 0.5)
                        nextTarget = new Vector2(this.goalPosition.x, this.goalPosition.y);

                    //Determine the trajectory direction
                    Vector2 trajectoryDirection = (nextTarget - currentPosition);

                    //Estimate the max distance allowed
                    float travelDistance = time * this.Velocity;

                    //Compute the new position
                    Vector2 nextPosition = currentPosition + trajectoryDirection.normalized * travelDistance;

                    //Update the position and rotation according to the animation
                    this.transform.position = this.animator.rootPosition;
                    this.transform.rotation = this.animator.rootRotation;

                    //The current direction vector
                    Vector3 currentDirection = (this.transform.rotation * Vector3.forward);
                    currentDirection.y = 0;



                    //Estimate the current angle mismatch
                    float currentDeltaAngle = Vector3.Angle(currentDirection, new Vector3(trajectoryDirection.x, 0, trajectoryDirection.y));

                    //Rotate to direction
                    //Adjust the rotation
                    //Only adjust the rotation if a sufficient distance to the goal can be encountred
                    //In case the distance is too low -> Turning in place is used instead
                    if (Math.Abs(currentDeltaAngle) > 1e-5f && goalDistance > 0.3f)
                    {
                        float maxAngle = time * this.AngularVelocity;
                        float goalDelta = Math.Abs(currentDeltaAngle);
                        float delta = Math.Min(maxAngle, goalDelta);

                        Vector3 right = (this.transform.rotation * Vector3.right);
                        Vector3 left = (this.transform.rotation * Vector3.left);

                        //Determine the sign of the angle
                        float sign = -1;

                        if (Vector3.Angle(right, new Vector3(trajectoryDirection.x, 0, trajectoryDirection.y)) < Vector3.Angle(left, new Vector3(trajectoryDirection.x, 0, trajectoryDirection.y)))
                            sign = 1;

                        this.transform.rotation = Quaternion.Euler(0, sign * delta, 0) * this.transform.rotation;
                    }

                    //Update the animation -> Set the flots of the animator component to control the animation tree
                    this.animator.SetFloat("Velocity", ((nextPosition - currentPosition).magnitude / time));
                    this.animator.SetFloat("Direction", 0.5f);


                    //Check if target reached
                    if ((nextPosition - this.goalPosition).magnitude < this.goalAccuracy)
                    {
                        //Set velocity to zero
                        this.animator.SetFloat("Velocity", 0f);


                        //Check velocity threshold if defined
                        if (!useVelocityStoppingThreshold || (this.leftFootAnimationTracker.Velocity < velocityStoppingThreshold && this.rightFootAnimationTracker.Velocity < velocityStoppingThreshold))
                        {
                            //Go to finishing state and perform further reorientation
                            if (this.useTargetOrientation)
                            {
                                this.state = WalkState.Finishing;
                            }

                            //Finish and move to idle
                            else
                            {
                                this.state = WalkState.Idle;
                                this.events.Add(new MSimulationEvent(this.instruction.Name, mmiConstants.MSimulationEvent_End, this.instruction.ID));
                            }
                        }
                    }

                    break;

                //The finishing state which performs the final fine-grained rotation
                case WalkState.Finishing:

                    //Get the ttarget transform
                    MTransform targetTransform = this.GetTarget();

                    MVector3 forward = targetTransform.Rotation.Multiply(new MVector3(0, 0, 1));
                    Vector2 forward2 = new Vector2((float)forward.X, (float)forward.Z).normalized;

                    bool validOrientation = false;

                    if (this.OrientateTowards(goalPosition + forward2 * 10f, this.timespan, 5))
                    {
                        validOrientation = true;
                    }

                    //Only finish if velocity is close to zero
                    if (validOrientation)
                    {
                        //Set to finished -> Reset state machine
                        this.state = WalkState.Idle;

                        //Add finished event
                        this.events.Add(new MSimulationEvent(this.instruction.Name, mmiConstants.MSimulationEvent_End, this.instruction.ID));
                    }


                    break;
            }

        }


        /// <summary>
        /// Computes a path given a start and end position
        /// </summary>
        /// <param name="from"></param>
        /// <param name="target"></param>
        /// <param name="filterSceneObjects"></param>
        /// <returns></returns>
        private MotionTrajectory2D ComputePath(Vector2 from, Vector2 target, bool filterSceneObjects = true)
        {
            List<MTransform> computedPath = new List<MTransform>();

            //Only plan path if distance is above threshold
            if ((from - target).magnitude > 0.2f)
            {

                try
                {
                    //Get all scene objects from the scene
                    List<MSceneObject> sceneObjects = this.SceneAccess.GetSceneObjects();

                    ///Remove scene objects in range if filtering is enabled
                    if (filterSceneObjects)
                    {
                        MVector3 hipPosition = this.GetGlobalPosition(this.simulationState.Initial, MJointType.PelvisCentre);
                        MVector3 leftHandPosition = this.GetGlobalPosition(this.simulationState.Initial, MJointType.LeftWrist);
                        MVector3 rightHandPosition = this.GetGlobalPosition(this.simulationState.Initial, MJointType.RightWrist);

                        for (int i = sceneObjects.Count - 1; i >= 0; i--)
                        {
                            MVector3 sceneObjectPosition = sceneObjects[i].Transform.Position;

                            float hipDist = (sceneObjectPosition.Subtract(hipPosition)).Magnitude();

                            float lhandDist = (leftHandPosition.Subtract(sceneObjectPosition)).Magnitude();
                            float rhandDist = (rightHandPosition.Subtract(sceneObjectPosition)).Magnitude();


                            if (lhandDist < 0.5f || rhandDist < 0.5f)
                            {
                                MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_DEBUG, $"Removing scene object {sceneObjects[i].Name}, that is not included in path planning.");
                                sceneObjects.RemoveAt(i);
                            }
                        }
                    }


                    //Compute the path using the path planning service
                    MPathConstraint result = ServiceAccess.PathPlanningService.ComputePath(
                        new MVector() { Values = new List<double>() { from.x, from.y } },
                        new MVector() { Values = new List<double>() { target.x, target.y } },
                        sceneObjects,
                        new Dictionary<string, string>()
                        {
                            { "mode", "2D"},
                            { "time", Serialization.ToJsonString(1.0f)},
                            { "radius", Serialization.ToJsonString(0.3f)},
                            { "height", Serialization.ToJsonString(0.5f)},
                        });



                    //Get the computed path
                    if (result.PolygonPoints.Count > 0)
                    {
                        if (result.PolygonPoints[0].ParentToConstraint != null)
                        {
                            computedPath = result.PolygonPoints.Select(s => new MTransform() { Position = new MVector3(s.ParentToConstraint.Position.X, 0, s.ParentToConstraint.Position.Z) }).ToList();
                        } else
                        {
                            // TODO: Legacy support. Remove in a future version
                            computedPath = result.PolygonPoints.Select(s => new MTransform() { Position = new MVector3(s.TranslationConstraint.X(), 0, s.TranslationConstraint.Z()) }).ToList();
                        }
                    }



                }
                catch (Exception e)
                {
                    MMICSharp.Adapter.Logger.Log( MMICSharp.Adapter.Log_level.L_ERROR,"Problem at computing path using service " + e.Message + " " + e.StackTrace);

                    //In case of an exception return the straight line 
                    //To do use an optional flag to adjust the desired behavior, e.g. should an error be returned instead
                    if (this.useStraightLineIfNoPath)
                    {
                        computedPath = new List<MTransform>()
                        {
                            new MTransform() { Position = new MVector3(from.x, 0, from.y ) },
                            new MTransform() { Position = new MVector3((from.x + target.x)/2, 0, (from.y+target.y)/2) },
                            new MTransform() { Position = new MVector3 (target.x, 0, target.y) },
                        };
                    }
                }
                finally
                {
                    if (this.useStraightLineIfNoPath && computedPath.Count == 0)
                    {
                        computedPath = new List<MTransform>()
                        {
                            new MTransform() { Position = new MVector3(from.x, 0, from.y ) },
                            new MTransform() { Position = new MVector3((from.x + target.x)/2, 0, (from.y+target.y)/2) },
                            new MTransform() { Position = new MVector3 (target.x, 0, target.y) },
                        };

                    }
                }
            }
            //If really close to goal -> No detailed planning is required
            else
            {
                computedPath = new List<MTransform>()
                {
                        new MTransform() { Position = new MVector3(from.x, 0, from.y ) },
                        new MTransform() { Position = new MVector3((from.x + target.x)/2, 0, (from.y+target.y)/2) },
                        new MTransform() { Position = new MVector3 (target.x, 0, target.y) },
                };
            }


            MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_DEBUG, "Computed path elements: " + computedPath.Count);

            if (computedPath.Count == 0)
                return null;

            //Create a motion trajectory from the path
            return new MotionTrajectory2D(computedPath, this.Velocity);
        }


        /// <summary>
        /// Creates the visualization data (drawing calls) required for showing the computed trajectory
        /// </summary>
        private void CreateTrajectoryVisualization()
        {
            float length = trajectory.Length;

            List<TimedPose2D> poses = trajectory.SampleFrames((int)(length / 0.02f));

            //Create the drawing call related information
            List<double> data = new List<double>();
            foreach (float[] pos in poses.Select(s => s.Position.ToArray()))
            {
                data.AddRange(pos.ToList().Select(s => (double)s).ToList());
            }

            this.drawingCalls = new List<MDrawingCall>()
            {
                new MDrawingCall()
                {
                    Type = MDrawingCallType.DrawLine2D,
                    Data = data
                }
            };

        }

 
        /// <summary>
        /// Estimates the next waypoint of a given path.
        /// Returns the first point which is farer away than the specified distance
        /// </summary>
        /// <param name="globalPath"></param>
        /// <param name="currentPosition"></param>
        /// <param name="horizon">The time horizon for planning</param>
        /// <returns></returns>
        private Vector2 GetNextWaypoint(MotionTrajectory2D globalPath, Vector2 currentPosition, TimeSpan horizon)
        {

            Vector2 waypoint = currentPosition;

            TimeSpan time = this.EstimateCurrentTime(currentPosition);

            //Determine the next pose
            TimedPose2D pose = this.trajectory.Poses.Last();

            //Only interpolate if time is not exceeded
            if (time + horizon < pose.Time)
                pose = this.trajectory.GetPose(time + horizon);

            //Set the waypoint
            waypoint = new Vector2(pose.Position.x, pose.Position.y);

            return waypoint;
        }


        /// <summary>
        /// Estimates the current time of the current position within the spline
        /// </summary>
        /// <returns></returns>
        private TimeSpan EstimateCurrentTime(Vector2 position)
        {
            TimeSpan time = TimeSpan.Zero;

            float minDist = float.MaxValue;
            int bestIndex = this.currentIndex;

            for (int i = this.currentIndex; i < this.discretePoses.Count; i++)
            {
                TimedPose2D frame = this.discretePoses[i];

                float dist = (frame.Position - position).magnitude;

                if (dist < minDist)
                {
                    time = frame.Time;
                    minDist = dist;
                    bestIndex = i;
                }
            }

            this.currentIndex = bestIndex;
            return time;
        }


        /// <summary>
        /// Orientates the agent towards the specified position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="timespan">The timespan</param>
        /// <returns></returns>
        private bool OrientateTowards(Vector2 position, TimeSpan timespan, float threshold)
        {
            Vector2 currentPosition = new Vector2(this.transform.position.x, this.transform.position.z);
            Quaternion currentRotation = this.GetComRotation();


            //Compute the target direction
            Vector2 targetDirection = (position - currentPosition).normalized;

            Vector3 currentDirection = (currentRotation * Vector3.forward);
            currentDirection.y = 0;


            float currentDeltaAngle = Vector3.Angle(currentDirection, new Vector3(targetDirection.x, 0, targetDirection.y));

            //Rotate to direction
            //Adjust the rotation
            if (Math.Abs(currentDeltaAngle) > threshold)
            {
                float maxAngle = (float)timespan.TotalSeconds * this.AngularVelocity;
                float goalDelta = Math.Abs(currentDeltaAngle);
                float delta = Math.Min(maxAngle, goalDelta);

                Vector3 right = (currentRotation * Vector3.right);
                Vector3 left = (currentRotation * Vector3.left);

                //Determine the sign of the angle
                float sign = -1;

                if (Vector3.Angle(right, new Vector3(targetDirection.x, 0, targetDirection.y)) < Vector3.Angle(left, new Vector3(targetDirection.x, 0, targetDirection.y)))
                    sign = 1;


                //Compute the aniumation value
                float animationDirectionValue = goalDelta;

                if (Math.Abs(goalDelta) > maxAngle)
                {
                    if (sign < 0)
                        animationDirectionValue = -0.5f;
                    else
                        animationDirectionValue = 0.5f;
                }

                else
                {
                    animationDirectionValue = (goalDelta / maxAngle) / 2.0f;
                }

                //Set the direction
                this.animator.SetFloat("Direction", 0.5f + animationDirectionValue);

                return false;
            }
            else
            {
                this.animator.SetFloat("Direction", 0.5f);

                return true;
            }
        }


        #region helper functions



        /// <summary>
        /// Returns the transform of the target 
        /// </summary>
        /// <returns></returns>
        private MTransform GetTarget()
        {
            if (instruction.Properties.ContainsKey("TargetID"))
            {
                //First check if this is a gemoetry constraint
                if(instruction.Constraints !=null && instruction.Constraints.Exists(s=>s.ID == instruction.Properties["TargetID"]))
                {                   
                    MConstraint match = instruction.Constraints.Find(s => s.ID == instruction.Properties["TargetID"]);
                    return match.GeometryConstraint.ParentToConstraint;
                    
                }

                else
                {
                    return this.SceneAccess.GetTransformByID(instruction.Properties["TargetID"]);
                }
            }

            if (instruction.Properties.ContainsKey("TargetName"))
            {
                MSceneObject sceneObject = this.SceneAccess.GetSceneObjectByName(instruction.Properties["TargetName"]);

                if (sceneObject != null)
                {
                    return sceneObject.Transform;
                }
            }

            return null;
        }


        /// <summary>
        /// Returns the center of mass rotation
        /// </summary>
        /// <returns></returns>
        private Quaternion GetComRotation()
        {
            return Quaternion.Euler(0, this.transform.rotation.eulerAngles.y, 0);
        }


        /// <summary>
        /// Returns the global position of the specific joint type and posture
        /// </summary>
        /// <param name="posture"></param>
        /// <param name="jointType"></param>
        /// <returns></returns>
        private MVector3 GetGlobalPosition(MAvatarPostureValues posture, MJointType jointType)
        {
            this.SkeletonAccess.SetChannelData(posture);
            return this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, jointType);
        }

        #endregion

        #endregion
    }

}

