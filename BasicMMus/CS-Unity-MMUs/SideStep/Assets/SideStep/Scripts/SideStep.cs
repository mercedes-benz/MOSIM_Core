using MMICSharp.Common;
using MMIStandard;
using MMIUnity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Auto-generated MMU class which utilizes the Unity Animator as main source for the motion generation. 
/// </summary>
public class SideStep : UnityMMUBase
{
    public Dictionary<string,MJointType> BoneTypeMapping = new Dictionary<string,MJointType>();

    private float rootZ;
    private Quaternion initialRotation;
    private Quaternion initialRotationInverse;
    private MTransform targetLocation;
    private Vector3 initialLocation;
    private const float stepLength = 0.387765f;
    private int stepsToGo = 0;
    private float motionScale = 1;

    /// <summary>
    /// The animator component
    /// </summary>
    private Animator animator;

    /// <summary>
    /// The assigned instruction
    /// </summary>
    private MInstruction instruction;


    protected override void Awake()
    {
        //Assign the name of the MMU
        this.Name = "SideStep";

        //Assign the motion type of the MMU
        this.MotionType = "Pose/SideStep";

		//Auto generated source code for bone type mapping:
		this.BoneTypeMapping = new Dictionary<string,MJointType>()
		{
			{"pelvis", MJointType.PelvisCentre},
			{"thigh_l", MJointType.LeftHip},
			{"thigh_r", MJointType.RightHip},
			{"calf_l", MJointType.LeftKnee},
			{"calf_r", MJointType.RightKnee},
			{"foot_l", MJointType.LeftAnkle},
			{"foot_r", MJointType.RightAnkle},
			{"spine_01", MJointType.S1L5Joint},
			{"spine_02", MJointType.T1T2Joint},
			{"neck_01", MJointType.C4C5Joint},
			{"head", MJointType.HeadJoint},
			{"upperarm_l", MJointType.LeftShoulder},
			{"upperarm_r", MJointType.RightShoulder},
			{"lowerarm_l", MJointType.LeftElbow},
			{"lowerarm_r", MJointType.RightElbow},
			{"hand_l", MJointType.LeftWrist},
			{"hand_r", MJointType.RightWrist},
			{"ball_l", MJointType.LeftBall},
			{"ball_r", MJointType.RightBall},
			{"thumb_01_l", MJointType.LeftThumbTip},
			{"thumb_02_l", MJointType.LeftThumbMid},
			{"thumb_03_l", MJointType.LeftThumbCarpal},
			{"index_01_l", MJointType.LeftIndexProximal},
			{"index_02_l", MJointType.LeftIndexMeta},
			{"index_03_l", MJointType.LeftIndexDistal},
			{"middle_01_l", MJointType.LeftMiddleProximal},
			{"middle_02_l", MJointType.LeftMiddleMeta},
			{"middle_03_l", MJointType.LeftMiddleDistal},
			{"ring_01_l", MJointType.LeftRingProximal},
			{"ring_02_l", MJointType.LeftRingMeta},
			{"ring_03_l", MJointType.LeftRingDistal},
			{"pinky_01_l", MJointType.LeftLittleProximal},
			{"pinky_02_l", MJointType.LeftLittleMeta},
			{"pinky_03_l", MJointType.LeftLittleDistal},
			{"thumb_01_r", MJointType.RightThumbTip},
			{"thumb_02_r", MJointType.RightThumbMid},
			{"thumb_03_r", MJointType.RightThumbCarpal},
			{"index_01_r", MJointType.RightIndexProximal},
			{"index_02_r", MJointType.RightIndexMeta},
			{"index_03_r", MJointType.RightIndexDistal},
			{"middle_01_r", MJointType.RightMiddleProximal},
			{"middle_02_r", MJointType.RightMiddleMeta},
			{"middle_03_r", MJointType.RightMiddleDistal},
			{"ring_01_r", MJointType.RightRingProximal},
			{"ring_02_r", MJointType.RightRingMeta},
			{"ring_03_r", MJointType.RightRingDistal},
			{"pinky_01_r", MJointType.RightLittleProximal},
			{"pinky_02_r", MJointType.RightLittleMeta},
			{"pinky_03_r", MJointType.RightLittleDistal},
		};

		//Auto generated source code for assignemnt of root transform and root bone:
		this.Pelvis = this.gameObject.GetComponentsInChildren<Transform>().First(s=>s.name == "pelvis");
		this.RootTransform = this.transform;


        //Get the animator (needs to be added in before)
        this.animator = this.GetComponent<Animator>();
        
        //Disable the animator at the beginning (otherwise retargeting won't work)
        this.animator.enabled = false;

		//It is important that the bone assignment is done before the base class awake is called
		base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
	    //All required scripts should be added in here

		//Auto generated source code for script initialization.

        base.Start();
    }

    /// <summary>
    /// Basic initialize routine. This routine is called when the MMU is initialized. 
    /// The method must be implemented by the MMU developer.
    /// </summary>
    /// <param name="avatarDescription"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
    {
        //Execute the instruction on the main thread (required in order to access unity functionality)
        this.ExecuteOnMainThread(() =>
        {
            //Call the base class initialization (important)
            base.Initialize(avatarDescription, properties);

            //Set culling mode to always animate (in adapter the mesh might be invisible)
            this.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            //Deactivate the animator we want to trigger it manually in the dostep
            this.animator.enabled = false;
            
        });

        //Setup other specific properties

		//Return success
        return new MBoolResponse(true);
    }

    /// <summary>
    /// Method to assign a MInstriction to the MMU:
    /// The method must be provided by the MMU developer.
    /// </summary>
    /// <param name="motionCommand"></param>
    /// <param name="avatarState"></param>
    /// <returns></returns>
    public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState state)
    {
        //Assign the instruction to the class variable
        this.instruction = instruction;


        //Execute the instruction on the main thread (required in order to access unity functionality)
        this.ExecuteOnMainThread(() =>
        {
            
            //Assign the posture
            this.AssignPostureValues(state.Current);

            this.initialRotation = this.RootTransform.rotation;
            this.initialRotationInverse = new Quaternion(this.initialRotation.x, this.initialRotation.y, this.initialRotation.z, -this.initialRotation.w);
            this.initialLocation = new Vector3(this.RootTransform.position.x, this.RootTransform.position.y, this.RootTransform.position.z);
            this.rootZ = (this.initialRotationInverse * this.RootTransform.position).z;

            //Parameterize the animator based on the given instruction and avatarState
            this.stepsToGo = 1;
            if (instruction.Properties.ContainsKey("TargetID"))
            {
                targetLocation = this.SceneAccess.GetTransformByID(instruction.Properties["TargetID"]);
                var startZ = this.initialRotationInverse * this.RootTransform.position;
                var targetZ = Vector3.Scale(this.initialRotationInverse * (targetLocation.Position.ToVector3() - this.initialLocation),new Vector3(1,0,0));
                this.animator.SetBool("WalkRight", targetZ.x > 0);
                var D = Mathf.Abs(targetZ.x);
                stepsToGo = Mathf.RoundToInt(D / stepLength);
                if (stepsToGo > 0)
                    this.motionScale = D / stepsToGo / stepLength;
                else
                    this.motionScale = 1;
                Debug.Log("StepsToGo: " + stepsToGo.ToString() + ", motionscale: " + motionScale);
            }
            this.animator.SetBool("AnimationDone", false);
        });

        //To do insert custom code heere

        return new MBoolResponse(true);
    }

    /// <summary>
    /// Basic do step routine. This method must be implemented by the MMU developer.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="avatarState"></param>
    /// <returns></returns>
    public override MSimulationResult DoStep(double time, MSimulationState state)
    {

        //Create a new simulation result
        MSimulationResult result = new MSimulationResult()
        {
            Posture = state.Current,
            Constraints = state.Constraints!=null ? state.Constraints: new List<MConstraint>(),
            Events = state.Events !=null? state.Events: new List<MSimulationEvent>(),
            SceneManipulations = state.SceneManipulations!=null ? state.SceneManipulations: new List<MSceneManipulation>(),
        };

        //Execute the instruction on the main thread (required in order to access unity functionality)
        this.ExecuteOnMainThread(() =>
        {
            //Update the animator
            this.animator.Update((float)time);                                                                                                                                                                                                                //here x scale should be based on motionScale
            var v1 = this.initialRotationInverse * (new Vector3(this.RootTransform.position.x, this.RootTransform.position.y, this.RootTransform.position.z) - this.initialLocation);
            Debug.Log(this.RootTransform.position.x.ToString() + ", " + this.RootTransform.position.y.ToString() + ", " + this.RootTransform.position.z.ToString()+" -> "+
                v1.x.ToString()+", "+v1.y.ToString()+", "+v1.z.ToString());
            this.RootTransform.position = this.initialRotation * (Vector3.Scale(this.initialRotationInverse * (new Vector3(this.RootTransform.position.x, this.RootTransform.position.y, this.RootTransform.position.z) - this.initialLocation), new Vector3(1, 0, 0))) + this.initialLocation;// (new Vector3(0,this.initialLocation.y,this.initialLocation.z))); //constrain z coordinate of the avatar to walk along a straight line no matter errors in motion caputre data
            this.RootTransform.rotation = initialRotation;
            
            //Get the current posture of the after in the intermediate skeleton representation
            result.Posture = this.GetRetargetedPosture();

            //Debug.Log(this.animator.bodyPosition.x.ToString()+", "+this.animator.bodyPosition.z.ToString());
            //To do -> Process the events and return it as result
            if (this.animator.GetBool("AnimationDone"))
            {
                stepsToGo--;
                if (stepsToGo>0) //make one more step
                {
                    this.animator.SetBool("AnimationDone", false);
                }
                else //stop walking
                result.Events.Add(new MSimulationEvent()
                {
                    Name = "Animation finished",
                    Reference = this.instruction.ID,
                    Type = mmiConstants.MSimulationEvent_End
                });
            }
        });

        //Finally, add the end event at the end of the motion
        //result.Events.Add(new MSimulationEvent(this.instruction.Name, mmiConstants.MSimulationEvent_End, this.instruction.ID));

        //Return the result
        return result;
    }


    /// <summary>
    /// Method to return the boundary constraints.
    /// Method can be optionally implemented by the developers.
    /// </summary>
    /// <param name="motionCommand"></param>
    /// <returns></returns>
    public override List<MConstraint> GetBoundaryConstraints(MInstruction motionCommand)
    {
        List<MConstraint> firstPose = new List<MConstraint>();
        this.ExecuteOnMainThread(() =>
        {
            //Update the animator
            this.animator.Update((float)0.01);

            //Get the current posture of the after in the intermediate skeleton representation
            MAvatarPostureValues posture = this.GetRetargetedPosture();

            MConstraint mConstraint = new MConstraint()
            {
                ID = MInstructionFactory.GenerateID(),
                PostureConstraint = new MPostureConstraint(posture)
            };
            firstPose.Add(mConstraint);
        });
        return firstPose;
    }

    /// <summary>
    /// Method checks if the prerequisites for starting the instruction are fulfilled.
    /// </summary>
    /// <param name="instruction"></param>
    /// <returns></returns>
    public override MBoolResponse CheckPrerequisites(MInstruction instruction)
    {
        return new MBoolResponse(true);
    }



}
