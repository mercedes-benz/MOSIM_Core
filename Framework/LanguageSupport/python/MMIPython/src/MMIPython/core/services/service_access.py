## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald, Janis Sprenger

# -*- coding: utf-8 -*-
"""

"""

from MMIStandard.scene.ttypes import MSceneObject, MCollider
from MMIStandard.avatar.ttypes import MAvatarPostureValues, MAvatarPosture
from MMIStandard.core.ttypes import MIPAddress
from MMIStandard.services.ttypes import MIKProperty
from MMIStandard.math.ttypes import MTransform

from MMIStandard.register import MMIRegisterService
from MMIStandard.services import MGraspPoseService, MPathPlanningService, MRetargetingService, MCollisionDetectionService, MInverseKinematicsService

#from MMIStandard.ttypes import MAvatarPosture, MAvatarPostureValues, MVector, MCollider, MTransform, IKProperty, MBoneType, MSceneObject, MIPAddress
#from MMIStandard import IKService, CollisionDetectionService, PathPlanningService, RetargetingService, GraspPoseService, MMURegisterService

from MMIPython.core.utils.thrift_client import ThriftClient

class ServiceAccess(object):
    """
    An access point to the standard MMI Services
    
    Attributes
    ----------
    
    _address : str
        The root address which is used to get the information 
        about all available services and accessing them
        
    service_descriptions : dict
        A dictionary with all descriptions for the different services
    """
    
    def __init__(self, register_ip_address, sessionID):
        """
        The default constructor which requires the root address.
        
        Parameters
        ----------
        register_ip_address : MIPAddress
            The root register ip address to access the services
        """
        
        assert(isinstance(register_ip_address, MIPAddress)), 'The register_address is no MIPAddress'
        
        self._register_ip_address = register_ip_address
        self.service_descriptions = dict()
        self.__RetargetingService = None
        self.__PathPlanningService = None
        self.__BlendingService = None

        self.loadedServices = {}
        self.__sessionID = sessionID
        self.initialize()
        
    def _get_service_description(self, service_name):
        """
        Returns the description of a given service name
        
        Parameters
        -----------
        service_name : str
            The service name
            
        Returns
        ---------
        ServiceDescription
            The description of the service
        """
        
        assert(isinstance(service_name, str)), "service_name is no string"
        
        description = None
        
        if service_name not in self.service_descriptions:
            self.initialize()
            
        if service_name in self.service_descriptions:
            description = self.service_descriptions[service_name]
            print("service found: ", description)
            
        return description
    
        
    def initialize(self):
        """
        Initializes the service access.
        """
        
        descriptions = None
        
        # Get the service descriptions from the mmu register
        with ThriftClient(self._register_ip_address.Address, self._register_ip_address.Port, MMIRegisterService.Client) as client:
            descriptions = client._access.GetRegisteredServices(self.__sessionID)
        
        if descriptions is None or len(descriptions) == 0:
            print('No service descriptions received.')
        
        # Store descriptions into dictionary
        for description in descriptions:
            self.service_descriptions[description.Name] = description

    def GetRetargetingService(self):
        if self.__RetargetingService is None:
            description = self._get_service_description("Retargeting")
            self.__RetargetingService = ThriftClient(description.Addresses[0].Address, description.Addresses[0].Port, MRetargetingService.Client)
            self.__RetargetingService.__enter__()
        return self.__RetargetingService._access

    def GetPathPlanningService(self):
        if self.__PathPlanningService is None:
            description = self._get_service_description("pathPlanningService")
            self.__PathPlanningService = ThriftClient(description.Addresses[0].Address, description.Addresses[0].Port, MPathPlanningService.Client)
            self.__PathPlanningService.__enter__()
        return self.__PathPlanningService._access

    def GetService(self, name, ServiceType):
        if not name in self.loadedServices:
            description = self._get_service_description(name)
            self.loadedServices[name] = ThriftClient(description.Addresses[0].Address, description.Addresses[0].Port, ServiceType.Client)
            self.loadedServices[name].__enter__()
        return self.loadedServices[name]._access



    # Todo: enable access functions for all services. 


    # The following functions are in violation of the thrift concept. 
    #
    # def resolve_collision(current_posture):
    #     """
    #     ???
        
    #     Parameters
    #     ----------
    #     current_posture : MAvatarPosture
    #         The base posture
            
    #     Returns
    #     --------
    #     MAvatarPosture
    #     """
    #     assert (isinstance(current_posture, MAvatarPosture)), "current_posture is no MAvatarPosture"
    #     raise NotImplementedError

    # def ground_character(current_posture):
    #     """
    #     ???
        
    #     Parameters
    #     ----------
    #     current_posture : MAvatarPosture
    #         The base posture

    #     Returns
    #     --------
    #     MAvatarPosture
    #     """
    #     assert (isinstance(current_posture, MAvatarPosture)), "current_posture is no MAvatarPosture"
    #     raise NotImplementedError


    # def blend (from_posture,to_posture,percentage):
    #     """
    #     ???
        
    #     Parameters
    #     ----------
    #     from_posture : MAvatarPosture
    #         The start posture
    #     to_posture : MAvatarPosture
    #         The end possture
    #     percentage:  float

    #     Returns
    #     --------
    #     MAvatarPosture
    #     """
    #     assert (isinstance(from_posture, MAvatarPosture)), "current_posture is no MAvatarPosture"
    #     assert (isinstance(to_posture, MAvatarPosture)), "current_posture is no MAvatarPosture"
    #     assert (isinstance(percentage, float)), "current_posture is no MAvatarPosture"
    #     raise NotImplementedError


        


    # def compute_ik(self, current_posture, properties):
    #     """
    #     Computes the IK posture with from the ik properties and base posture.
        
    #     Parameters
    #     ----------
    #     current_posture : MAvatarPosture
    #         The base posture
    #     properties : list<IKProperty> or dict<string,string>
    #         The properties for the IK
            
    #     Returns
    #     --------
    #     MAvatarPosture
    #         The computed IK posture
    #     """
        
    #     assert (isinstance(current_posture, MAvatarPosture)), "current_posture is no MAvatarPosture"
    #     assert (isinstance(properties, dict) or isinstance(properties, list)), "properties is no dict or list"
    #     if isinstance(properties, list):
    #         assert(all(isinstance(x, IKProperty) for x in properties)), "Not all members are of type IKProperty in properties"
    #     else:
    #         assert(all(isinstance(x, str) for x in properties.keys())), "Not all keys are of type str in properties"
    #         assert(all(isinstance(x, str) for x in properties.values())), "Not all values are of type str in properties"
        
    #     # Get the description of the service
    #     description = self._get_service_description('ikService')
        
    #     if description is not None:
            
    #         # Create a service client with the service type
    #         with ThriftClient(description.Address.Address, description.Address.Port, IKService.Client) as client:
    #             return client._access.ComputeIK(current_posture, properties)
        
    # This function violates the concept of thrift. 
    # 
    # def compute_path(self, start, goal, scene_objects, parameters):
    #     """
    #     Computes a path from start to goal under consideration of given scene objects.
        
    #     Parameters
    #     ----------
    #     start : MVector
    #         The start point
    #     goal : MVector
    #         The goal point
    #     scene_objects : list<MSceneObject>
    #         The list of scene objects to consider
    #     parameters : dict<string,string>
    #         A dictionary with optional parameters
            
    #     Returns
    #     --------
    #     list<MVector>
    #         The computed path
    #     """
    #     assert (isinstance(start, MVector)), "start is no MVector"
    #     assert (isinstance(goal, MVector)), "goal is no MVector"
    #     assert (isinstance(scene_objects, list)), "scene_objects is no list"
    #     assert (all(isinstance(x, MSceneObject) for x in scene_objects)), "Not all members are of type MSceneObject in scene_objects"
    #     assert (isinstance(parameters, dict)), "properties is no dict"
    #     assert (all(isinstance(x, str) for x in parameters.keys())), "Not all keys are of type str in parameters"
    #     assert (all(isinstance(x, str) for x in parameters.values())), "Not all values are of type str in parameters"
        
    #     # Get the description of the service
    #     description = self._get_service_description('pathPlanningService')
        
    #     if description is not None:
            
    #         # Create a service client with the service type
    #         with ThriftClient(description.Address.Address, description.Address.Port, PathPlanningService.Client) as client:
    #             return client._access.ComputePath(start, goal, scene_objects, parameters)
    
    # def compute_penetration(self, colliderA, transformA, colliderB, transformB):
    #     """
    #     Computes the penetration of two colliders.
        
    #     Parameters
    #     ----------
    #     colliderA : MCollider
    #         The first collider
    #     transformA : MTransform
    #         The transform of the first collider
    #     colliderB : MCollider
    #         The second collider
    #     tranformB : MTransform
    #         The transform of the second collider
            
    #     Returns
    #     --------
    #     MVector
    #         The penetration vector
    #     """
    #     assert (isinstance(colliderA, MCollider)), "colliderA is no MCollider"
    #     assert (isinstance(transformA, MTransform)), "transformA is no MTransform"
    #     assert (isinstance(colliderB, MCollider)), "colliderB is no MCollider"
    #     assert (isinstance(transformB, MTransform)), "tranformB is no MTransform"
        
    #     # Get the description of the service
    #     description = self._get_service_description('collisionDetectionService')
        
    #     if description is not None:
            
    #         # Create a service client with the service type
    #         with ThriftClient(description.Address.Address, description.Address.Port, CollisionDetectionService.Client) as client:
    #             return client._access.ComputePenetration(colliderA, transformA, colliderB, transformB)
    
    # def causes_collision(self, colliderA, transformA, colliderB, transformB):
    #     """
    #     Detects if two colliders causes a collision
        
    #     Parameters
    #     ----------
    #     colliderA : MCollider
    #         The first collider
    #     transformA : MTransform
    #         The transform of the first collider
    #     colliderB : MCollider
    #         The second collider
    #     tranformB : MTransform
    #         The transform of the second collider
            
    #     Returns
    #     --------
    #     bool
        
    #     """
    #     assert (isinstance(colliderA, MCollider)), "colliderA is no MCollider"
    #     assert (isinstance(transformA, MTransform)), "transformA is no MTransform"
    #     assert (isinstance(colliderB, MCollider)), "colliderB is no MCollider"
    #     assert (isinstance(transformB, MTransform)), "tranformB is no MTransform"
        
    #     # Get the description of the service
    #     description = self._get_service_description('collisionDetectionService')
        
    #     if description is not None:
            
    #         # Create a service client with the service type
    #         with ThriftClient(description.Address.Address, description.Address.Port, CollisionDetectionService.Client) as client:
    #             return client._access.CausesCollision(colliderA, transformA, colliderB, transformB)
            
    # def compute_grasp_pose(self, current_posture, bone_type, grasp_target, position_hand):
    #     """
    #     Computes the grasp pose of a hand.
        
    #     Parameters
    #     ----------
    #     current_posture : MAvatarPosture
    #         The current posture
    #     bone_type : MBoneType
    #         The bone type of the hand (which hand should be used)
    #     grasp_target : MSceneObject
    #         The grasp target scene object
    #     position_hand : bool
    #         Flag if the hand should be positioned
            
    #     Returns
    #     --------
    #     MVector
    #         The penetration vector
    #     """
    #     assert (isinstance(current_posture, MAvatarPosture)), "current_posture is no MAvatarPosture"
    #     assert (isinstance(bone_type, MBoneType)), "bone_type is no MBoneType"
    #     assert (isinstance(grasp_target, MSceneObject)), "grasp_target is no MSceneObject"
    #     assert (isinstance(position_hand, bool)), "position_hand is no bool"
        
    #     # Get the description of the service
    #     description = self._get_service_description('graspPoseService')
        
    #     if description is not None:
            
    #         # Create a service client with the service type
    #         with ThriftClient(description.Address.Address, description.Address.Port, GraspPoseService.Client) as client:
    #             return client._access.ComputeGraspPose(current_posture, bone_type, grasp_target, position_hand)