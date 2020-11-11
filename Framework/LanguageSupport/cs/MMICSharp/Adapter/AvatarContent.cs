// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMIStandard;
using System.Collections.Generic;


namespace CSharpAdapter
{
    /// <summary>
    /// Avatar specific content (e.g. the MMU instances)
    /// </summary>
    public class AvatarContent
    {
        /// <summary>
        /// The avatar id
        /// </summary>
        public string AvatarId;

        /// <summary>
        /// The list of MMUs of the session
        /// </summary>
        public Dictionary<string, IMotionModelUnitDev> MMUs = new Dictionary<string, IMotionModelUnitDev>();


        /// <summary>
        /// The posture of the reference avatar
        /// </summary>
        public MAvatarPosture ReferencePosture;


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="avatarId"></param>
        public AvatarContent(string avatarId)
        {
            this.AvatarId = avatarId;
        }
    }
}
