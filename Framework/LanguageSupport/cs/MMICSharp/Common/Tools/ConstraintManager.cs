// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MMICSharp.Common.Tools
{
    /// <summary>
    /// A class which helps accessing and managing constraints (within MMUs)
    /// </summary>
    public class ConstraintManager
    {
        /// <summary>
        /// The assigned constraints
        /// </summary>
        protected List<MConstraint> constraints;

        /// <summary>
        /// The assigned scene acess
        /// </summary>
        protected readonly MSceneAccess.Iface sceneAccess;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ConstraintManager(MSceneAccess.Iface sceneAccess)
        {
            this.sceneAccess = sceneAccess;
        }

        /// <summary>
        /// Method to set new constraints
        /// </summary>
        /// <param name="constraints"></param>
        public virtual void SetConstraints(ref List<MConstraint> constraints)
        {
            this.constraints = constraints;
        }


        /// <summary>
        /// Indicates whether an endeffector constraint for the given type is defined
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual bool HasEndeffectorConstraint(MJointType type)
        {
            return this.constraints.Exists(s => s.JointConstraint != null && s.JointConstraint.JointType == type);
        }

        /// <summary>
        /// Indicates whether a constraint with the given id is available
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool HasConstraint(string id)
        {
            return this.constraints.Exists(s => s.ID == id);
        }


        /// <summary>
        /// Returns the endeffector constraint
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual MJointConstraint GetEndeffectorConstraint(string id)
        {
            MConstraint constraint = this.constraints.Find(s => s.JointConstraint != null && s.ID == id);

            if (constraint == null || constraint.JointConstraint == null)
                return null;

            return constraint.JointConstraint;
        }


        /// <summary>
        /// Returns the endeffector constraint
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual MJointConstraint GetEndeffectorConstraint(MJointType type)
        {
            MConstraint constraint = this.constraints.Find(s => s.JointConstraint != null && s.JointConstraint.JointType == type);

            if (constraint == null || constraint.JointConstraint == null)
                return null;

            return constraint.JointConstraint;
        }


        /// <summary>
        /// Removes all endeffector constraints for the specific type
        /// </summary>
        /// <param name="type"></param>
        public virtual void RemoveEndeffectorConstraints(MJointType type)
        {
            List<MConstraint> conflictingConstraints = this.constraints.Where(s => s.JointConstraint != null && s.JointConstraint.JointType == type).ToList();

            if (conflictingConstraints != null)
                foreach (MConstraint constrToRemove in conflictingConstraints)
                {
                    this.constraints.Remove(constrToRemove);
                }
        }


        /// <summary>
        /// Sets a new endeffector constraint and replaces old ones if available
        /// </summary>
        /// <param name="newConstraint"></param>
        /// <returns></returns>
        public virtual void SetEndeffectorConstraint(MJointConstraint newConstraint, String id = null)
        {
            //Create a new id if null
            if(id == null)
                id = Guid.NewGuid().ToString();

            this.RemoveEndeffectorConstraints(newConstraint.JointType);

            //Create a new constraint 
            MConstraint constraint = new MConstraint(id)
            {
                JointConstraint = newConstraint
            };

            this.constraints.Add(constraint);
        }


        /// <summary>
        /// Sets an endeffector constraint with a given local position and rotation and a parent
        /// </summary>
        /// <param name="type"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parentID">The id of the parent</param>
        public virtual void SetEndeffectorConstraint(MJointType type, MVector3 position, MQuaternion rotation, String id = null, String parentID = "")
        {
            //Create a new endeffector constriant using the MJointConstraint
            this.SetEndeffectorConstraint(new MJointConstraint()
            {
                JointType = type,
                GeometryConstraint = new MGeometryConstraint()
                {
                    ParentObjectID = parentID,
                    ParentToConstraint = new MTransform(Guid.NewGuid().ToString(), position, rotation)
                },
            },id);
        }




        /// <summary>
        /// Returns the joint constraints
        /// </summary>
        /// <returns></returns>
        public virtual List<MConstraint> GetJointConstraints()
        {
            return this.constraints.Where(s => s.JointConstraint != null).ToList();
        }

        /// <summary>
        /// Combines the constraints (joint constraints are merged and overwritten, other constraints are simply added to the referenced list)
        /// </summary>
        /// <param name="constraints"></param>
        public virtual void Combine(List<MConstraint> constraints)
        {
            //Integrate the newly defined onces
            foreach (MConstraint constraint in constraints)
            {
                //Merge the joint constraints
                if(constraint.JointConstraint !=null)
                    this.SetEndeffectorConstraint(constraint.JointConstraint);

                //Add the other constraints directly
                else
                    constraints.Add(constraint);
            }             
        }


        /// <summary>
        /// Removes all constraints
        /// </summary>
        public virtual void Clear()
        {
            this.constraints.Clear();
        }
    }
}
