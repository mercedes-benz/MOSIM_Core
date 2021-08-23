using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MMIUnity.Retargeting
{
    public abstract class IJointAlignment
    {
        public abstract void AlignAvatar(ISVisualizationJoint root);
    }
}
