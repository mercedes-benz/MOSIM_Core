// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Felix Gaisbauer

package Adapter;

import Access.Abstraction.ServiceAccess;

import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;

public class SessionContent {
    /*
		Class which contains the content which is related to the specific session
	*/

    //	The corresponding service access
    public final ServiceAccess serviceAccess;
    //	The corresponding scene access
    public final MMIScene SceneBuffer;

    public final ConcurrentMap<String, AvatarContent> AvatarContent;
    //	The id of the session
    public final String SessionId;
    // The last time the session was used
    public long LastAccess;

    // Basic constructor

    public SessionContent(String sessionId) {
        this.SessionId = sessionId;
        AvatarContent = new ConcurrentHashMap<>();
        this.SceneBuffer = new MMIScene();
        this.serviceAccess = new ServiceAccess(SessionData.registerAddress, sessionId);
        //this.serviceAccess.initialize();
    }

    //  Returns the avatar content based on the id
    public AvatarContent getAvatarContentByAvatarID(String avatarId) throws RuntimeException {
        AvatarContent result = this.AvatarContent.get(avatarId);
        if (result != null)
            return result;
        throw new RuntimeException("Can not find avatar content with id: " + avatarId);
    }

    public AvatarContent getAvatarContentBySessionID(String sessionID) throws RuntimeException {
        return this.getAvatarContentByAvatarID(SessionTools.getSplittedIDs(sessionID)[1]);
    }

    //   Creates new AvatarContent
    public AvatarContent createAvatarContent(String avatarId) {
        this.AvatarContent.put(avatarId, new AvatarContent(avatarId));
        return this.AvatarContent.get(avatarId);
    }


}
