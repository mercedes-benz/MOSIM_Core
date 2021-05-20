from MMIPython.core.interfaces.motion_model_interface import MotionModelInterface
from MMIStandard.core.ttypes import MBoolResponse, MIPAddress
from MMIStandard.mmu.ttypes import MInstruction, MSimulationState, MSimulationResult, MSimulationEvent
MSimulationEvent_End = "end"
from MMIStandard.math.ttypes import MQuaternion, MVector3

from .helpers import * 

from anim_utils.animation_data import BVHReader, MotionVector, SkeletonBuilder   
import numpy as np


JOINT_MAP = {
    "Hips":0, "Chest":1, "Chest2":2, "Chest3":3, "Chest4":4, "Neck":5, "Head":6,
    "RightCollar":7, "RightShoulder":8, "RightElbow":9, "RightWrist":10,
    "LeftCollar":11, "LeftShoulder":12, "LeftElbow":13, "LeftWrist":14,
    "RightHip":15, "RightKnee":16, "RightAnkle": 17, "RightToe": 18,
    "LeftHip":19, "LeftKnee":20, "LeftAnkle":21, "LeftToe":22
}


class BVH_Streamer(MotionModelInterface):
    def __init__(self, service_access, scene_access):
        super().__init__(service_access, scene_access)

        # connect to retargeting service
        self.retargetingService = self.service_access.GetRetargetingService()
        self.basePosture = LoadMAvatarPosture("data/yawning_configuration.mos")

        # setup retargeting
        self.retargetingService.SetupRetargeting(self.basePosture)

        # load bvh
        bvh = BVHReader("data/yawning.bvh")   
        mv = MotionVector()  
        mv.from_bvh_reader(bvh)  
        skeleton = SkeletonBuilder().load_from_bvh(bvh)  
        joint_positions = []  
        joint_rotations = []
        for frame in mv.frames:  
            positions = []  
            rotations = []

            for j in skeleton.animated_joints:  
                p = skeleton.nodes[j].get_global_position(frame)  
                q = skeleton.nodes[j].get_global_orientation_quaternion(frame)
                positions.append(p)  
                rotations.append(q)
            joint_positions.append(positions)
            joint_rotations.append(rotations)  

        self.motion_data = (joint_positions, joint_rotations)

        # framerate
        self.frameTime = 1 / 30.0
        # frame counter
        self.frameCounter = 1



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
        return MBoolResponse(True)

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
        self.instruction = motionInstruction
        # there is no instruction to be assigned for this MMU. Read out your instructions here. 
        return MBoolResponse(True)


    def DoStep(self, dtime, simulationState : MSimulationState):
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
        # create result buffer and copy input variables. 
        simres = MSimulationResult()
        simres.Posture = simulationState.Current
        simres.Events = simulationState.Events
        simres.SceneManipulations = simulationState.SceneManipulations
        simres.Constraints = simulationState.Constraints


        # get Posture
        globalPosture = MAvatarPosture(self.basePosture.AvatarID, [])
        for j in self.basePosture.Joints:
            pos = self.motion_data[0][self.frameCounter][JOINT_MAP[j.ID]] / 100.0
            vPos = MVector3(-pos[0], pos[1], pos[2])
            rot = self.motion_data[1][self.frameCounter][JOINT_MAP[j.ID]]
            qRot = MQuaternion(-rot[1], rot[2], rot[3], -rot[0])
            jc = MJoint(j.ID, j.Type, vPos, qRot, j.Channels, j.Parent)

            globalPosture.Joints.append(jc)

        # retarget to intermediate
        intermediatePosture = self.retargetingService.RetargetToIntermediate(globalPosture)
        intermediatePosture.PostureData[0] = simres.Posture.PostureData[0]
        intermediatePosture.PostureData[2] = simres.Posture.PostureData[2]
        intermediatePosture.AvatarID = simres.Posture.AvatarID

        simres.Posture = intermediatePosture

        # check end event
        if self.frameCounter >= len(self.motion_data[0])-1:
            simres.Events.append(MSimulationEvent(self.instruction.Name, MSimulationEvent_End, self.instruction.ID))
        else:
            self.frameCounter+=1
        return simres

    def GetBoundaryConstraints(self, instruction):
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
        return []

    def CheckPrerequisites(self, instruction):
        """
        Parameters
        ---------
         - instruction: MInstruction
            The motion to assign

        Returns
        ------
        MBoolResponse
        """
        return MBoolResponse(True)


    def Abort(self, instructionId):
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
        return MBoolResponse(True)


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
        return MBoolResponse(True)


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
        return []


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
        return MBoolResponse(True)


    def ExecuteFunction(self, name, parameters):
        """
        //Method for executing an arbitrary function (optionally)
        Parameters:
         - name : str
            name of the isntruction
         - parameters : dictionary<str,str> 
            optional parameters

        """
        return