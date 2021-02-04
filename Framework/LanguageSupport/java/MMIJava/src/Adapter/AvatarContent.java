// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Felix Gaisbauer

package Adapter;

import java.util.HashMap;
import java.util.Map;

public class AvatarContent {
    /*
         Class which specifies the content of an avatar
     */

    //	the id of the avatar
    public final String AvatarId;

    // The list of MMUs of the session
    public final Map<String, MotionModelUnitBase> MMUs = new HashMap<>();

    //	Basic constructor
    public AvatarContent(String avatarId) {
        this.AvatarId = avatarId;
    }

    //	Returns the MMU based on the id
    MotionModelUnitBase getMMUbyId(String mmuId) throws RuntimeException {
        MotionModelUnitBase result = this.MMUs.get(mmuId);
        if (result != null)
            return result;
        throw new RuntimeException("MMU with id: " + mmuId + " not found in avatar content");
    }
}
