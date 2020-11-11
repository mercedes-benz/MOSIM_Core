## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald


# -*- coding: utf-8 -*-
"""

"""

##not needed anymore motion_model_interface must be implemented by a MMU


from MMIStandard.mmu.ttypes import MInstruction, MMUDescription, MSimulationState
from MMIStandard.avatar.ttypes import MAvatarDescription
#from MMIStandard.ttypes import MMUDescription, MAvatarDescription, MInstruction

from MMIPython.core.interfaces.motion_model_interface import MotionModelInterface
#from MMIPython.core.containers.avatar_state import AvatarState
#from MMIPython.core.utils import conversion_utils as conv
import MMIPython.abstraction.mmu.mmu_access
import MMIPython.abstraction.access.interface.adapter_access

import MMIPython.abstraction.mmu.utils

class MotionModelUnit(MotionModelInterface):
    """
    The MotionModelUnit which is used as a representation of the actual motion model implementations.
    
    Attributes
    ----------
    service_access : ServiceAccess
        Access to the services which are by default shipped with the MMI standard
    scene_access : ISceneAccess
        The access to the scene
    _mmu_access : MMUAccess
        The assigned mmu access
    _sessionID : str
        The assigned session ID
    _description
        The description of the MMU
    __adapter_access : IAdapter
        The assigned adapter access
    """
    
    def __init__(self, service_access, scene_access, mmu_access, adapter_access, sessionID, description):
        """
        Constructor which takes a service and scene access as input parameters
        
        Parameters
        -----------
        service_access : ServiceAccess
            Access to the services which are by default shipped with the MMI standard
        scene_access : SceneAccess
            The access to the scene
        mmu_access : MMUAccess
            Access to the mmus
        adapter_access : IAdapterAccess
            The access to the adapter
        sessionID : str
            The session ID
        description : MMUDescription
            The mmu description
        """
        assert (isinstance(mmu_access, MMIPython.abstraction.mmu.mmu_access.MMUAccess)), "mmu_access is no MMUAccess."
        assert (isinstance(adapter_access, MMIPython.abstraction.access.interface.adapter_access.IAdapterAccess)), "adapter_access is no IAdapter."
        assert (isinstance(sessionID, str)), "sessionID is no string."
        assert (isinstance(description, MMUDescription)), "description is no MMUDescription."
        
        super(MotionModelUnit, self).__init__(service_access, scene_access)
        
        self.name = description.Name
        self.id = description.ID
        self._mmu_access = mmu_access
        self._sessionID = sessionID
        self._description = description
        self.__adapter_access = adapter_access
        
    def get_adapter(self):
        """
        Returns the assigned adapter access.
        
        Returns
        -------
        IAdapterAccess
        """
        return self.__adapter_access
        
    def change_adapter(self, adapter_access):
        """
        Changes the adapter of this MMU.
        
        Parameters
        ----------
        adapter_access : IAdapterAccess
            The access to the adapter
        """
        if self.__adapter_access is not None:
            self.__adapter_access.dispose()
        self.__adapter_access = adapter_access
        
    
    def initialize(self, avatar_description : MAvatarDescription, properties):
        """
        The initialization of the MMU
        
        Parameters
        ----------
        avatar_description : MAvatarDescription
            The intermediate avatar description
        properties : dict
            Properties for the actual MMU
            
        Returns
        -------
        bool
        """
        
        assert (isinstance(avatar_description, MAvatarDescription)), "avatar_description is no MAvatarDescription."
        assert (isinstance(properties, dict)), "properties is no dict."
        assert (all(isinstance(x, str) for x in properties.keys())), "Not all keys are of type string in properties"
        assert (all(isinstance(x, str) for x in properties.values())), "Not all values are of type string in properties"
        
        
        # Initialize the mmu at the adapter
        return self.__adapter_access.initialize(self._mmu_access.intermediate_avatar_description, properties, self.id, self._sessionID)
                
    
    def get_prerequisites(self, motion_instruction : MInstruction):
        """
        Returns the prerequisites to start the motion instruction
        
        Parameters
        ---------
        motion_instruction : MotionInstruction
            The motion to specify the prerequisites
            
        Returns
        --------
        list<MConstraint>
            The prerequisites
        """
        assert (isinstance(motion_instruction, MInstruction)), "motion_instruction is no MInstruction."
        
        if __debug__:
            print("Name : {0}".format(self.name))
            print("ID : {0}".format(self._sessionID))
        
        return self.__adapter_access.get_prerequisites(motion_instruction, self.id, self._sessionID)
    
    def assign_instruction(self, motion_instruction : MInstruction, avatar_state : MSimulationState):
        """
        Method sets the motion instruction
        
        Parameters
        ---------
        motion_instruction : MInstruction
            The motion to assign
        avatar_state : MSimulationState
            The current avatar state 
            
        Returns
        -------
        bool
        """
        
        assert (isinstance(motion_instruction, MInstruction)), "motion_instruction is no MInstruction."
        assert (isinstance(avatar_state, MSimulationState)), "avatar_state is no AvatarState."
        
        # Transfer operation to Adapter
        return self.__adapter_access.assign_instruction(motion_instruction, avatar_state, self.id, self._sessionID)
    
    def do_step(self, time, avatar_state : MSimulationState):
        """
        Do step routine updates the computation within the MMU
        
        Parameters
        ---------
        time : float
            The delta time
        avatar_state : MSimulationState
            The current avatar state
            
        Returns
        ------
        MSimulationResult
            The computed result of the do step routine
        """
        
        assert (isinstance(time, float)),"time is no float"
        assert (isinstance(avatar_state, MSimulationState)), "avatar_state is no AvatarState."
        
        # #If automatic retargeting enabled perform it on the state
        # if self._mmu_access.retargeting is not None:
        #     avatar_state.Initial = self._mmu_access.retargeting.retarget_to_intermediate(avatar_state.Initial)
        #     avatar_state.Current = self._mmu_access.retargeting.retarget_to_intermediate(avatar_state.Current)
            
        hierarchy = self._mmu_access.intermediate_avatar_description.Posture
        
        # Do step in adapter
        result = self.__adapter_access.do_step(time, avatar_state, self.id, self._sessionID)
        
        # # Retarget back
        # if self._mmu_access.retargeting is not None:
        #     result.Posture = self._mmu_access.retargeting.retarget_to_specific(result.Posture)
            
        return result
    
    def abort(self):
        """
        Method forces the termination of a MMU
            
        Returns
        -------
        bool
        """
        return self.__adapter_access.abort(self.id, self._sessionID)

    def dispose(self):
        """
        A method for disposing the internally used resources
            
        Returns
        -------
        bool
        """
        return self.__adapter_access.dispose_mmu(self.id, self._sessionID)
        
    def create_checkpoint(self):
        """
        Creates a new checkpoint for the specific MMU
        
        Returns
        --------
        bytearray
            The checkpoint
        """
        
        return self.__adapter_access.create_checkpoint(self.id, self._sessionID)
    
    def restore_checkpoint(self, data):
        """
        Restores a checkpoint for a given MMU
        
        Parameters
        ----------
        data : bytearray
            The checkpoint data to restore
            
        Returns
        -------
        bool
        """
        assert (isinstance(data, bytes)),"data is no bytes"
        
        return self.__adapter_access.restore_checkpoint(self.id, self._sessionID, data)
    