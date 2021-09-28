// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Janis Sprenger, Stephan Adam

// This class provides the required methods for connecting to the Retargeting service
// and defines the methods for utilizing the Retargeting service.

#include "RetargetingAccess.h"

/**
* Retargeting Access enables the access to a remote retargeting service.
**/
RetargetingAccess::RetargetingAccess( string sessionId, const MIPAddress& mmiRegisterAddress,
                                      const int maxTime, const string& _avatarID )
{
    this->_sessionID = sessionId;
    this->_avatarID = _avatarID;
    this->_mmiRegisterAddress = mmiRegisterAddress;
    this->_maxTime = maxTime;

    if( !this->Connect() )
    {
        throw MOSIMException( "Retargeting connection failed!" );
    }
}

RetargetingAccess::~RetargetingAccess()
{
    // close the thrift connection
    if( this->_retargetingClient )
        delete this->_retargetingClient;
}

/**
* Connect can be used to connect to the retargeting service. This function currently is directly
*called by the
**/
bool RetargetingAccess::Connect()
{
    vector<MServiceDescription> serviceDescriptions;
    bool adapterDescriptionReceived = false;
    for( int i = 0; i <= this->_maxTime; i++ )
    {
        try
        {
            // Connect to registry and receive registered services.
            ThriftClient<MMIRegisterServiceClient> client( this->_mmiRegisterAddress.Address,
                                                           this->_mmiRegisterAddress.Port, true );
            client.access->GetRegisteredServices( serviceDescriptions, this->_sessionID );
            adapterDescriptionReceived = true;
        }
        catch( exception e )
        {
            string message = "Cannot connect to mmi register";
            Logger::printLog( L_ERROR, message );
            std::cout << "Cannot connect to mmi register, message: " << e.what() << endl;
            this_thread::sleep_for( chrono::seconds( 1 ) );
            if( i == this->_maxTime )
            {
                adapterDescriptionReceived = false;
                throw e;
                break;
            }
        }
    }
    if( adapterDescriptionReceived )
    {
        for( int i = 0; i < serviceDescriptions.size(); i++ )
        {
            if( serviceDescriptions[i].Name == this->RETARGETING_SERVICE_NAME )
            {
                MServiceDescription _retargetingServiceDesc = serviceDescriptions[i];
                try
                {
                    // Iterate services an access service with name "Retargeting".
                    this->_retargetingClient = new ThriftClient<MRetargetingServiceClient>(
                        _retargetingServiceDesc.Addresses[0].Address,
                        _retargetingServiceDesc.Addresses[0].Port, true );
                    return true;
                }
                catch( exception e )
                {
                    string message = "Cannot connect to retargeting Service";  // +e.what();
                    Logger::printLog( L_ERROR, message );
                    std::cout << "Cannot connect to mmi register, message: " << e.what() << endl;
                    return false;
                }
                break;
            }
        }
    }
    return false;
}

//////////////////////////////////////////////////////////////////////
// Interface function forwards:

MAvatarDescription RetargetingAccess::SetupRetargeting( const MAvatarPosture& globalTarget )
{
    MAvatarDescription desc;
    this->_retargetingClient->access->SetupRetargeting( desc, globalTarget );
    return desc;
}
MAvatarPostureValues RetargetingAccess::RetargetToIntermediate( const MAvatarPosture& globalTarget )
{
    MAvatarPostureValues vals;
    this->_retargetingClient->access->RetargetToIntermediate( vals, globalTarget );
    return vals;
}
MAvatarPosture RetargetingAccess::RetargetFromIntermediate(
    const MAvatarPostureValues& intermediatePostureValues )
{
    MAvatarPosture post;
    this->_retargetingClient->access->RetargetToTarget( post, intermediatePostureValues );
    return post;
}
