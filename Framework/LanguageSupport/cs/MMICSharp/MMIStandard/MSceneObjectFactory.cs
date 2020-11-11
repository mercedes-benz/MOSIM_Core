// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Generic;

namespace MMIStandard.Utils
{
    public class MSceneObjectFactory
    {
        public static MSceneObject CreateSceneObject(string name)
        {
            string id = Guid.NewGuid().ToString();

            MSceneObject sceneObject = new MSceneObject()
            {
                ID = id,
                Name = name,
                Transform = new MTransform(id, new MVector3 (0, 0, 0), new MQuaternion(0,0,0,1)),
                Properties = new Dictionary<string, string>(),
            };

            return sceneObject;       
        }

        public static MSceneObject CreateSceneObject(string name, MVector3 position, MQuaternion rotation, string parent = null)
        {
            string id = Guid.NewGuid().ToString();

            MSceneObject sceneObject = new MSceneObject()
            {
                ID = id,
                Name = name,
                Transform = new MTransform(id, position, rotation)
                {
                    Parent = parent
                },
                Properties = new Dictionary<string, string>(),
            };

            return sceneObject;
        }
    }
}
