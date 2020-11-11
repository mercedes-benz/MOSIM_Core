## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald


# -*- coding: utf-8 -*-
"""

"""
from MMIStandard.avatar.ttypes import MAvatarPosture, MAvatarDescription 
from MMIStandard.scene.ttypes import MSceneManipulation,  MSceneManipulation
from MMIStandard.mmu.ttypes import MMUDescription, MInstruction, MSimulationState
#from MMIStandard.ttypes import MSceneManipulation, MAvatarPosture, MMUDescription, MAvatarDescription, MInstruction, MAvatarState

import MMIPython.abstraction.mmu.motion_model_unit
import MMIPython.abstraction.mmu.mmu_access

class IAdapterAccess(object):
    
    """
    The adapter access interface.
    
    Attributes
    ----------
    description : AdapterDescription
        The description of the adapter
    initialized : bool
        Flag which indicates whether the communciaton has been successfully initialized
    aborted : bool
        Flag which indicates whether the initialization has been aborted
    scene_synchronized : bool
        Flag which indicates whether the scene has been successfully synchronized
    loaded : bool
        Flag which indicates whether the mmus of this adapter have been succesfully loaded
    mmu_descriptions : list<MMUDescription>
        The list of mmus of this adpater
    
    _client : IAdapterClient
        The client adapter
    _mmuAccess : MMUAccess
        The MMU Access
    """
    
    def __init__(self, mmuAccess):
        
        """
        Basic initialization of an adapter access
        
        Parameters
        ----------
        mmuAccess : MMUAccess
            The MMU Access
        """
        
        assert (isinstance(mmuAccess, MMIPython.abstraction.mmu.mmu_access.MMUAccess)),"mmuAccess is no MMUAccess"
        
        self.description = None
        self.initialized = False
        self.aborted = False
        self.scene_synchronized = False
        self.loaded = False
        self.mmu_descriptions = list()
        
        self._mmuAccess = mmuAccess
        self._client = None
        
    def start(self):
        """
        Method starts the process
        
        Needs to be implemented by child.
        """
        pass
    
    def create_client(self):
        """
        Creates a client for communication with the adapter
        
        Needs to be implemented by child.
        
        Returns
        -------
        IAdapterClient
            The communication client
        """
        pass
    
    def initialize(self, intermediate_avatar_description : MAvatarDescription, properties, mmuID, sessionID):
        """
        Basic initialization of a MMU
        
        Parameters
        ----------
        intermediate_avatar_description : MAvatarDescription
            The intermediate avatar description
        properties : dict
            The properties for initialization of the MMU
        mmuID : str
            The mmu ID
        sessionID : str
            The session ID
            
        Returns
        -------
        bool
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmuID, str)),"mmuID is no string"
        assert (isinstance(intermediate_avatar_description, MAvatarDescription)),"intermediate_avatar_description is no MAvatarDescription"
        assert (isinstance(properties, dict)),"properties is no dict"
        assert (all(isinstance(x, str) for x in properties.keys())), "Not all keys are of type string in properties"
        assert (all(isinstance(x, str) for x in properties.values())), "Not all values are of type string in properties"
        
        return self._client.get_adapter().Initialize(intermediate_avatar_description, properties, mmuID, sessionID)
        
    def abort(self, mmuID, sessionID):
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
        bool
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmuID, str)),"mmuID is no string"
        
        return self._client.get_adapter().Abort(mmuID, sessionID)
        
    def assign_instruction(self, instruction : MInstruction, simulationState : MSimulationState, mmuID, sessionID):
        """
        Assigns a command to a MMU
        
        Parameters
        ----------
        instruction : MInstruction
            The instruction
        avatar_state : MSimulationState
            The Simulation State, previously MAvatarState
        mmuID : str
            The mmu ID
        sessionID : str
            The session ID
            
        Returns
        -------
        bool
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmuID, str)),"mmuID is no string"
        assert (isinstance(instruction, MInstruction)), "instruction is no MInstruction"
        assert (isinstance(simulationState, MSimulationState)), "avatar_state is no MAvatarState"
        
        return self._client.get_adapter().AssignInstruction(instruction, simulationState, mmuID, sessionID)
        
    def do_step(self, time, simulationState : MSimulationState, mmuID, sessionID):
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
        assert (isinstance(simulationState, MSimulationState)), "avatar_state is no MAvatarState"
        assert (isinstance(time, float)),"time is no float"
        
        return self._client.get_adapter().DoStep(time, simulationState, mmuID, sessionID)
    

    def get_prerequisites(self, instruction : MInstruction, mmuID, sessionID):
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
        
        return self._client.get_adapter().GetPrerequisites(instruction, mmuID, sessionID)
        
            
    def create_mmu_connections(self, mmu_descriptions, sessionID):
        """
        Returns all mmus which are available at the assigned adapter and for the given session Id
        
        Parameters
        ----------
        mmu_descriptions : list<MMUDescription>
            The mmu descriptions for the mmu connections
        sessionID : str
            The session ID
        
        Returns
        --------
        list<MotionModelInterface>
            A list of mmus
        
        """
        assert (isinstance(mmu_descriptions, list)), "mmu_descriptions is no list"
        assert (all(isinstance(x, MMUDescription) for x in mmu_descriptions)), "Not all members are of type MMUDescription in mmu_descriptions"
        assert (isinstance(sessionID, str)),"SessionID is no string"
        
        result = list()
        
        available_mmus = self._client.get_adapter().GetMMus(sessionID)
        
        for id in available_mmus:
            
            # Get all matching descriptions(should be only one)
            description = [x for x in self.mmu_descriptions if x.ID == id]
            
            if len(description) > 0:
                
                # Create a connection
                result.append(MMIPython.abstraction.mmu.motion_model_unit.MotionModelUnit(self._mmuAccess.service_access, 
                                                                                          self._mmuAccess.scene_access, 
                                                                                          self._mmuAccess, 
                                                                                          self, sessionID, 
                                                                                          description[0]))
        
        return result
    
    def synchronize_scene(self, events, sessionID):
        """
        Synchronizes the scene of the adapter
        
        Parameters
        ----------
        events : list<MSceneManipulation>
            A list of scene manipulations
        sessionID : str
            The sessionID
            
        Returns
        -------
        bool
        """
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(events, list)),"events is no list"
        assert (all(isinstance(x, MSceneManipulation) for x in events)), "Not all members are of type MSceneManipulation in events"
        
        self.scene_synchronized = self._client.get_adapter().SynchronizeScene(events, sessionID)
        
        return self.scene_synchronized
    
    def get_scene(self, sessionID):
        """
        Fetches the entire scene from the adapter
        
        Parameters
        ----------
        sessionID : str
            The session ID
        
        Returns
        --------
        list<MSceneObject>
            The scene objects of this scene
        """
        assert (isinstance(sessionID, str)),"SessionID is no string"
        
        return self._client.get_adapter().GetScene(sessionID)
    
    def get_scene_changes(self, sessionID):
        """
        Gets the scene events of the current frame
        
        Parameters
        ----------
        sessionID : str
            The session ID
        
        Returns
        --------
        list<MSceneManipulation>
            The scene changes from the current frame
        """
        assert (isinstance(sessionID, str)),"SessionID is no string"
        
        return self._client.get_adapter().GetSceneChanges(sessionID)
    
    def dispose(self):
        """
        Dispose method which closes the used application
            
        Returns
        -------
        bool
        """
        return self._client.dispose()
        
    def dispose_mmu(self, mmuID, sessionID):
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
        bool
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmuID, str)),"mmuID is no string"
        
        return self._client.get_adapter().Dispose(mmuID, sessionID)
    
    def create_session(self, reference_avatar, sessionID):
        """
        Creates a new session with a given session ID
        
        Parameters
        ----------
        reference_avatar : MAvatarPosture
            The avatar posture for this session
        sessionID : str
            The session ID
            
        Returns
        -------
        bool
        """
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(reference_avatar, MAvatarPosture)),"reference_avatar is no MAvatarPosture"
        
        return self._client.get_adapter().CreateSession(sessionID, reference_avatar)
    
    def get_loadable_mmus(self):
        """
        Returns all loadable MMUs identified by the adapater
        
        Returns
        --------
        list<MMUDescription>
            The list of mmus defined by their description
        """
        
        self.mmu_descriptions = self._client.get_adapter().GetLoadableMMUs()
        return self.mmu_descriptions
    
    def load_mmus(self, mmuIDs, sessionID):
        """
        Loads the mmus by ID
        
        Parameters
        ----------
        mmuIDs : list<str>
            The mmu IDs
        sessionID : str
            The sessionID
            
        Returns
        -------
        bool
        """
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(mmuIDs, list)),"mmuIDs is no list"
        assert (all(isinstance(x, str) for x in mmuIDs)), "Not all members are of type string in mmuIDs"
        
        return self._client.get_adapter().LoadMMUs(mmuIDs, sessionID)
    
    def get_status(self):
        """
        Returns the status of the adapter
        
        Returns
        --------
        dict<str,str>
            The status as dict
        """
        return self._client.get_adapter().GetStatus()
    
    def create_checkpoint(self, mmuID, sessionID):
        """
        Creates a new checkpoint for the specific MMU
        
        Parameters
        ----------
        mmuID : str
            The mmu ID
        sessionID : str
            The session ID
        
        Returns
        --------
        bytearray
            The checkpoint
        """
        assert (isinstance(mmuID, str)),"mmuID is no string"
        assert (isinstance(sessionID, str)),"SessionID is no string"
        
        return self._client.get_adapter().CreateCheckpoint(mmuID, sessionID)
    
    def restore_checkpoint(self, mmuID, sessionID, data):
        """
        Restores a checkpoint for a given MMU
        
        Parameters
        ----------
        mmuID : str
            The mmu which should restore the checkpoint
        sessionID : str
            The session ID
        data : bytearray
            The checkpoint data to restore
            
        Returns
        -------
        bool
        """
        assert (isinstance(mmuID, str)),"mmuID is no string"
        assert (isinstance(sessionID, str)),"SessionID is no string"
        assert (isinstance(data, bytearray)),"data is no bytearray"
        
        return self._client.get_adapter().RestoreCheckpoint(mmuID, sessionID, data)

