// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Felix Gaisbauer

package Extensions;

import de.mosim.mmi.avatar.*;
import de.mosim.mmi.math.MQuaternion;
import de.mosim.mmi.math.MVector3;
import de.mosim.mmi.services.MIKProperty;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import static de.mosim.mmi.avatar.MJointType.*;

public class MAvatarPostureExtensions {
    /// <summary>
    /// Returns a new renamed instance
    /// </summary>
    /// <param name="posture"></param>
    /// <param name="mapping"></param>
    /// <returns></returns>
    public static MAvatarPosture Rename(MAvatarPosture posture, Map<String, String> mapping) {
        MAvatarPosture newPosture = posture.deepCopy();

        for (MJoint transform : newPosture.Joints) {
            if (mapping.containsKey(transform.ID)) {
                transform.ID = mapping.get(transform.ID);
            }
        }
        return newPosture;
    }

    /// <summary>
    /// Returns a new renamed instance
    /// </summary>
    /// <param name="posture"></param>
    /// <param name="mapping"></param>
    /// <returns></returns>
    public static void AssignBoneTypes(MAvatarPosture posture, Map<String, MJointType> mapping) {
        for (MJoint transform : posture.Joints) {
            if (mapping.containsKey(transform.ID)) {
                transform.Type = mapping.get(transform.ID);
            }
        }
    }


    /// <summary>
    /// Returns the global position of the given bone
    /// </summary>
    /// <param name="posture"></param>
    /// <param name="boneType"></param>
    /// <returns></returns>
    public static MVector3 GetGlobalPosition(MAvatarPosture posture, MJointType boneType) {
        //Create an empty list which represents the hierarchy
        List<MJoint> hierarchy = new ArrayList<>();

        //Get the specified bone by type

        MJoint bone = MAvatarPostureExtensions.getJoint(posture, boneType);

        if (bone == null) {
            throw new RuntimeException("Bone  type cannot be found");
        }

        hierarchy.add(bone);

        //Update the hierarchy until no more parent is available
        while (bone.Parent != null) {
            //Assign the parent
            MJoint finalBone = bone;
            bone = posture.Joints.stream().filter(s -> s.ID.equals(finalBone.Parent)).findFirst().orElse(null);
            if (bone != null) {
                hierarchy.add(0, bone);
            } else {
                //Break while loop if no more bone
                break;
            }
        }


        MVector3 position = hierarchy.get(0).Position.deepCopy();
        MQuaternion rotation = hierarchy.get(0).Rotation.deepCopy();


        for (int i = 0; i < hierarchy.size() - 1; i++) {
            //Compute the new position
            position = MVector3Extensions.add(position, MQuaternionExtensions.multiply(rotation, hierarchy.get(i + 1).getPosition()));

            //Update rotation a postiori
            //  rotation=MQuaternionExtensions.multiply(rotation,hierarchy.get(i+1).getRotation());
        }

        return position;
    }


    /// <summary>
    /// Returns the global position of the given bone
    /// </summary>
    /// <param name="posture"></param>
    /// <param name="boneType"></param>
    /// <returns></returns>
    public static MVector3 GetGlobalPosition(MAvatarPosture posture, String boneName) {
        //Create an empty list which represents the hierarchy
        List<MJoint> hierarchy = new ArrayList<>();

        //Get the specified bone by type
        MJoint bone = posture.Joints.stream().filter(s -> s.ID.equals(boneName)).findFirst().orElse(null);


        if (bone == null) {
            throw new RuntimeException("Bone  type cannot be found");
        }

        hierarchy.add(bone);

        //Update the hierarchy until no more parent is available
        while (bone.Parent != null) {
            //Assign the parent
            MJoint finalBone = bone;
            bone = posture.Joints.stream().filter(s -> s.ID.equals(finalBone.Parent)).findFirst().orElse(null);
            if (bone != null) {
                hierarchy.add(0, bone);
            } else {
                //Break while loop if no more bone
                break;
            }
        }


        MVector3 position = hierarchy.get(0).Position.deepCopy();
        MQuaternion rotation = hierarchy.get(0).Rotation.deepCopy();


        for (int i = 0; i < hierarchy.size() - 1; i++) {
            //Compute the new position
            position = MVector3Extensions.add(position, MQuaternionExtensions.multiply(rotation, hierarchy.get(i + 1).getPosition()));

            //Update rotation a postiori
            //  rotation=MQuaternionExtensions.multiply(rotation,hierarchy.get(i+1).getRotation());
        }

        return position;
    }

    /// <summary>
    /// Returns the global rotation of the specified bone type
    /// </summary>
    /// <param name="posture"></param>
    /// <param name="boneType"></param>
    /// <returns></returns>
    public static MQuaternion GetGlobalRotation(MAvatarPosture posture, MJointType boneType) {
        //Create an empty list which represents the hierarchy
        List<MJoint> hierarchy = new ArrayList<>();

        MJoint bone = MAvatarPostureExtensions.getJoint(posture, boneType);

        if (bone == null) {
            throw new RuntimeException("Bone  type cannot be found");
        }

        hierarchy.add(bone);

        //Update the hierarchy until no more parent is available
        while (bone.Parent != null) {
            //Assign the parent
            MJoint finalBone = bone;
            bone = posture.Joints.stream().filter(s -> s.ID.equals(finalBone.getParent())).findFirst().orElse(null);
            if (bone != null) {
                hierarchy.add(0, bone);
            } else {
                //Break while loop if no more bone
                break;
            }
        }


        MQuaternion rotation = hierarchy.get(0).Rotation.deepCopy();

        for (int i = 0; i < hierarchy.size() - 1; i++) {
            //Update rotation a postiori
            rotation = MQuaternionExtensions.multiply(rotation, hierarchy.get(i + 1).getRotation());
        }

        return rotation;
    }

    /// <summary>
    /// Returns the global rotation of the specified bone type
    /// </summary>
    /// <param name="posture"></param>
    /// <param name="boneType"></param>
    /// <returns></returns>
    public static MQuaternion GetGlobalRotation(MAvatarPosture posture, String boneName) {
        //Create an empty list which represents the hierarchy
        List<MJoint> hierarchy = new ArrayList<>();

        MJoint bone = posture.Joints.stream().filter(s -> s.ID.equals(boneName)).findFirst().orElse(null);


        if (bone == null) {
            throw new RuntimeException("Bone  type cannot be found");
        }

        hierarchy.add(bone);

        //Update the hierarchy until no more parent is available
        while (bone.Parent != null) {
            //Assign the parent
            MJoint finalBone = bone;
            bone = posture.Joints.stream().filter(s -> s.ID.equals(finalBone.getParent())).findFirst().orElse(null);
            if (bone != null) {
                hierarchy.add(0, bone);
            } else {
                //Break while loop if no more bone
                break;
            }
        }


        MQuaternion rotation = hierarchy.get(0).Rotation.deepCopy();

        for (int i = 0; i < hierarchy.size() - 1; i++) {
            //Update rotation a postiori
            rotation = MQuaternionExtensions.multiply(rotation, hierarchy.get(i + 1).getRotation());
        }

        return rotation;
    }



    /// <summary>
    /// Assigns a hand pose
    /// </summary>
    /// <param name="avatarPosture"></param>
    /// <param name="handPose"></param>
    /// <param name="ignoreWrist"></param>
    public static void AssignHandPose(MAvatarPosture avatarPosture, MHandPose handPose, boolean ignoreWrist) {
        for (MJoint handBone : handPose.Joints) {
            if (ignoreWrist && handBone.Type == LeftWrist || handBone.Type == RightWrist)
                continue;
            MJoint bone = MAvatarPostureExtensions.getJoint(avatarPosture, handBone.Type);
            if (bone == null) {
                bone = avatarPosture.Joints.stream().filter(s -> s.ID.equals(handBone.ID)).findFirst().orElse(null);
            }

            if (bone != null) {
                bone.Position = handBone.Position;
                bone.Rotation = handBone.Rotation;
            } else {
                throw new RuntimeException("bone: " + handBone.Type + " not found");
            }
        }
    }


    /// <summary>
    /// Scales the avatar posture based on the given bone lengths
    /// </summary>
    /// <param name="avatarPosture"></param>
    /// <param name="boneLengths"></param>
    public static void SetBoneLengths(MAvatarPosture avatarPosture, Map<MJointType, Float> boneLengths) throws Exception {
        if (avatarPosture.Joints == null)
            throw new Exception("Cannot scale empty avatar posture!");

        for (MJointType element : boneLengths.keySet()) {
            MJoint bone = MAvatarPostureExtensions.getJoint(avatarPosture, element);

            //Scale the bone
            if (bone != null) {
                //Get the current local position/offset of the bone
                MVector3 position = bone.Position.deepCopy();

                //Normalize local position/offset and multiply by desired length
                bone.Position = MVector3Extensions.multiply(position, boneLengths.get(element) / MVector3Extensions.magnitude(position));
            }
        }
    }


    /// <summary>
    /// Sets the local positions of the avatar posture based on an input posture
    /// </summary>
    /// <param name="posture"></param>
    /// <param name="data"></param>
    public static void SetLocalPositions(MAvatarPosture posture, MAvatarPosture data) {
        for (MJoint transform : data.Joints) {
            posture.Joints.stream().filter(s -> s.ID.equals(transform.ID)).findFirst().ifPresent(match -> match.Position = transform.Position);
        }
    }


    /// <summary>
    /// Returns a bone based on the given bone type
    /// </summary>
    /// <param name="posture"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static MJoint getJoint(MAvatarPosture posture, MJointType type) {


        return posture.Joints.stream().filter(s -> s.Type == type || s.ID.equals(type.toString())).findFirst().orElse(null);

    }


    /// <summary>
    /// Assigns the bone rotations of the bones (from source posture) specified in the type list.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    /// <param name="types">The bone rotations which should be assigned</param>
    public static void AssignBoneRotations(MAvatarPosture target, MAvatarPosture source, List<MJointType> types) {
        for (int i = 0; i < target.Joints.size(); i++) {
            MJoint joint = target.Joints.get(i);

            if (types.contains(joint.Type)) {
                MJoint match = MAvatarPostureExtensions.getJoint(target, joint.Type);
                joint.Rotation = new MQuaternion(match.Rotation.X, match.Rotation.Y, match.Rotation.Z, match.Rotation.W);
            }
        }
    }


    /// <summary>
    /// Done -> root transform not considered
    /// </summary>
    /// <param name="avatarPosture"></param>
    /// <returns></returns>
    public static MAvatarPostureValues GetPostureValues(MAvatarPosture avatarPosture) {
        //Create the new format
        MAvatarPostureValues newFormat = new MAvatarPostureValues();

        newFormat.PostureData = new ArrayList<>();
        newFormat.AvatarID = avatarPosture.AvatarID;

        if (avatarPosture.AvatarID == null)
            newFormat.AvatarID = "default";

        //Add root bone value
        newFormat.PostureData.add(avatarPosture.Joints.get(0).Position.X);
        newFormat.PostureData.add(avatarPosture.Joints.get(0).Position.Y);
        newFormat.PostureData.add(avatarPosture.Joints.get(0).Position.Z);

        newFormat.PostureData.add(avatarPosture.Joints.get(0).Rotation.X);
        newFormat.PostureData.add(avatarPosture.Joints.get(0).Rotation.Y);
        newFormat.PostureData.add(avatarPosture.Joints.get(0).Rotation.Z);
        newFormat.PostureData.add(avatarPosture.Joints.get(0).Rotation.W);

        //Add the other values
        for (int i = 1; i < avatarPosture.Joints.size(); i++) {
            newFormat.PostureData.add(avatarPosture.Joints.get(i).Rotation.X);
            newFormat.PostureData.add(avatarPosture.Joints.get(i).Rotation.Y);
            newFormat.PostureData.add(avatarPosture.Joints.get(i).Rotation.Z);
            newFormat.PostureData.add(avatarPosture.Joints.get(i).Rotation.W);
        }

        return newFormat;
    }


    /// <summary>
    /// Assigns the posture values to the respective MAvatarPosture
    /// </summary>
    /// <param name="postureValues"></param>
    /// <param name="hierarchy"></param>
    /// <returns></returns>
    public static void AssignPostureValues(MAvatarPosture posture, MAvatarPostureValues postureValues) {

        if (postureValues == null) {
            throw new RuntimeException("Posture values are null!");
        }

        if (postureValues.PostureData.size() != posture.Joints.size() * 4 + 3) {
            throw new RuntimeException("Value count does not fit to bone count of hierarchy: " + postureValues.PostureData.size() + " against " + posture.Joints.size());
        }


        //Assign the values
        if (postureValues.PostureData.size() >= 7) {
            posture.Joints.get(0).Position = new MVector3(postureValues.PostureData.get(0), postureValues.PostureData.get(1), postureValues.PostureData.get(2));
            posture.Joints.get(0).Rotation = new MQuaternion(postureValues.PostureData.get(3), postureValues.PostureData.get(4), postureValues.PostureData.get(5), postureValues.PostureData.get(6));

            int index = 1;
            for (int i = 7; i < postureValues.PostureData.size(); i += 4) {
                posture.Joints.get(index).Rotation = new MQuaternion(postureValues.PostureData.get(i), postureValues.PostureData.get(i + 1), postureValues.PostureData.get(i + 2), postureValues.PostureData.get(i + 3));
                index++;
            }
        }
    }


    /// <summary>
    /// Function to check if the specified ik constraints are already fulfilled
    /// </summary>
    /// <param name="ikProperties"></param>
    /// <param name="posture"></param>
    /// <param name="rotationThreshold"></param>
    /// <param name="translationThreshold"></param>
    /// <returns></returns>
    public static boolean IKConstraintsFulfilled(MAvatarPosture posture, List<MIKProperty> ikProperties, float translationThreshold, float rotationThreshold) {
        for (MIKProperty ikProperty : ikProperties) {
            double distance = 0f;

            switch (ikProperty.OperationType) {
                case SetPosition:

                    switch (ikProperty.Target) {
                        case LeftHand:
                            distance = MVector3Extensions.distance(MVector3Extensions.toMVector3(ikProperty.Values), MAvatarPostureExtensions.GetGlobalPosition(posture, LeftWrist));
                            break;

                        case RightHand:
                            distance = MVector3Extensions.distance(MVector3Extensions.toMVector3(ikProperty.Values), MAvatarPostureExtensions.GetGlobalPosition(posture, RightWrist));
                            break;

                        case LeftFoot:
                            distance = MVector3Extensions.distance(MVector3Extensions.toMVector3(ikProperty.Values), MAvatarPostureExtensions.GetGlobalPosition(posture, LeftAnkle));
                            break;

                        case RightFoot:
                            distance = MVector3Extensions.distance(MVector3Extensions.toMVector3(ikProperty.Values), MAvatarPostureExtensions.GetGlobalPosition(posture, RightAnkle));
                            break;

                        case Root:
                            distance = MVector3Extensions.distance(MVector3Extensions.toMVector3(ikProperty.Values), MAvatarPostureExtensions.GetGlobalPosition(posture, PelvisCentre));
                            break;
                    }

                    //Check if solving is required
                    if (distance > translationThreshold) {
                        //Set to false and break instantly
                        return false;
                    }

                    break;


                case SetRotation:

                    switch (ikProperty.Target) {
                        case LeftHand:
                            distance = MQuaternionExtensions.angle(MQuaternionExtensions.toMQuaternion(ikProperty.Values), MAvatarPostureExtensions.GetGlobalRotation(posture, LeftWrist));
                            break;

                        case RightHand:
                            distance = MQuaternionExtensions.angle(MQuaternionExtensions.toMQuaternion(ikProperty.Values), MAvatarPostureExtensions.GetGlobalRotation(posture, RightWrist));
                            break;

                        case LeftFoot:
                            distance = MQuaternionExtensions.angle(MQuaternionExtensions.toMQuaternion(ikProperty.Values), MAvatarPostureExtensions.GetGlobalRotation(posture, LeftAnkle));
                            break;

                        case RightFoot:
                            distance = MQuaternionExtensions.angle(MQuaternionExtensions.toMQuaternion(ikProperty.Values), MAvatarPostureExtensions.GetGlobalRotation(posture, RightAnkle));
                            break;

                        case Root:
                            distance = MQuaternionExtensions.angle(MQuaternionExtensions.toMQuaternion(ikProperty.Values), MAvatarPostureExtensions.GetGlobalRotation(posture, PelvisCentre));
                            break;
                    }

                    if (distance > rotationThreshold) {
                        return false;
                    }

                    break;
            }
        }

        return true;
    }
}
