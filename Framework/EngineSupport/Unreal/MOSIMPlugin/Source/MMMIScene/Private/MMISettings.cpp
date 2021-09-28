// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Storage for the most important IP and Port settings of the Plugin

#include "MMISettings.h"

// constructor
MMISettings::MMISettings()
    : MMIRegisterPort( 9009 ),
      MMIRegisterAddress( "127.0.0.1" ),
      MIPRegisterAddress( MIPAddress{} ),

      RemoteSceneAccessPort( 9000 ),
      RemoteSceneAccessAddress( "127.0.0.1" ),
      MIPRemoteSceneAccessAddress( MIPAddress{} ),

      RemoteSceneWritePort( 9001 ),
      RemoteSceneWriteAddress( "127.0.0.1" ),
      MIPRemoteSceneWriteAddress( MIPAddress{} ),
      AllowRemoteSceneConnections( true ),

      // TODO: probably unrequired
      // RemoteSkeletonAccessPort(9999),
      // RemoteSkeletonAccessAddress("127.0.0.1"),
      // MIPRemoteSkeletonAccessAddress(MIPAddress{}),
      // AllowRemoteSkeletonConnections(true)

      RemoteCoSimulationAccessPort( 9011 ),
      RemoteCoSimulationAccessAddress( "127.0.0.1" ),
      MIPRemoteCoSimulationAccessAddress( MIPAddress{} )

{
    MIPRegisterAddress.__set_Address( this->MMIRegisterAddress );
    MIPRegisterAddress.__set_Port( this->MMIRegisterPort );

    MIPRemoteSceneAccessAddress.__set_Address( this->RemoteSceneAccessAddress );
    MIPRemoteSceneAccessAddress.__set_Port( this->RemoteSceneAccessPort );

    MIPRemoteSceneWriteAddress.__set_Address( this->RemoteSceneWriteAddress );
    MIPRemoteSceneWriteAddress.__set_Port( this->RemoteSceneWritePort );

    MIPRemoteCoSimulationAccessAddress.__set_Address( this->RemoteCoSimulationAccessAddress );
    MIPRemoteCoSimulationAccessAddress.__set_Port( this->RemoteCoSimulationAccessPort );

    // TODO: probably unrequired
    // MIPRemoteSkeletonAccessAddress.__set_Address(this->RemoteSkeletonAccessAddress);
    // MIPRemoteSkeletonAccessAddress.__set_Port(this->RemoteSkeletonAccessPort);
}
