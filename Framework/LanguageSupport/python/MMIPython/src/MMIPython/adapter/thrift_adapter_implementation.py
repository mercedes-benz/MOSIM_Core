## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald, Janis Sprenger


 #-*- coding: utf-8 -*-
"""

"""
import os
import json
from datetime import datetime
from MMIStandard.avatar.ttypes import MAvatarDescription, MAvatarDescription
from MMIStandard.scene.ttypes import MSceneUpdate, MSceneManipulation
from MMIStandard.core.ttypes import MBoolResponse, MIPAddress
from MMIStandard.mmu.ttypes import MSimulationState, MInstruction
from MMIStandard.register.ttypes import MAdapterDescription
from MMIStandard.register import MMIAdapter, MMIRegisterService
#from MMIStandard.ttypes import MAvatarPosture, MInstruction, MAvatarState, MAvatarDescription, MSceneManipulation, MIPAddress
#from MMIStandard import MMIAdapter

from MMIPython.core.utils.description_utils import create_mmu_description
from MMIPython.core.services.service_access import ServiceAccess

from MMIPython.adapter.session.session_data import SessionData

from thrift.transport import TSocket
from thrift.transport import TTransport
from thrift.protocol import TCompactProtocol
from thrift.server import TServer

import MMIPython.core.scene.remote_scene_access as scene_access

class ThriftAdapterImplementation(MMIAdapter.Iface):
    
    """
    The implementation of the MMIAdapter
    
    """
    
    def __init__(self, session_data): #r_ip_address):

        """
        The basic constructor of the adapter
        
        Parameters
        ----------
        r_ip_address : MIPAddress
            The register MIPAddress used for the services
        """
        
        #assert (isinstance(r_ip_address, MIPAddress)),"r_ip_address is no MIPAddress"
        
        self.session_data = session_data       
   
 
        if __debug__:
            print('Adapter started in debug mode')
        else:
            print('Adapter started in optimized mode')
        
        
       
    
    def Initialize(self, avatarDescription, properties, mmuID, sessionID):
        """
        Basic initialization of a MMU
        
        Parameters
        ----------
        referenceAvatar : MAvatarDescription
            The reference avatar description
        properties : dict
            The properties for initialization of the MMU
        mmuID : str
            The mmu ID
        sessionID : str
            The session ID
            
        Returns
        -------
        MBoolResponse
        """
        if __debug__:
            print('--------------')
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmuID, str)),"mmuID is no string"
        assert (isinstance(avatarDescription, MAvatarDescription)),"avatarDescritption is no MAvatarDescription"
        assert (isinstance(properties, dict)),"properties is no dict"
        assert (all(isinstance(x, str) for x in properties.keys())), "Not all keys are of type string in properties"
        assert (all(isinstance(x, str) for x in properties.values())), "Not all values are of type string in properties"

        
        # get the mmu
        mmu = self.session_data.GetMMU(sessionID, mmuID)
        if mmu is not None:
            self.session_data.last_access =datetime.now()
            return mmu.Initialize(avatarDescription, properties)
    
        else:
             return MBoolResponse(False,["MMU " + mmuID + "with session :" + sessionID+  " could not be initialized"])

        """
        if mmu is not None:
            result = mmu.initialize(avatarDescription, properties)
        
            # Update last access
            SessionData.last_access =datetime.now()
            
            if initialized:
                print('Initialized MMU {1} with session : {0}'.format(sessionID, mmuID))
            else:
                print('MMU {1} with session : {0} could not be initialized'.format(sessionID, mmuID))
            
        else:
            print('MMU {1} with session : {0} not found'.format(sessionID, mmuID))
        
        if __debug__:
            print('--------------')
        """
        #return initialized
        

    def AssignInstruction(self, instruction, simulationState, mmuID, sessionID):
        """
        Assigns a command to a MMU
        
        Parameters
        ----------
        instruction : MInstruction
            The reference avatar posture
        avatarState : MAvatarState
            The MAvatarState
        mmuID : str
            The mmu ID
        sessionID : str
            The session ID
            
        Returns
        -------
        MBoolResponse
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmuID, str)),"mmuID is no string"
        assert (isinstance(instruction, MInstruction)), "instruction is no MInstruction"
        assert (isinstance(simulationState, MSimulationState)), "simulationState is no MSimulationState"
        
        print('Assign instruction to MMU {1} with session : {0}'.format(sessionID, mmuID))
        
        
        # Get the avatar content for the stored posture hierarchy
        avatar_content = self.session_data.get_avatar_content(sessionID)
        
        # Update last access
        self.session_data.last_access =datetime.now()
        
        # get mmu
        mmu = self.session_data.GetMMU(sessionID, mmuID)
        
        if mmu is not None:
            # Assign instruction
            return mmu.AssignInstruction(instruction, simulationState)
        else:
            return MBoolResponse(False,["MMU " + mmuID + "with session :" + sessionID+  " could not be found"])
      

    def DoStep(self, time, simulationState, mmuID, sessionID):
        """
        Basic do step routine which triggers the simulation update of the repsective MMU
        
        Parameters
        ------------
        time : float
            The simulation time
        avatarState : MAvatarState
            The incoming MAvatarState
        mmuID : str
            The mmuID
        sessionID : str
            The session ID
        
        Returns
        -------
        MMUResult
            The result of the MMU
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmuID, str)),"mmuID is no string"
        assert (isinstance(simulationState, MSimulationState)), "simulationState is no MSimulationState"
        assert (isinstance(time, float)),"time is no float"
        
        mmu_result = None
        print("Do Step")
        
        # Get the avatar content for the stored posture hierarchy
        avatar_content = self.session_data.get_avatar_content(sessionID)
        
        # get mmu
        mmu = self.session_data.GetMMU(sessionID, mmuID)
        # Update last access
        self.session_data.last_access =datetime.now()

        if mmu is not None:
            # Do step
           return mmu.DoStep(time,simulationState)
        else:
           print('MMU {1} with session : {0} could net be found'.format(sessionID, mmuID))
           return None

    def GetBoundaryConstraints(self, instruction, mmuID, sessionID):
        """
        Returns constraints which are relevant for the transition
        
        Parameters
        ------------
        instruction : MInstruction
            The given motion instruction
        mmuID : str
            The mmuID
        sessionID : str
            The session ID
        
        Returns
        -------
        list<MConstraint>
        """

        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmuID, str)),"mmuID is no string"
        assert (isinstance(instruction, MInstruction)), "instruction is no MInstruction"

        print('Get BoundardyConstraints of the MMU {1} with session : {0}'.format(sessionID, mmuID))

          # get mmu
        mmu = self.session_data.GetMMU(sessionID, mmuID)
        
        if mmu is not None:
            return mmu.GetBoundaryConstraints(instruction)
        else:
            print('MMU {1} with session : {0} could net be found'.format(sessionID, mmuID))
            return None
          
           

    def CheckPrerequisites(self, instruction, mmuID, sessionID):
        """
        
        Returns the prerequisites to start the given motion instruction
        
        Parameters
        -------------
        instruction : MInstruction
            The given motion instruction
        mmuID : str
            The mmuID
        sessionID : str
            The session ID
            
        Returns
        ----------
        list<MConstraints>
            Prerequisites
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmuID, str)),"mmuID is no string"
        assert (isinstance(instruction, MInstruction)), "instruction is no MInstruction"
        
        print('Check prerequisites of the MMU {1} with session : {0}'.format(sessionID, mmuID))
        
        
        # get mmu
        mmu = self.session_data.GetMMU(sessionID, mmuID)
        
        if mmu is not None:
            
#            if __debug__:
#                if mmu.name == "PythonAbstractionTestMMU":
#                    
#                    sessID, avID = session_utils.get_splitted_IDs(sessionID)
#                    
#                    if instruction.Properties is None:
#                        instruction.Properties = dict()
#                        
#                    instruction.Properties["sessionID"] = sessID
#                    instruction.Properties["avatarID"] = avID
#                    instruction.Properties["manageScene"] = "False"
#                    instruction.Properties["address"] = self._session_data.service_access._register_ip_address
                
            # Get prerequisites
            return mmu.CheckPrerequisites(instruction)
        else:
            print('MMU {1} with session : {0} could net be found'.format(sessionID, mmuID))
            return None
    
    def CloseSession(self, sessionID):
        return self.session_data.removeSessionContent(sessionID)

    def Abort(self, instructionID, mmuID, sessionID):
        """
        
        Forces the termination of a MMU
        
        Parameters
        ------------
        mmuID : str
            The mmuID
        sessionID : str
            The session ID
            
        Returns
        -------
        MBoolResponse
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmuID, str)),"mmuID is no string"
        
        print('Abort the MMU {1} with session : {0}'.format(sessionID, mmuID))
        # Update last access
        self.session_data.last_access =datetime.now()
        
        # get mmu
        mmu = self.session_data.GetMMU(sessionID, mmuID)
        
        if mmu is not None:
            # Abort mmu
            return mmu.Abort(instructionID)
        else:
            return MBoolResponse(False,["MMU " + mmuID + "with session :" + sessionID+  " could not be found"])



    def Dispose(self, mmuID, sessionID):
        """
        
        Called to release ressources from a MMU.
        
        Parameters
        ------------
        mmuID : str
            The mmuID
        sessionID : str
            The session ID
            
        Returns
        -------
        MBoolResponse
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmuID, str)),"mmuID is no string"
        
 
        
        print('Dispose the MMU {1} with session : {0}'.format(sessionID, mmuID))
        
        # get mmu
        mmu = self.session_data.GetMMU(sessionID, mmuID)
        
        if mmu is not None:
            # Dispose mmu
            return mmu.Dispose()
        else:
            return MBoolResponse(False,["MMU " + mmuID + "with session :" + sessionID+  " could not be found"])  

    def GetStatus(self):
        """
        Returns the status of this adapter as dictionary.
        
        Returns
        ---------
        dict
            The status as key value pairs.
        """
        
        #print('Get the adapter status')
        
        status = dict()

        status["Version"] = '0.0.2'
        status["Running since"] = self.session_data.start_time.strftime("%Y-%m-%d %H:%M:%S")
        status["Total Sessions"] = str(len(self.session_data.session_contents))
        status["Loadable MMUs"] = str(len(self.session_data.mmu_descriptions))

        status["Version"] = "5";

        if self.session_data.last_access is None:
            status["Last Access"] = "None"
        else:
            status["Last Access"] = self.session_data.last_access.strftime("%Y-%m-%d %H:%M:%S")
        
        
        return status;

    def GetAdapterDescription(self):
        return self.session_data.adapter_description

    def CreateSession(self, sessionID):
        """
        
        Creates a session with a given session ID
        
        Parameters
        ----------
        sessionID : str
            The session ID
       
        Returns
        -------
        MBoolResponse
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        
        print('Create a session with ID : {0}'.format(sessionID))
        
        # Create session content
        self.session_data.create_session_content(sessionID)
        
        return MBoolResponse(True,None)

    def PushScene(self, sceneUpdates, sessionID):
        """
        Method to synchronize the scene
        
        Parameters
        ----------
        sceneUpdates: MSceneUpdate
            The scene changes
        sessionID : str
            The session ID
       
        Returns
        -------
        MBoolResponse
        """
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(sceneUpdates, MSceneUpdate)),"sceneUpdates is no MSceneUpdate"
    
        # Get the session content
        session_content = self.session_data.get_session_content(sessionID)
        
        # if session_content is not None:
        #     return session_content.scene_access.apply(sceneUpdates)

        return MBoolResponse(True)



   

    def GetLoadableMMUs(self):
        """
        Returns the description of all loadable MMUs from this adapter.
        
        Returns
        ---------
        list<MMUDescription>
            The loadable MMUs.
        """
        
        #print('Collect loadable MMUs')
        return self.session_data.mmu_descriptions

    def GetMMus(self, sessionID):
        """
        Returns all loaded MMUs for the given session
        
        Parameters
        -----------
        sessionID : str
            The session ID
        """
        assert (isinstance(sessionID, str)),"SessionID is no string"
        
        print('Get loaded MMUs for session : {0}'.format(sessionID))
        
        mmus = list()
        
        mmus = self.session_data.GetLoadedMMUs(sessionID)
        # # Get the session content
        # avatar_content = self.session_data.get_avatar_content(sessionID, True)
        
        # if avatar_content is not None:
            
        #     # Get the mmus
        #     mmus = list(avatar_content.mmus.keys())
        mmus = [self.GetDescription(x, sessionID) for x in mmus]
            
        print('Return loaded MMUs {1} for session : {0}'.format(sessionID, mmus))
            
        return mmus

    def GetDescription(self, mmuID, sessionID):
        """
        
        Returns the description of a MMU.
        
        Parameters
        ------------
        mmuID : str
            The mmuID
        sessionID : str
            The session ID
        """
        
        print('Get description for session : {0}'.format(sessionID))
        
        return next((x for x in self.session_data.mmu_descriptions if x.ID == mmuID), None)

    def GetScene(self, sessionID):
        """
        Returns all objects from the scene with the given session ID.
        
        Returns
        ---------
        list<MSceneObject>
            The scene objects.
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        
        print('Get scene for session : {0}'.format(sessionID))
        
        
        # get session content
        session_content = self.session_data.get_session_content(sessionID)
        
        if session_content is not None:
            
            # get all scene obects from the scene access
            return session_content.scene_access.GetSceneObjects()
            
        

    def GetSceneChanges(self, sessionID):
        """
        Returns all scene manipulations from the scene with the given session ID.
        
        Returns
        ---------
        list<MSceneManipulation>
            The scene manipulations.
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        
        print('Get scene changes for session : {0}'.format(sessionID))
        
        scene_manipulations = list()
        
        # get session content
        session_content = self.session_data.get_session_content(sessionID)
        
        if session_content is not None:
            
            # get all scene obects from the scene access
            scene_manipulations = session_content.scene_access.scene_events
            
        return scene_manipulations

    def LoadMMUs(self, mmus, sessionID):
        """
       
        Loads all selected mmus to the given session.
        
        Parameters
        ------------
        mmus : list
            The mmu names
        sessionID : str
            The session ID
            
        Returns
        -------
        MBoolResponse
        """
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmus, list)),"mmus is no list"
        
        print('Load mmus for session : {0}'.format(sessionID))

        print("mmus: ", mmus)
        check = {}

        for mmu in mmus:
            if self.session_data.LoadMMU(sessionID, mmu):
                check[mmu] = "tbd"
        
        return check
                    

    def CreateCheckpoint(self, mmuID, sessionID):
        """
        Creates a checkpoint for the specified MMU.
        The checkpoint contains the internal state of each MMU which can be later used to restore the state.
        
        Parameters:
        ----------
        mmuID : str
            The mmu ID
        sessionID : str
            The session ID
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmuID, str)),"mmuID is no str"
        
        print('Create checkpoint for mmu : {1} with session : {0}'.format(sessionID, mmuID))

          
        # get mmu
        mmu = self.session_data.GetMMU(sessionID, mmuID)

        if mmu is not None:
           return mmu.CreateCheckPoint() 
        else:
            print('MMU {1} with session : {0} could net be found'.format(sessionID, mmuID))
            return None

        
        
    def RestoreCheckpoint(self, mmuID, sessionID, data):
        """
        Restores a checkpoint for the specified MMU.
        
        Parameters:
        ----------
        mmuID : str
            The mmu IDs
        sessionID : str
            The session ID
        data : bytearray
            The checkpoint data
            
        Returns
        -------
        bool
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmuID, str)),"mmuID is no string"
        print(type(data))
        assert (isinstance(data, bytes)),"data is no bytes"
        
        print('Restore checkpoint for mmu {1} with session : {0} with checkpoint : {1}'.format(sessionID, mmuID))
        
         # get mmu
        mmu = self.session_data.GetMMU(sessionID, mmuID)

        if mmu is not None:
           return mmu.RestoreCheckPoint(data)
        else:
            print('MMU {1} with session : {0} could net be found'.format(sessionID, mmuID))
            return None


    def ExecuteFunction(self, name, parameters, mmuID, sessionID):
        assert (isinstance(name, str)),"SessionID is no string"
        assert (isinstance(parameters, dict)),"SessionID is no string"

        # get mmu
        mmu = self.session_data.GetMMU(sessionID, mmuID)

        if mmu is not None:
           return mmu.ExecuteFunction(name,parameters)
        else:
            print('MMU {1} with session : {0} could net be found'.format(sessionID, mmuID))
            return None

    def create_response(self,result,message):
        if result.LogData is None:
            result.LogData=[message]
        else:
            result.LogData.append(message)
        
        return result



        