// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Felix Gaisbauer

package Adapter;

import Utils.LogLevel;
import Utils.Logger;
import de.mosim.mmi.avatar.MAvatarDescription;
import de.mosim.mmi.constraints.MConstraint;
import de.mosim.mmi.core.MBoolResponse;
import de.mosim.mmi.mmu.*;
import de.mosim.mmi.register.MAdapterDescription;
import de.mosim.mmi.register.MMIAdapter;
import de.mosim.mmi.scene.*;
import org.apache.thrift.TException;

import java.nio.ByteBuffer;
import java.text.DateFormat;
import java.util.*;
import java.util.stream.Collectors;

public class ThriftAdapterImplementation implements MMIAdapter.Iface {

    /**
     * Implementation of the thrift adapter functionality
     */

    //	Basic initialization of a MMMU
    @Override
    public MBoolResponse Initialize(MAvatarDescription avatarDescription, Map<String, String> properties, String mmuID, String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "Initialize");
        SessionData.lastAccess = System.currentTimeMillis();

        try {
            return SessionHandling.getMMUbyId(sessionID, mmuID).Initialize(avatarDescription, properties);
        } catch (Exception e) {
            MBoolResponse result = new MBoolResponse(false);
            String message = e.getMessage();
            Logger.printLog(LogLevel.L_ERROR, message);
            result.addToLogData(message);
            return result;
        }
    }

    //	Execute command of a MMU
    @Override
    public MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState, String mmuID, String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "AssignInstruction");
        SessionData.lastAccess = System.currentTimeMillis();

        try {
            return SessionHandling.getMMUbyId(sessionID, mmuID).AssignInstruction(instruction, simulationState);
        } catch (Exception e) {
            MBoolResponse result = new MBoolResponse(false);
            String message = e.getMessage();
            Logger.printLog(LogLevel.L_ERROR, message);
            result.addToLogData(message);
            return result;
        }
    }

    //	Basic do step routine which triggers the simulation update of the respective MMU
    @Override
    public MSimulationResult DoStep(double time, MSimulationState simulationState, String mmuID, String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "DoStep");
        SessionData.lastAccess = System.currentTimeMillis();

        try {
            return SessionHandling.getMMUbyId(sessionID, mmuID).DoStep(time, simulationState);
        } catch (Exception e) {
            Logger.printLog(LogLevel.L_ERROR, e.getMessage());
            return new MSimulationResult();
        }
    }

    //	Returns constraints which are relevant for the transition
    @Override
    public List<MConstraint> GetBoundaryConstraints(MInstruction instruction, String mmuID, String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "GetBoundaryConstraints");
        //System.out.println(" GetBoundaryConstraints");
        SessionData.lastAccess = System.currentTimeMillis();
        try {
            return SessionHandling.getMMUbyId(sessionID, mmuID).GetBoundaryConstraints(instruction);
        } catch (Exception e) {
            Logger.printLog(LogLevel.L_ERROR, e.getMessage());
            return new ArrayList<>();
        }
    }

    //	Check whether the instruction can be executed given the current state
    @Override
    public MBoolResponse CheckPrerequisites(MInstruction instruction, String mmuID, String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "CheckPrerequisites");
        SessionData.lastAccess = System.currentTimeMillis();

        try {
            return SessionHandling.getMMUbyId(sessionID, mmuID).CheckPrerequisites(instruction);
        } catch (Exception e) {
            MBoolResponse result = new MBoolResponse(false);
            String message = e.getMessage();
            Logger.printLog(LogLevel.L_ERROR, message);
            result.addToLogData(message);
            return result;
        }
    }

    //	Method which forces the MMU to finish
    @Override
    public MBoolResponse Abort(String instructionID, String mmuID, String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "Abort");
        SessionData.lastAccess = System.currentTimeMillis();

        try {
            return SessionHandling.getMMUbyId(sessionID, mmuID).Abort(instructionID);
        } catch (Exception e) {
            MBoolResponse result = new MBoolResponse(false);
            String message = e.getMessage();
            Logger.printLog(LogLevel.L_ERROR, message);
            result.addToLogData(message);
            return result;
        }
    }

    //	Method disposes the MMU
    @Override
    public MBoolResponse Dispose(String mmuID, String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "Dispose");
        SessionData.lastAccess = System.currentTimeMillis();

        try {
            return SessionHandling.getMMUbyId(sessionID, mmuID).Dispose(new HashMap<>());
        } catch (Exception e) {
            MBoolResponse result = new MBoolResponse(false);
            String message = e.getMessage();
            Logger.printLog(LogLevel.L_ERROR, message);
            result.addToLogData(message);
            return result;
        }
    }

    //	Method for executing an arbitrary function (optionally)
    @Override
    public Map<String, String> ExecuteFunction(String name, Map<String, String> parameters, String mmuID, String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "ExecuteFunction");
        SessionData.lastAccess = System.currentTimeMillis();

        try {
            return SessionHandling.getMMUbyId(sessionID, mmuID).ExecuteFunction(name, parameters);
        } catch (Exception e) {
            Logger.printLog(LogLevel.L_ERROR, e.getMessage());
            return new HashMap<>();
        }
    }

    //	Returns the status of the adapter
    @Override
    public Map<String, String> GetStatus() throws TException {
        // System.out.println("GetStatus");

        Map<String, String> temp = new HashMap<>();
        DateFormat df = DateFormat.getDateTimeInstance(DateFormat.FULL, DateFormat.LONG, Locale.ENGLISH);
        temp.put("Version", "0.1");
        Date date = new Date(SessionData.startTime);
        temp.put("Running since", df.format(date));
        temp.put("Total Sessions", Integer.toString(SessionData.sessionContents.size()));
        temp.put("Loadable MMUs", Integer.toString(SessionData.MMUDescriptions.size()));

        if (SessionData.lastAccess == 0) {
            temp.put("Last Access", "None");
        } else {
            date = new Date(SessionData.lastAccess);
            temp.put("Last Access", df.format(date));
        }
        return temp;
    }

    //	Returns the MAdapterDescription of the adapter in detail
    @Override
    public MAdapterDescription GetAdapterDescription() throws TException {
        return SessionData.adapterDescription;
    }

    //	Creates a session
    @Override
    public MBoolResponse CreateSession(String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "CreateSession");
        SessionData.lastAccess = System.currentTimeMillis();
        MBoolResponse result = new MBoolResponse(true);

        try {
            SessionHandling.createSessionContent(sessionID);
        } catch (Exception e) {
            result.setSuccessful(false);
            String message = e.getMessage();
            Logger.printLog(LogLevel.L_ERROR, message);
            result.addToLogData(message);
            return result;
        }
        return result;
    }

    //	Closes the session
    @Override
    public MBoolResponse CloseSession(String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "CloseSession");
        SessionData.lastAccess = System.currentTimeMillis();
        MBoolResponse result = new MBoolResponse(true);

        try {
            SessionHandling.removeSessionContent(sessionID);
        } catch (Exception e) {
            String message = e.getMessage();
            Logger.printLog(LogLevel.L_ERROR, message);
            result.addToLogData(message);
            return result;
        }
        return result;
    }

    //	Method to synchronize the scene
    @Override
    public MBoolResponse PushScene(MSceneUpdate sceneUpdates, String sessionID) {
        Logger.printLog(LogLevel.L_DEBUG, "PushScene");

        try {
            return SessionHandling.getSessionContentBySessionID(sessionID).SceneBuffer.Apply(sceneUpdates);
        } catch (Exception e) {
            MBoolResponse result = new MBoolResponse(false);
            String message = e.getMessage();
            Logger.printLog(LogLevel.L_ERROR, message);
            result.addToLogData(message);
            return result;
        }
    }

    //	Returns descriptions of all MMUs which can be loaded
    @Override
    public List<MMUDescription> GetLoadableMMUs() throws TException {
        //Logger.printLog(LogLevel.L_DEBUG,"GetLoadableMMUs");
        return SessionData.MMUDescriptions;
    }

    //	Returns all MMUs of the session
    @Override
    public List<MMUDescription> GetMMus(String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "GetMMUs");
        SessionData.lastAccess = System.currentTimeMillis();

        try {
            AvatarContent avatarContent = SessionHandling.getAvatarContentBySessionID(sessionID);
            return SessionData.MMUDescriptions.stream().filter(mmuDescription -> avatarContent.MMUs.containsKey(mmuDescription.ID)).collect(Collectors.toList());
        } catch (Exception e) {
            Logger.printLog(LogLevel.L_ERROR, e.getMessage());
            return new ArrayList<>();
        }
    }

    //	Returns the description of the MMU
    @Override
    public MMUDescription GetDescription(String mmuID, String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "GetDescription");
        SessionData.lastAccess = System.currentTimeMillis();

        return SessionData.MMUDescriptions.stream().filter(desc -> desc.Name.equals(mmuID)).findFirst().orElse(null);
    }

    //	Returns the whole scene
    @Override
    public List<MSceneObject> GetScene(String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "GetScene");
        SessionData.lastAccess = System.currentTimeMillis();

        try {
            return SessionHandling.getSessionContentBySessionID(sessionID).SceneBuffer.GetSceneObjects();
        } catch (Exception e) {
            Logger.printLog(LogLevel.L_ERROR, e.getMessage());
            return new ArrayList<>();
        }
    }

    //	Returns the scene changes of the current frame
    @Override
    public MSceneUpdate GetSceneChanges(String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "GetSceneChanges");
        SessionData.lastAccess = System.currentTimeMillis();

        try {
            return SessionHandling.getSessionContentBySessionID(sessionID).SceneBuffer.GetSceneChanges();
        } catch (Exception e) {
            Logger.printLog(LogLevel.L_ERROR, e.getMessage());
            return new MSceneUpdate();
        }
    }

    //	Method loads MMUs for the specific session
    @Override
    public HashMap<String,String> LoadMMUs(List<String> mmus, String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "LoadMMUs");
        SessionData.lastAccess = System.currentTimeMillis();

        //To do
        HashMap<String, String> mmuInstanceMapping = new HashMap<String, String>();



        try {
            String[] arr = SessionTools.getSplittedIDs(sessionID);
            String sceneId = arr[0];
            String avatarId = arr[1];
            SessionContent sessionContent = SessionHandling.getSessionContentBySceneID(sceneId);

            for (String id : mmus) {
                //Tuple<String,MMUDescription> loadProperty = SessionData.MMULoadingProperties.stream().filter(s->s.y.ID.equals(id)).findAny().orElse(null);
                MMUDescription mmuDescription = SessionData.MMUZipEntry.get(id);
                if (mmuDescription != null) {
                    MotionModelUnitBase mmu = AdapterController.getMMUInstantiator().InstantiateMMU(mmuDescription);

                    if (mmu != null) {
                        //Assign the service access
                        mmu.ServiceAccess = sessionContent.serviceAccess;

                        //Assign the scene
                        mmu.SceneAccess = sessionContent.SceneBuffer;

                        Logger.printLog(LogLevel.L_INFO, "Loaded MMU: " + mmuDescription.getName() + " for session: " + sessionID);
                        AvatarContent avatarContent = sessionContent.AvatarContent.get(avatarId);

                        if (avatarContent == null) {
                            avatarContent = new AvatarContent(avatarId);
                            sessionContent.AvatarContent.putIfAbsent(avatarId, avatarContent);
                        }
                        avatarContent.MMUs.put(mmuDescription.getID(), mmu);
                    }
                }
            }
        } catch (Exception e) {
            String message = e.getMessage();
            Logger.printLog(LogLevel.L_ERROR, message);
            //result.addToLogData(message);
            //result.setSuccessful((false));
        }
        return mmuInstanceMapping;
    }

    //	Method creates checkpoint of the given MMU
    @Override
    public ByteBuffer CreateCheckpoint(String mmuID, String sessionID) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "CreateCheckpoint");
        SessionData.lastAccess = System.currentTimeMillis();

        try {
            return SessionHandling.getMMUbyId(sessionID, mmuID).CreateCheckpoint();
        } catch (Exception e) {
            Logger.printLog(LogLevel.L_ERROR, e.getMessage());
            return ByteBuffer.wrap(null);
        }
    }

    //	Restores the checkpoint of the given MMU
    @Override
    public MBoolResponse RestoreCheckpoint(String mmuID, String sessionID, ByteBuffer checkpointData) throws TException {
        Logger.printLog(LogLevel.L_DEBUG, "RestoreCheckpoint");
        SessionData.lastAccess = System.currentTimeMillis();

        try {
            return SessionHandling.getMMUbyId(sessionID, mmuID).RestoreCheckpoint(checkpointData);
        } catch (Exception e) {
            MBoolResponse result = new MBoolResponse(false);
            String message = e.getMessage();
            Logger.printLog(LogLevel.L_ERROR, message);
            result.addToLogData(message);
            return result;
        }
    }
}
