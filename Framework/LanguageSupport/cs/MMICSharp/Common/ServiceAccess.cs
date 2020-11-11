// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Clients;
using MMIStandard;
using System;
using System.Collections.Generic;

namespace MMICSharp.Common
{
    /// <summary>
    /// Class to access all available services within the MMI framework
    /// </summary>
    [Serializable]
    public class ServiceAccess : IServiceAccess, IDisposable
    {

        internal bool AutoStart = true;

        #region services

        public MGraspPoseService.Iface GraspService
        {
            get;
            private set;
        }

        public GraspPoseServiceClient GraspServiceClient
        {
            get;
            private set;
        }

        /// <summary>
        /// Return the access to the ik service
        /// </summary>
        public MInverseKinematicsService.Iface IKService
        {
            get
            {
                if (this.IKServiceClient != null)
                    return this.IKServiceClient.Access;
                else
                {
                    MServiceDescription serviceDescription = this.GetServiceDescription("ikService");

                    //Create the client and start
                    this.IKServiceClient = new IKServiceClient(serviceDescription.Addresses[0].Address, serviceDescription.Addresses[0].Port, AutoStart);
                    //Always start (open the connection) if requested
                    this.IKServiceClient.Start();

                    return this.IKServiceClient.Access;
                }
            }
        }

        /// <summary>
        /// Returns the IKService client
        /// </summary>
        public IKServiceClient IKServiceClient
        {
            get;
            private set;
        }


        
        public MPathPlanningService.Iface PathPlanningService
        {
            get
            {
                if (this.PathPlanningServiceClient != null)
                    return this.PathPlanningServiceClient.Access;
                else
                {
                    MServiceDescription serviceDescription = this.GetServiceDescription("pathPlanningService");

                    //Create the client and start
                    this.PathPlanningServiceClient = new PathPlanningServiceClient(serviceDescription.Addresses[0].Address, serviceDescription.Addresses[0].Port, AutoStart);
                    //Always start (open the connection) if requested
                    this.PathPlanningServiceClient.Start();

                    return this.PathPlanningServiceClient.Access;
                }
            }
        }

        /// <summary>
        /// Returns the PathPlanning client
        /// </summary>
        public PathPlanningServiceClient PathPlanningServiceClient
        {
            get;
            private set;
        }

        public MRetargetingService.Iface RetargetingService
        {
            get
            {
                if (this.RetargetingServiceClient != null)
                    return this.RetargetingServiceClient.Access;
                else
                {
                    MServiceDescription serviceDescription = this.GetServiceDescription("retargetingService");

                    //Create the client
                    this.RetargetingServiceClient = new RetargetingServiceClient(serviceDescription.Addresses[0].Address, serviceDescription.Addresses[0].Port, AutoStart);
                    //Always start (open the connection) if requested
                    this.RetargetingServiceClient.Start();


                    return this.RetargetingServiceClient.Access;
                }
            }
        }

        /// <summary>
        /// Returns the Retargeting client
        /// </summary>
        public RetargetingServiceClient RetargetingServiceClient
        {
            get;
            private set;
        }

        public MMIRegisterService.Iface RegisterService
        {
            get
            {
                if (this.RegisterServiceClient != null)
                    return this.RegisterServiceClient.Access;
                else
                {
                    //Create the client
                    this.RegisterServiceClient = new MMIRegisterServiceClient(this.registerAddress.Address, this.registerAddress.Port, AutoStart);
                    //Always start (open the connection) if requested
                    this.RegisterServiceClient.Start();

                    return this.RegisterServiceClient.Access;
                }
            }
        }


        /// <summary>
        /// Returns the Register Service client
        /// </summary>
        public MMIRegisterServiceClient RegisterServiceClient
        {
            get;
            private set;
        }

        public MCollisionDetectionService.Iface CollisionDetectionService
        {
            get
            {
                if (this.CollisionDetectionServiceClient != null)
                    return this.CollisionDetectionServiceClient.Access;
                else
                {
                    MServiceDescription serviceDescription = this.GetServiceDescription("collisionDetectionService");

                    //Create the client
                    this.CollisionDetectionServiceClient = new CollisionDetectionServiceClient(serviceDescription.Addresses[0].Address, serviceDescription.Addresses[0].Port, AutoStart);
                    //Always start (open the connection) if requested
                    this.CollisionDetectionServiceClient.Start();


                    return this.CollisionDetectionServiceClient.Access;
                }
            }
        }


        /// <summary>
        /// Returns the Register Service client
        /// </summary>
        public CollisionDetectionServiceClient CollisionDetectionServiceClient
        {
            get;
            private set;
        }

        public MGraspPoseService.Iface GraspPoseService
        {
            get
            {
                if (this.GraspPoseServiceClient != null)
                    return this.GraspPoseServiceClient.Access;
                else
                {
                    MServiceDescription serviceDescription = this.GetServiceDescription("graspPoseService");

                    //Create the client
                    this.GraspPoseServiceClient = new GraspPoseServiceClient(serviceDescription.Addresses[0].Address, serviceDescription.Addresses[0].Port, AutoStart);
                    //Always start (open the connection) if requested
                    this.GraspPoseServiceClient.Start();

                    return this.GraspPoseServiceClient.Access;
                }
            }
        }



        /// <summary>
        /// Returns the Register Service client
        /// </summary>
        public GraspPoseServiceClient GraspPoseServiceClient
        {
            get;
            private set;
        }


        public WalkPointEstimationServiceClient WalkPointEstimationServiceClient
        {
            get;
            private set;
        }

        public MWalkPointEstimationService.Iface WalkPointEstimationService
        {
            get
            {
                if (this.GraspPoseServiceClient != null)
                    return this.WalkPointEstimationServiceClient.Access;
                else
                {
                    MServiceDescription serviceDescription = this.GetServiceDescription("walkPointEstimationService");

                    //Create the client
                    this.WalkPointEstimationServiceClient = new WalkPointEstimationServiceClient(serviceDescription.Addresses[0].Address, serviceDescription.Addresses[0].Port, AutoStart);
                    //Always start (open the connection) if requested
                    this.WalkPointEstimationServiceClient.Start();

                    return this.WalkPointEstimationServiceClient.Access;
                }
            }
        }
        public PostureBlendingServiceClient PostureBlendingServiceClient
        {
            get;
            private set;
        }


        public MPostureBlendingService.Iface PostureBlendingService
        {
            get
            {
                if (this.GraspPoseServiceClient != null)
                    return this.PostureBlendingServiceClient.Access;
                else
                {
                    MServiceDescription serviceDescription = this.GetServiceDescription("postureBlendingService");

                    //Create the client
                    this.PostureBlendingServiceClient = new PostureBlendingServiceClient(serviceDescription.Addresses[0].Address, serviceDescription.Addresses[0].Port, AutoStart);
                    //Always start (open the connection) if requested
                    this.PostureBlendingServiceClient.Start();

                    return this.PostureBlendingServiceClient.Access;
                }
            }
        }






        #endregion


        #region private variables

        private readonly MIPAddress registerAddress;

        private Dictionary<string, MServiceDescription> serviceDescriptions = new Dictionary<string, MServiceDescription>();
        private List<IDisposable> clients = new List<IDisposable>();


        private string sessionID;

        #endregion

        /// <summary>
        /// Creates a new service access with a given root address.
        /// The root address is used to get the information about all available services and accessing them
        /// </summary>
        /// <param name="address"></param>
        public ServiceAccess(MIPAddress registerAddress, string sessionID)
        {
            this.registerAddress = registerAddress;
            this.sessionID = sessionID;
        }


        public void Initialize()
        {

            Console.WriteLine("Trying to fetch the services from: " + this.registerAddress.Address + " " + this.registerAddress.Port);

            try
            {
                //Create a new client to connect to the specific register server
                using (MMIRegisterServiceClient client = new MMIRegisterServiceClient(this.registerAddress.Address, this.registerAddress.Port))
                {
                    foreach (MServiceDescription serviceDescription in client.Access.GetRegisteredServices(this.sessionID))
                    {
                        if (!this.serviceDescriptions.ContainsKey(serviceDescription.Name))
                        {
                            this.serviceDescriptions.Add(serviceDescription.Name, serviceDescription);
                        }

                        Console.WriteLine(serviceDescription.Name + " " + serviceDescription.Addresses[0].Address + " " + serviceDescription.Addresses[0].Port);
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Problem at fetching available services from the register. Wrong address? Server down?");
            }            
        }




        /// <summary>
        /// Use the service access base to call an arbitrary service
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Dictionary<string, string> Consume(MIPAddress address, Dictionary<string, string> parameters)
        {
            //Create a base service client
            using (MMIServiceBaseClient client = new MMIServiceBaseClient(address.Address, address.Port))
            {
                return client.Access.Consume(parameters);
            }
        }



        /// <summary>
        /// Disposes the service access
        /// </summary>
        public void Dispose()
        {
            foreach(IDisposable client in this.clients)
            {
                client.Dispose();
            }

            this.clients.Clear();
        }



        /// <summary>
        /// Returns the service description by name
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private MServiceDescription GetServiceDescription(string serviceName)
        {
            MServiceDescription description = null;
            if (!serviceDescriptions.TryGetValue(serviceName, out description))
            {
                this.Initialize();
            }

            if (!serviceDescriptions.TryGetValue(serviceName, out description))
            {
                return null;
            }

            return serviceDescriptions[serviceName];
        }

    }
}
