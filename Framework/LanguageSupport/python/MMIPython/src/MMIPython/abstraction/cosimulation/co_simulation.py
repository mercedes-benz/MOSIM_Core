## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald

# -*- coding: utf-8 -*-
"""

"""

#from MMIPython.core.containers.avatar_state import AvatarState
from MMIPython.core.interfaces.motion_model_interface import MotionModelInterface
from MMIStandard.mmu.ttypes import MSimulationState

class CoSimulationInterface(MotionModelInterface):
    """
    The base co simulation which can be utilized to access lower level mmus.
    
    Attributes
    ----------
    service_access : ServiceAccess
        Access to the services which are by default shipped with the MMI standard
    scene_access : MSceneAccess
        The access to the scene
    name : str
        The name of the MMU
    manage_scene : bool
        Flag specifies whether the co-simulator is responsible for the 
        scene synchronization and is allowed to write directly into the scene
    mmu_access : MMUAccess
        The mmu access of this co simulator
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
        
        super(CoSimulationInterface, self).__init__(service_access, scene_access)
        
        self.name = None
        self.mmu_access = None
        self.manage_scene = False
        
    
    def initialize(self, reference_avatar, properties):
        """
        The initializationof the MMU
        
        Parameters
        ----------
        reference_avatar : MAvatarDescription
            The reference avatar posture
        properties : dict
            Properties for the actual MMU
        """
        if __debug__:
            print("Initialize CoSimulationInterface\n-------------------------------")
        
    def setup(self, sessionID, avatarID, manage_scene):
        """
        Setup the co simulation
        
        Has to be implemented by child class
        
        sessionID : str
            A unique ID for the session
        avatarID : str
            A unique ID for the avatar
        manage_scene : bool
            Flag specifies whether the co-simulator is responsible for the 
            scene synchronization and is allowed to write directly into the scene
        """
        pass
    
    def add_instruction(self, instruction, priority):
        """
        Adds a new instruction to this co simulation
        
        Has to be implemented by child
        
        Parameters
        ---------
        instruction : MInstruction
            The Motion instruction to be added
        priority : int
            The priority of this motion instruction
        """
        pass
    
    def pre_compute_frame(self):
        """
        Method is exceucted before the compute frame. 
        
        Can be implemented by child
        """
        pass
    
    def do_step(self, time, avatar_state : MSimulationState):
        """
        Do step routine updates the computation within the MMU
        
        Can be overridden by the child class
        
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
        assert (isinstance(time, float)), "time is no float"
        assert (isinstance(avatar_state, MSimulationState)), "avatar_state is no AvatarState"
        
        # Update the scene in the underlying adapters
        if self.manage_scene:
            
            # Get the scene manipulations
            events = self.scene_access.get_events()
            
            # Do the update via mmu access
            self.mmu_access.synchronize_scene(events)
        
        # Do some pre processing
        self.pre_compute_frame()
        
        # Do the actual processing
        result = self.compute_frame(time, avatar_state)
        
        # Do some post processing
        self.post_compute_frame(result)
        
        return result
    
    def compute_frame(self, time, avatar_state : MSimulationState):
        """
        Method which does the co-simulation for one frame.
        
        If scene management is activated, the co-simulation should automatically write 
        and synchronize the scene/otherwise it has to be realized by a superior instance
        
        Has to be implemented by child
        
        Parameters
        ---------
        time : float
            The simulation time delta
        avatar_state : MSimulationState
            The current avatar state
            
        Returns
        --------
        SimulationResult
            The result of a co simulation step is a mmu result itself
        """
        pass
    
    def post_compute_frame(self, result):
        """
        Method is exceucted after the compute frame. 
        
        Can be implemented by child
        
        Parameters
        ----------
        result : MSimulationResult
            The result of this MMU step
        """
        pass
        
    def handle_tasks(self, mmu):
        """
        Handles (possible) tasks which can be executed by the mmu instance
        
        Has to be implemented by child
        
        Parameters
        ----------
        mmu : MotionModelInterface
            The instance of the mmu
        """
        
    def handle_events(self, mmu, mmu_result):
        """
        Handles the events of MMUs
        
        Has to be implemented by child
        
        Parameters
        ----------
        mmu : MotionModelInterface
            The instance of the mmu
        mmu_result : SimulationResult
            The result of the mmu step
        """
        pass
    
    def handle_drawing_calls(self, mmu, mmu_result):
        """
        Handles the drawing calls of MMUs 
        
        Can be implemented by child
        
        Parameters
        ----------
        mmu : MotionModelInterface
            The instance of the mmu
        mmu_result : SimulationResult
            The result of the mmu step
        """
        pass
    
    def merge_results(self, results):
        """
        Method which does the actual merging process of the MMU results.
        
        Has to be implemented by child
        
        Parameters
        ----------
        mmu_result : list<SimulationResult>
            The results of all mmu steps
            
        Returns
        ---------
        AvatarPosture
            The resulting posture of the merging process
        list<MSceneManipulation>
            The resulting scene manipulations
        list<MSimulationEvent>
            The resulting events
        """
        pass