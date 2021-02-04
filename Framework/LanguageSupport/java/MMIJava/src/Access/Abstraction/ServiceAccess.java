// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Felix Gaisbauer

package Access.Abstraction;

import ThriftClients.*;
import Utils.LogLevel;
import Utils.Logger;
import de.mosim.mmi.core.MIPAddress;
import de.mosim.mmi.core.MServiceDescription;
import de.mosim.mmi.services.*;
import org.apache.thrift.TException;

import java.util.HashMap;
import java.util.Map;

public class ServiceAccess implements IServiceAccess {

    /*
        Class provides the access to all available services in the MMIFramework
*/
    //	The address of the MMIRegister
    private final MIPAddress registerAddress;
    //	Map which contains all descriptions of the available services
    private final Map<String, MServiceDescription> serviceDescriptions = new HashMap<>();
    //	The id of the session to which it belongs
    private final String sessionID;

    private IKServiceClient ikService;
    private PathPlanningServiceClient pathPlanningService;
    private RetargetingServiceClient retargetingServiceClient;
    private BlendingServiceClient blendingServiceClient;
    private CollisionDetectionServiceClient collisionDetectionService;
    private GraspPoseServiceClient graspPoseService;
    private WalkPointEstimationServiceClient walkPointEstimationService;


    /// <summary>
    /// Creates a new service access with a given root address.
    /// The root address is used to get the information about all available services and accessing them
    /// </summary>
    /// <param name="address"></param>
    public ServiceAccess(MIPAddress registerAddress, String sessionID) {
        this.registerAddress = registerAddress;
        this.sessionID = sessionID;
    }


    private void initialize() {
        Logger.printLog(LogLevel.L_INFO, "Trying to fetch the services from register: " + this.registerAddress.getAddress() + ":" + this.registerAddress.getPort());

        try (MMIRegisterServiceClient client = new MMIRegisterServiceClient(this.registerAddress.getAddress(), this.registerAddress.getPort())) {
            for (MServiceDescription serviceDescription : client.Access.GetRegisteredServices(this.sessionID)) {
                this.serviceDescriptions.putIfAbsent(serviceDescription.getName(), serviceDescription);
                Logger.printLog(LogLevel.L_INFO, "found: " + serviceDescription.getName() + " at " + serviceDescription.Addresses.get(0).getAddress() + ":" + serviceDescription.Addresses.get(0).getPort());
            }
        } catch (TException e) {
            Logger.printLog(LogLevel.L_ERROR, "Problem at fetching available services from the register. Wrong address? Server down?");
            //e.printStackTrace();
        }
    }


    @Override
    public IKServiceClient getIkThriftClient() {
        MServiceDescription serviceDescription = this.getServiceDescription("ikService");
        if (serviceDescription == null)
            throw new RuntimeException("IK-Service not found");

        if (this.ikService == null) {
            this.ikService = new IKServiceClient(serviceDescription.Addresses.get(0).getAddress(), serviceDescription.Addresses.get(0).getPort());
        }
        this.ikService.start();
        return this.ikService;
    }

    @Override
    public PathPlanningServiceClient getPathPlanningThriftClient() {
        MServiceDescription serviceDescription = this.getServiceDescription("pathPlanningService");
        if (serviceDescription == null)
            throw new RuntimeException("pathPlanning-Service not found");

        if (this.pathPlanningService == null) {
            this.pathPlanningService = new PathPlanningServiceClient(serviceDescription.Addresses.get(0).getAddress(), serviceDescription.Addresses.get(0).getPort());
        }
        this.pathPlanningService.start();
        return this.pathPlanningService;
    }

    @Override
    public RetargetingServiceClient getRetargetingThriftClient() {
        MServiceDescription serviceDescription = this.getServiceDescription("retargetingService");
        if (serviceDescription == null)
            throw new RuntimeException("retargeting-Service not found");

        if (this.pathPlanningService == null) {
            this.retargetingServiceClient = new RetargetingServiceClient(serviceDescription.Addresses.get(0).getAddress(), serviceDescription.Addresses.get(0).getPort());
        }
        this.retargetingServiceClient.start();
        return this.retargetingServiceClient;
    }

    //not implemented yet
    @Override
    public BlendingServiceClient getBlendingThriftClient() {
        throw new RuntimeException("Not implemented yet");

        /*MServiceDescription serviceDescription = this.getServiceDescription("blendingService");
        if (serviceDescription == null)
            throw new RuntimeException("blending-Service not found");

        if (this.blendingServiceClient==null)
        {
            this.blendingServiceClient=new BlendingServiceClient(serviceDescription.Addresses.get(0).getAddress(), serviceDescription.Addresses.get(0).getPort());
        }
        this.blendingServiceClient.Start();
        return this.blendingServiceClient;*/
    }

    @Override
    public CollisionDetectionServiceClient getCollisionDetectionThriftClient() {
        MServiceDescription serviceDescription = this.getServiceDescription("collisionDetectionService");
        if (serviceDescription == null)
            throw new RuntimeException("collisionDetection-Service not found");

        if (this.collisionDetectionService == null) {
            this.collisionDetectionService = new CollisionDetectionServiceClient(serviceDescription.Addresses.get(0).getAddress(), serviceDescription.Addresses.get(0).getPort());
        }
        this.collisionDetectionService.start();
        return this.collisionDetectionService;
    }

    @Override
    public GraspPoseServiceClient getGraspPoseThriftClient() {
        MServiceDescription serviceDescription = this.getServiceDescription("graspPoseService");
        if (serviceDescription == null)
            throw new RuntimeException("graspPose-Service not found");

        if (this.graspPoseService == null) {
            this.graspPoseService = new GraspPoseServiceClient(serviceDescription.Addresses.get(0).getAddress(), serviceDescription.Addresses.get(0).getPort());
        }
        this.graspPoseService.start();
        return this.graspPoseService;
    }

    @Override
    public WalkPointEstimationServiceClient getWalkPointEstimationThriftClient() {
        MServiceDescription serviceDescription = this.getServiceDescription("walkPointEstimationService");
        if (serviceDescription == null)
            throw new RuntimeException("graspPose-Service not found");

        if (this.walkPointEstimationService == null) {
            this.walkPointEstimationService = new WalkPointEstimationServiceClient(serviceDescription.Addresses.get(0).getAddress(), serviceDescription.Addresses.get(0).getPort());
        }
        this.walkPointEstimationService.start();
        return this.walkPointEstimationService;    }

    @Override
    public MInverseKinematicsService.Client getIkService() {
        return this.getIkThriftClient().Access;
    }

    @Override
    public MPathPlanningService.Client getPathPlanningService() {
        return this.getPathPlanningThriftClient().Access;
    }

    @Override
    public MRetargetingService.Client getRetargetingService() {
        return this.getRetargetingThriftClient().Access;
    }

    @Override
    public MBlendingService.Client getBlendingService() {
        return this.getBlendingThriftClient().Access;
    }

    @Override
    public MCollisionDetectionService.Client getCollisionDetectionService() {
        return this.getCollisionDetectionThriftClient().Access;
    }

    @Override
    public MGraspPoseService.Client getGraspPoseService() {
        return this.getGraspPoseThriftClient().Access;
    }

    @Override
    public MWalkPointEstimationService.Client getWalkPointEstimationService() {
        return this.getWalkPointEstimationThriftClient().Access;
    }


    private MServiceDescription getServiceDescription(String serviceName) {
        if (!serviceDescriptions.containsKey(serviceName))
            this.initialize();

        if (!serviceDescriptions.containsKey(serviceName))
            return null;

        return serviceDescriptions.get(serviceName);
    }
}
