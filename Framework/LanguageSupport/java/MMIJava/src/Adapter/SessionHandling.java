package Adapter;


import Utils.LogLevel;
import Utils.Logger;

public class SessionHandling {
    /*
            Helper class for session handling
    */

    //	Deletes a session content based on the session id
    public static void removeSessionContent(String sessionId) throws RuntimeException {
        String[] arr = SessionTools.getSplittedIDs(sessionId);
        String sceneId = arr[0];
        String avatarId = arr[1];

        SessionContent content = SessionHandling.getSessionContentBySceneID(sceneId);

        if (SessionData.sessionContents.remove(sceneId) == null)
            throw new RuntimeException("Can not find Session content with ID: " + sceneId);
    }


    //	Creates a new session content
    public static SessionContent createSessionContent(String sessionId) throws RuntimeException {
        //Get the ids
        String[] arr = SessionTools.getSplittedIDs(sessionId);
        String sceneId = arr[0];
        String avatarId = arr[1];

        //Create a new session content
        SessionContent sessionContent = null;

        //check if session content is already available
        if (SessionData.sessionContents.containsKey(sceneId)) {
            sessionContent = SessionData.sessionContents.get(sceneId);
            AvatarContent avatarContent = new AvatarContent(avatarId);
            if (sessionContent.AvatarContent.putIfAbsent(avatarId, avatarContent) != null) {
                throw new RuntimeException("Unable to create session: session and avatar content already available");
            }
        } else {
            Logger.printLog(LogLevel.L_INFO, "Create new session :" + sessionId);
            sessionContent = new SessionContent(sceneId);
            AvatarContent avatarContent = new AvatarContent(avatarId);
            sessionContent.AvatarContent.putIfAbsent(avatarId, avatarContent);
            SessionData.sessionContents.putIfAbsent(sceneId, sessionContent);
        }
        return sessionContent;
    }

    //	 Returns the session content based on the session id
    static SessionContent getSessionContentBySceneID(String sceneID) throws RuntimeException {
        SessionContent result = SessionData.sessionContents.get(sceneID);
        if (result != null)
            return result;
        throw new RuntimeException("Can not find session content with ID: " + sceneID);
    }

    static SessionContent getSessionContentBySessionID(String sessionID) throws RuntimeException {
        return getSessionContentBySceneID(SessionTools.getSplittedIDs(sessionID)[0]);
    }


    static MotionModelUnitBase getMMUbyId(String sessionID, String mmuID) throws RuntimeException {

        return getAvatarContentBySessionID(sessionID).getMMUbyId(mmuID);
    }

    static AvatarContent getAvatarContentBySessionID(String sessionID) throws RuntimeException {
        String[] splittedIDs = SessionTools.getSplittedIDs(sessionID);
        return getSessionContentBySessionID(splittedIDs[0]).getAvatarContentByAvatarID(splittedIDs[1]);
    }

}
