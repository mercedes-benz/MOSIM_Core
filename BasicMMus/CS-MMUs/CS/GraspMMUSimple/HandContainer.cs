using MMIStandard;
using System.Collections.Generic;

namespace GraspMMUSimple
{
    public class HandContainer
    {
        public MJointType Type = MJointType.LeftWrist;

        public List<Finger> Fingers = new List<Finger>();

        public MHandPose ClosedHand;

        /// <summary>
        /// The final posture that should be established
        /// </summary>
        public MPostureConstraint FinalPosture;

        /// <summary>
        /// The assigned instruction
        /// </summary>
        public MInstruction Instruction;

        /// <summary>
        /// The angular velocity used for blending
        /// </summary>
        public float AngularVelocity = 90f;

        /// <summary>
        /// The time the blend required (if defined)
        /// </summary>
        public float Duration = 1f;

        /// <summary>
        /// The elapsed time
        /// </summary>
        public float Elapsed = 0;

        /// <summary>
        /// Specifies whether the duration/time is used
        /// </summary>
        public bool HasDuration = false;

        /// <summary>
        /// Indicates whether a release should be performed
        /// </summary>
        public bool Release = false;

        /// <summary>
        /// Flag which indicates whether the positioning is finished
        /// </summary>
        public bool Positioned = false;

        public bool KeepHandPose = false;

        public bool UseGlobalCoordinates = true;


        /// <summary>
        /// The current finger rotations
        /// </summary>
        public Dictionary<MJointType, MQuaternion> InitialFingerRotations = new Dictionary<MJointType, MQuaternion>();

    }


    public class Finger
    {
        public List<MJointType> JointTypes = new List<MJointType>();

        public MJointType FingerTip = MJointType.Undefined;

        public float MinDistance = float.MaxValue;

        public Dictionary<MJointType, MQuaternion> Current = new Dictionary<MJointType, MQuaternion>();


        public Dictionary<MJointType, MQuaternion> Best = new Dictionary<MJointType, MQuaternion>();
    }
}
