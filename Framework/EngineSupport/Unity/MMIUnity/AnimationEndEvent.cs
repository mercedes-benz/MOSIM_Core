// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Michael Romer, Patrick Engesser

using UnityEngine;

namespace MMIUnity
{
    /// <summary>
    /// This is a helper class for the Unity Mecanim Animator which allows a MMU to
    /// detect if an animation (e. g. a motion capture recording) has completed.
    /// </summary>
    public class AnimationEndEvent : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //This can be queried by the MMU script
            animator.SetBool("AnimationDone", true);
        }
    }
}
