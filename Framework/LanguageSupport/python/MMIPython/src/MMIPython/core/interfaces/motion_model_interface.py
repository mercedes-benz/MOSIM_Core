## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald

# -*- coding: utf-8 -*-
"""

"""
from MMIStandard.mmu import MotionModelUnit
from MMIPython.core.services.service_access import ServiceAccess
#from MMIPython.core.interfaces.scene_access import ISceneAccess
from MMIStandard.services import MSceneAccess
from MMIStandard.mmu.ttypes import MInstruction, MSimulationState
from abc import ABC, abstractmethod

class MotionModelInterface(MotionModelUnit.Iface,ABC):
    """
    The MotionModelInterface which is the base class for all python MMUs.
    
    Attributes
    ----------
    service_access : ServiceAccess
        Access to the services which are by default shipped with the MMI standard
    scene_access : MSceneAccess
        The access to the scene
    name : str
        The name of the MMU
    """
    
    def __init__(self, service_access, scene_access):
        """
        Constructor which takes a service and scene access as input parameters
        
        Parameters
        -----------
        service_access : ServiceAccess
            Access to the services which are by default shipped with the MMI standard
        scene_access : SceneAccess
            The access to the scene
        """
        assert (isinstance(service_access, ServiceAccess)), "service_access is no ServiceAccess."
        assert (isinstance(scene_access, MSceneAccess.Iface)), "scene_access is no SceneAccess."
        
        self.service_access = service_access
        self.scene_access = scene_access
        self.name = None
        self.id= None
        
    @abstractmethod
    def Initialize(self, avatarDescription, properties):
        """
        The initializationof the MMU
        
        Parameters
        ----------
        avatar_description : MAvatarDescription
            The intermediate avatar description
        properties : dict
            Properties for the actual MMU
            
        Returns
        --------
        MBoolResponse

        """
        pass

    @abstractmethod
    def AssignInstruction(self, motionInstruction : MInstruction, simulationState : MSimulationState):
        """
        Method sets the motion instruction
        
        Parameters
        ---------
        motion_instruction : MInstruction
            The motion to assign
        simulationState : MSimulationState
            The current simulation state
            
        Returns
        --------
        bool
        """
        pass

    @abstractmethod
    def DoStep(self, time, simulationState : MSimulationState):
        """
        Do step routine updates the computation within the MMU
        
        Parameters
        ---------
        time : double
            The delta time
        simulationState : MSimulationState
            The current simulation state
            
        Returns
        ------
        SimulationResult
            The computed result of the do step routine

        """
        pass
    @abstractmethod
    def GetBoundaryConstraints(self, instruction : MInstruction):
        """
        Returns constraints which are relevant for the transition

        Parameters
        ---------
        instruction : MInstruction
          The motion to assign
            
        Returns
        ------
        list<MConstraint>
        
        """
        pass

    @abstractmethod
    def CheckPrerequisites(self, instruction : MInstruction):
        """
        Parameters
        ---------
         - instruction: MInstruction
            The motion to assign

        Returns
        ------
        MBoolResponse
        """
        pass

    @abstractmethod
    def Abort(self, instructionId : MInstruction):
        """
        Method forces the termination of a MMU

        Parameters
        ---------
        instructionID: MInstruction
          The id of the instruction
        
        Returns
        --------
        MBoolResponse

        """
        pass

    @abstractmethod
    def Dispose(self, parameters):
        """
        A method for disposing the internally used resources
        
         Parameters
        ---------
        parameters: dictionary<str,stra>
          optional parameters
        
          Returns
        --------
        MBoolResponse

        """
        pass

    @abstractmethod
    def CreateCheckpoint(self):
        """
        Creates a Checkpoint
        
         Parameters
        ---------
        parameters: dictionary<str,stra>
          optional parameters
        
            Returns
        -------
        bytearray
            The checkpoint data
        """
        pass

    @abstractmethod
    def RestoreCheckpoint(self, data):
        """
        Restores the checkpoint from the data
        
        Parameters
        ----------
        data : object
            The checkpoint data
            
        Returns
        -------
        MBoolResponse
        """
        pass

    @abstractmethod
    def ExecuteFunction(self, name, parameters):
        """
        //Method for executing an arbitrary function (optionally)
        Parameters:
         - name : str
            name of the isntruction
         - parameters : dictionary<str,str> 
            optional parameters

        """
        pass
  