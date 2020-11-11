// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

namespace MMIStandard
{
    public static class MAvatarExtensions
    {
        public static MAvatar Clone(this MAvatar avatar)
        {
            MAvatar clone = new MAvatar();
            clone.ID = avatar.ID;
            clone.Name = avatar.Name;

            //To do -> proper cloning
            clone.Description = avatar.Description;
            clone.PostureValues = avatar.PostureValues;
            clone.SceneObjects = avatar.SceneObjects;

            return clone;
        }
    }
}
