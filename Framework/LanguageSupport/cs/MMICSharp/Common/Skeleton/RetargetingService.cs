// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Janis Sprenger

using MMIStandard;
using System.Collections.Generic;
using System;

namespace MMICSharp.Common
{
    /// <summary>
    /// Implementation of the MRetargetingService interface. 
    /// 
    /// @author: Janis.Sprenger
    /// </summary>
    public class RetargetingService : MRetargetingService.Iface
    {
        
        public MServiceDescription ServiceDescription;
        private IntermediateSkeleton skeleton = new IntermediateSkeleton();
        private Dictionary<string, Dictionary<MJointType, string>> joint_mappings = new Dictionary<string, Dictionary<MJointType, string>>();
        private Dictionary<string, MAvatarPosture> basePostures = new Dictionary<string, MAvatarPosture>();
        private Dictionary<string, Dictionary<string, string>> children = new Dictionary<string, Dictionary<string, string>>();

        public RetargetingService(int port) : this("127.0.0.1", port) { }

        public RetargetingService(string ip, int port)
        {
            Console.WriteLine("Start Service at port" + port.ToString());
            var id = Guid.NewGuid().ToString();
            var name = "Retargeting";
            var language = "C#";
            var addresses = new List<MIPAddress> { new MIPAddress(ip, port) };
            ServiceDescription = new MServiceDescription(name, id, language, addresses);

            //skeleton.InitializeAnthropometry(desc);
        }
        public Dictionary<string, string> Consume(Dictionary<string, string> properties)
        {
            throw new System.NotImplementedException();
        }

        public MServiceDescription GetDescription()
        {
            return ServiceDescription;
        }

        public Dictionary<string, string> GetStatus()
        {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Retargets the global posture to the intermediate skeleton
        /// </summary>
        /// <param name="globalTarget"></param>
        /// <returns></returns>
        public MAvatarPostureValues RetargetToIntermediate(MAvatarPosture globalTarget)
        {
            RJoint root = ((RJoint)this.skeleton.GetRoot(globalTarget.AvatarID));
            bool rootFound = false;
            foreach (MJoint j in globalTarget.Joints)
            {
                if(j.Type != MJointType.Undefined)
                {
                    RJoint rj = ((RJoint)root.GetChild(j.Type));
                    rj.RetargetPositionToIS(j.Position, j.Rotation);
                    rj.RetargetRotationToIS(j.Rotation);

                    if (!rootFound)
                    {
                        rootFound = true;
                        if (j.Type == MJointType.Root)
                        {
                        } else
                        {
                            MVector3 globalPos = rj.GetGlobalPosManually();
                            root.SetGlobalPosManually(new MVector3(globalPos.X, 0, globalPos.Z));
                            //rj.SetGlobalPosManually(new MVector3(0, globalPos.Y, 0));
                        }
                    }
                }
            }

            root.RecomputeLocalTransformations();
            MAvatarPostureValues ret = new MAvatarPostureValues(globalTarget.AvatarID, root.GetAvatarPostureValues());
            return ret;
        }

        public MAvatarPosture RetargetToTarget(MAvatarPostureValues intermediatePostureValues)
        {
            string id = intermediatePostureValues.AvatarID;
            RJoint root = ((RJoint)this.skeleton.GetRoot(id));
            root.SetAvatarPostureValues(intermediatePostureValues);

            MAvatarPosture targetOut = new MAvatarPosture();
            targetOut.AvatarID = id;
            targetOut.Joints = new List<MJoint>();
            foreach(MJoint j in this.basePostures[id].Joints)
            {
                MJoint outJ = new MJoint();
                outJ.ID = j.ID;
                outJ.Type = j.Type;
                outJ.Parent = j.Parent;
                if (outJ.Type != MJointType.Undefined)
                {

                    RJoint rj = (RJoint)root.GetChild(j.Type);
                    outJ.Position = (rj).RetargetPositionToTarget();
                    outJ.Rotation = (rj).RetargetRotationToTarget();
                }else
                {
                    outJ.Position = j.Position;
                    outJ.Rotation = j.Rotation;
                }
                targetOut.Joints.Add(outJ);
            }
            Dictionary<string, string> _children = this.children[id];
            for (int i = 0; i<targetOut.Joints.Count; i++)
            {
                MJoint outJ = targetOut.Joints[i];
                if(outJ.Type == MJointType.Undefined)
                {
                    bool setRot = false;
                    bool setPos = false;
                    Console.WriteLine("no jointtype " + outJ.ID);
                    if(i == 0)
                    {
                        // find first joint that is mapped
                        foreach(MJoint j in targetOut.Joints)
                        {
                            if(j.Type != MJointType.Undefined)
                            {
                                outJ.Position = new MVector3(j.Position.X, 0, j.Position.Z);
                                //j.Position.X = 0;
                                //j.Position.Z = 0;

                                MVector3 forward = outJ.Rotation.Multiply(new MVector3(0, 0, 1));
                                forward.Y = 0;
                                forward.Normalize();
                                MVector3 currentForward = j.Rotation.Multiply(new MVector3(0, 0, 1));
                                MQuaternion drot = MVector3Extensions.FromToRotation(currentForward, forward);
                                outJ.Rotation = drot.Multiply(j.Rotation);
                                //outJ.Rotation = MQuaternionExtensions.Inverse(drot).Multiply(outJ.Rotation);

                                setPos = true;
                                setRot = true;
                                break;
                            }
                        }

                    } else
                    {
                        /*
                         * This is disabled for now, as it was not working propperly. 
                         * 
                        if(_children.ContainsKey(outJ.ID) && _children[outJ.ID] != "")
                        {
                            for(int jID = i+1; jID < targetOut.Joints.Count; jID ++)
                            {
                                MJoint j = targetOut.Joints[jID];
                                if (j.ID == _children[outJ.ID])
                                {

                                    MVector3 srcDir = new MVector3(0, 1, 0);//outJ.Rotation.Multiply(new MVector3(0, 1, 0)).Normalize
                                    MVector3 trgDir = null;
                                    MQuaternion parentRot = null;
                                    if(outJ.Parent != null)
                                    {
                                        for(int pID = i-1; pID > 0; pID--)
                                        {
                                            if(targetOut.Joints[pID].ID == outJ.Parent)
                                            {
                                                if(targetOut.Joints[pID].Type != MJointType.Undefined)
                                                {
                                                    parentRot = targetOut.Joints[pID].Rotation;
                                                    trgDir = MQuaternionExtensions.Inverse(parentRot).Multiply(j.Position.Subtract(outJ.Position).Normalize());
                                                }      
                                            }
                                        }
                                    }
                                    if(trgDir != null)
                                    {
                                        MQuaternion rot = MVector3Extensions.FromToRotation(srcDir, trgDir);
                                        outJ.Rotation = parentRot.Multiply(rot);
                                        outJ.Position = null;
                                        setRot = true;
                                        break;

                                    }


                                }
                            }
                        }*/
                    }
                    if (!setRot)
                    {
                        outJ.Rotation = null;
                    }
                    if (!setPos)
                    {
                        outJ.Position = null;
                    }
                }
            }
            return targetOut;
        }

        public MBoolResponse Setup(MAvatarDescription avatar, Dictionary<string, string> properties)
        {
            throw new System.NotImplementedException();

        }

        public MAvatarDescription SetupRetargeting(MAvatarPosture globalTarget)
        {
            Console.WriteLine("\nSetting up retargeting");
            string id = globalTarget.AvatarID;
            Dictionary<MJointType, string> joint_map = new Dictionary<MJointType, string>();
            Dictionary<string, string> _children = new Dictionary<string, string>();
            foreach(MJoint j in globalTarget.Joints)
            {
                if(j.Type != MJointType.Undefined)
                {
                    joint_map.Add(j.Type, j.ID);
                }
                if (j.Parent != null && j.Parent != "")
                {
                    if(_children.ContainsKey(j.Parent))
                    {
                        // parent has multiple children and thus cannot be auto - aligned
                        _children[j.Parent] = "";
                    } else
                    {
                        _children.Add(j.Parent, j.ID);
                    }
                }
            }
            this.children.Add(id, _children);
            if(joint_mappings.ContainsKey(id))
            {
                Console.WriteLine("Warning: Skeleton alread existing under ID " + id);
                this.joint_mappings[id] = joint_map;
            } else
            {
                this.joint_mappings.Add(id, joint_map);
            }
            if(basePostures.ContainsKey(id))
            {
                this.basePostures[id] = globalTarget;
            } else
            {
                this.basePostures.Add(id, globalTarget);
            }
            MAvatarDescription desc = IntermediateSkeleton.GenerateFromDescriptionFile(id);
            this.skeleton.InitializeAnthropometry(desc);

            Console.WriteLine("Scaling Skeleton");
            ((RJoint)this.skeleton.GetRoot(id)).ScaleSkeleton(globalTarget, joint_map);
            Console.WriteLine("Initializing Zero Posture");
            this.skeleton.GetRoot(id).SetAvatarPostureValues(null);
            Console.WriteLine("Setting Base Reference");
            ((RJoint)this.skeleton.GetRoot(id)).SetBaseReference(globalTarget);


            Console.WriteLine("Retargeting Successfully set up");

            return desc;
        }

        public IntermediateSkeleton GetSkeleton()
        {
            return this.skeleton;
        }

        public MBoolResponse Dispose(Dictionary<string, string> properties)
        {
            return new MBoolResponse(true);
        }

        public MBoolResponse Restart(Dictionary<string, string> properties)
        {
            throw new NotImplementedException();
        }
    }
}
