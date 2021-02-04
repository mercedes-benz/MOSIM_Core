// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Felix Gaisbauer

package Adapter;

public class SessionTools {
    /*
		Helper class for sessions
	*/

    // returns the scene id and the avatar id based on the session id
    public static String[] getSplittedIDs(String sessionId) throws RuntimeException {

        if (sessionId == null || sessionId.equals("")) {
            throw new RuntimeException("SessionId is empty");
        }

        String[] splitted = sessionId.split(":");

        // Test if the Format was correct
        if (splitted.length == 2 && !splitted[0].equals("") && !splitted[1].equals("")) {
            return splitted;
        } else
            return new String[]{sessionId, "0"};
    }
}
