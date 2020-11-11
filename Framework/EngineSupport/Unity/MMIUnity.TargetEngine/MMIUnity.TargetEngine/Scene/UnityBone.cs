// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using UnityEngine;

namespace MMIUnity.TargetEngine.Scene
{
    /// <summary>
    /// Class for representing a MMI compatible bone within unity
    /// </summary>
    public class UnityBone : MonoBehaviour
    {
        public MJointType Type = MJointType.Undefined;

        [HideInInspector]
        public string ID;

        private void Awake()
        {
            this.ID = System.Guid.NewGuid().ToString();
        }
    }
}
