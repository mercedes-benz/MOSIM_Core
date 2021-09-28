// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// This class provides the required methods for connecting to the Skeleton Access service
// and defines the methods for utilizing the Skeleton Access service.

#include "SkeletonAccess.h"

/**
* Skeleton Access enables the access to a remote skeleton service.
**/
SkeletonAccess::SkeletonAccess( string sessionId, const MIPAddress& mmiRegisterAddress,
                                const int maxTime, const string& _avatarID )
    : skeletonServiceAddr( new MIPAddress() )
{
    this->_sessionID = sessionId;
    this->_avatarID = _avatarID;
    this->_mmiRegisterAddress = mmiRegisterAddress;
    this->_maxTime = maxTime;

    if( !this->Connect() )
    {
        throw MOSIMException( "Skeleton Access connection failed!" );
    }
}

SkeletonAccess::~SkeletonAccess()
{
    // close the thrift connection again
    if( this->_skeletonClient )
        delete _skeletonClient;
}

/**
* Connect can be used to connect to the skeleton service. This function currently is directly called
*by the
**/
bool SkeletonAccess::Connect()
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
                MServiceDescription _skeletonServiceDesc = serviceDescriptions[i];
                try
                {
                    // Iterate services an access service with name "Retargeting".
                    this->_skeletonClient = new ThriftClient<MSkeletonAccessClient>(
                        _skeletonServiceDesc.Addresses[0].Address,
                        _skeletonServiceDesc.Addresses[0].Port, true );
                    this->skeletonServiceAddr->__set_Address(
                        _skeletonServiceDesc.Addresses[0].Address );
                    this->skeletonServiceAddr->__set_Port( _skeletonServiceDesc.Addresses[0].Port );
                    return true;
                }
                catch( exception e )
                {
                    string message = "Cannot connect to skeleton access Service";  // +e.what();
                    Logger::printLog( L_ERROR, message );
                    std::cout << "Cannot connect to mmi skeleton access Service, message: "
                              << e.what() << endl;
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

void SkeletonAccess::InitializeAnthropometry( const MAvatarDescription& description )
{
    this->_skeletonClient->access->InitializeAnthropometry( description );
}

MAvatarDescription SkeletonAccess::GetAvatarDescription( const string& avatarID )
{
    MAvatarDescription avatarDesc = MAvatarDescription();
    this->_skeletonClient->access->GetAvatarDescription( avatarDesc, avatarID );
    return avatarDesc;
}

void SkeletonAccess::SetAnimatedJoints( const string& avatarID,
                                        const vector<MJointType::type>& joints )
{
    this->_skeletonClient->access->SetAnimatedJoints( avatarID, joints );
}

void SkeletonAccess::SetChannelData( const MAvatarPostureValues& values )
{
    this->_skeletonClient->access->SetChannelData( values );
}

MAvatarPosture SkeletonAccess::GetCurrentGlobalPosture( const string& avatarID )
{
    MAvatarPosture posture = MAvatarPosture();
    this->_skeletonClient->access->GetCurrentGlobalPosture( posture, avatarID );
    return posture;
}

MAvatarPosture SkeletonAccess::GetCurrentLocalPosture( const string& avatarID )
{
    MAvatarPosture posture = MAvatarPosture();
    this->_skeletonClient->access->GetCurrentLocalPosture( posture, avatarID );
    return posture;
}

MAvatarPostureValues SkeletonAccess::GetCurrentPostureValues( MAvatarPostureValues& _return,
                                                              const string& avatarID )
{
    MAvatarPostureValues postureVals = MAvatarPostureValues();
    this->_skeletonClient->access->GetCurrentPostureValues( postureVals, avatarID );
    return postureVals;
}

MAvatarPostureValues SkeletonAccess::GetCurrentPostureValuesPartial(
    const string& avatarID, const vector<MJointType::type>& joints )
{
    MAvatarPostureValues postureVals = MAvatarPostureValues();
    this->_skeletonClient->access->GetCurrentPostureValuesPartial( postureVals, avatarID, joints );
    return postureVals;
}

vector<MVector3> SkeletonAccess::GetCurrentJointPositions( const string& avatarID )
{
    vector<MVector3> vectorVector = vector<MVector3>();
    this->_skeletonClient->access->GetCurrentJointPositions( vectorVector, avatarID );
    return vectorVector;
}

MVector3 SkeletonAccess::GetRootPosition( const string& avatarID )
{
    MVector3 returnVec = MVector3();
    this->_skeletonClient->access->GetRootPosition( returnVec, avatarID );
    return returnVec;
}

MQuaternion SkeletonAccess::GetRootRotation( const string& avatarID )
{
    MQuaternion returnQuat = MQuaternion();
    this->_skeletonClient->access->GetRootRotation( returnQuat, avatarID );
    return returnQuat;
}

MVector3 SkeletonAccess::GetGlobalJointPosition( const string& avatarId,
                                                 const MJointType::type joint )
{
    MVector3 returnVec = MVector3();
    this->_skeletonClient->access->GetGlobalJointPosition( returnVec, avatarId, joint );
    return returnVec;
}

MQuaternion SkeletonAccess::GetGlobalJointRotation( const string& avatarId,
                                                    const MJointType::type joint )
{
    MQuaternion returnQuat = MQuaternion();
    this->_skeletonClient->access->GetGlobalJointRotation( returnQuat, avatarId, joint );
    return returnQuat;
}

MVector3 SkeletonAccess::GetLocalJointPosition( const string& avatarId,
                                                const MJointType::type joint )
{
    MVector3 returnVec = MVector3();
    this->_skeletonClient->access->GetLocalJointPosition( returnVec, avatarId, joint );
    return returnVec;
}

MQuaternion SkeletonAccess::GetLocalJointRotation( const string& avatarId,
                                                   const MJointType::type joint )
{
    MQuaternion returnQuat = MQuaternion();
    this->_skeletonClient->access->GetLocalJointRotation( returnQuat, avatarId, joint );
    return returnQuat;
}

void SkeletonAccess::SetRootPosition( const string& avatarId, const MVector3& position )
{
    this->_skeletonClient->access->SetRootPosition( avatarId, position );
}

void SkeletonAccess::SetRootRotation( const string& avatarId, const MQuaternion& rotation )
{
    this->_skeletonClient->access->SetRootRotation( avatarId, rotation );
}

void SkeletonAccess::SetGlobalJointPosition( const string& avatarId, const MJointType::type joint,
                                             const MVector3& position )
{
    this->_skeletonClient->access->SetGlobalJointPosition( avatarId, joint, position );
}

void SkeletonAccess::SetGlobalJointRotation( const string& avatarId, const MJointType::type joint,
                                             const MQuaternion& rotation )
{
    this->_skeletonClient->access->SetGlobalJointRotation( avatarId, joint, rotation );
}

void SkeletonAccess::SetLocalJointPosition( const string& avatarId, const MJointType::type joint,
                                            const MVector3& position )
{
    this->_skeletonClient->access->SetLocalJointPosition( avatarId, joint, position );
}

void SkeletonAccess::SetLocalJointRotation( const string& avatarId, const MJointType::type joint,
                                            const MQuaternion& rotation )
{
    this->_skeletonClient->access->SetLocalJointRotation( avatarId, joint, rotation );
}

MAvatarPostureValues SkeletonAccess::RecomputeCurrentPostureValues( const string& avatarId )
{
    MAvatarPostureValues postureVals = MAvatarPostureValues();
    this->_skeletonClient->access->RecomputeCurrentPostureValues( postureVals, avatarId );
    return postureVals;
}