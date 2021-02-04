// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser

package Adapter;


import de.mosim.mmi.core.MIPAddress;
import de.mosim.mmi.mmu.MMUDescription;
import de.mosim.mmi.register.MAdapterDescription;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;

public class SessionData {
    /*
		Class which contains the data of the sessions and MMUs
	*/

    //	The description of the adapter
    public static MAdapterDescription adapterDescription;
    //	The address of the MMIRegister
    public static MIPAddress registerAddress;
    //	The last time the adapter was used
    public static long lastAccess = 0;
    //	The time when the adapter was started
    public static long startTime;
    //	Map which contains all sessions
    public static ConcurrentMap<String, SessionContent> sessionContents = new ConcurrentHashMap<>();
    //  MMUID to MMUDescription mapping
    public static Map<String, MMUDescription> MMUZipEntry = new HashMap<>();
    //	Descriptions of the loadable MMUs>
    public static List<MMUDescription> MMUDescriptions = new ArrayList<>();
}
