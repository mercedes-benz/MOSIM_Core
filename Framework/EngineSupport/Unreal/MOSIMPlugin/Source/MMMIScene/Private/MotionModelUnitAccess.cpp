// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam, Janis Sprenger

// This class provides the methods for handling MMUs.

#include "MotionModelUnitAccess.h"
#include "RemoteAdapterAccess.h"

#include "MMUAccess.h"

#include <thread>
#include <chrono>
#include <stdexcept>

MotionModelUnitAccess::MotionModelUnitAccess( MMUAccess* mmuAccess, RemoteAdapterAccess* adapter,
                                              const string sessionId, MMUDescription description )
    : mmuAccess( mmuAccess ),
      Adapter( adapter ),
      sessionId( sessionId ),
      Description( description ),
      MotionType( description.MotionType ),
      Name( description.Name ),
      ID( description.ID )
{
}

void MotionModelUnitAccess::Initialize( MBoolResponse& _return,
                                        const MAvatarDescription& avatarDescription,
                                        const map<string, string>& properties )
{
    auto start = chrono::steady_clock::now();

    // TODO: Problems when initializing the Co-Simualtion MMU more than once
    // while (true)
    //{
    this->Adapter->thriftClient.access->Initialize( _return, avatarDescription, properties,
                                                    this->ID, this->sessionId );
    // auto stop = chrono::steady_clock::now();
    // std::chrono::duration<double> elapsedSeconds = stop - start;
    // if (_return.Successful)
    //	break;
    // else if (elapsedSeconds.count() > 10) {
    //	// TODO: Co-Simulation MMU is not able to initialize the MMUs correctly in case this is done
    //several times (e.g. with multiple avatars)
    //	runtime_error error("Problems initializing MMUs!");
    //	throw error;
    //	break;
    //}
    // else {
    //	std::this_thread::sleep_for(500ms);
    //}
    //}
}

void MotionModelUnitAccess::AssignInstruction( MBoolResponse& _return,
                                               const MInstruction& motionInstruction,
                                               const MSimulationState& simulationState )
{
    auto start = chrono::steady_clock::now();

    while( true )
    {
        this->Adapter->thriftClient.access->AssignInstruction(
            _return, motionInstruction, simulationState, this->ID, this->sessionId );
        auto stop = chrono::steady_clock::now();
        std::chrono::duration<double> elapsedSeconds = stop - start;
        if( _return.Successful )
            break;
        else if( elapsedSeconds.count() > 10 )
        {
            runtime_error error( "Problems assigning Instruction!" );
            throw error;
            break;
        }
        else
            std::this_thread::sleep_for( 500ms );
    }
}

void MotionModelUnitAccess::DoStep( MSimulationResult& _return, const double time,
                                    const MSimulationState& simulationState )
{
    this->Adapter->thriftClient.access->DoStep( _return, time, simulationState, this->ID,
                                                this->sessionId );
}

void MotionModelUnitAccess::GetBoundaryConstraints( vector<MConstraint>& _return,
                                                    const MInstruction& instruction )
{
    this->Adapter->thriftClient.access->GetBoundaryConstraints( _return, instruction, this->ID,
                                                                this->sessionId );
}

void MotionModelUnitAccess::CheckPrerequisites( MBoolResponse& _return,
                                                const MInstruction& instruction )
{
    this->Adapter->thriftClient.access->CheckPrerequisites( _return, instruction, this->ID,
                                                            this->sessionId );
}

void MotionModelUnitAccess::Abort( MBoolResponse& _return, const string& instructionId )
{
    this->Adapter->thriftClient.access->Abort( _return, instructionId, this->ID, this->sessionId );
}

void MotionModelUnitAccess::Dispose( MBoolResponse& _return, const map<string, string>& parameters )
{
    // TODO_IMPORTANT: call does not work at the moment --> find different way --> important for
    // disposing the co-simulation mmu
    this->Adapter->thriftClient.access->Dispose( _return, this->ID, this->sessionId );
}

void MotionModelUnitAccess::CreateCheckpoint( string& _return )
{
    this->Adapter->thriftClient.access->CreateCheckpoint( _return, this->ID, this->sessionId );
}

void MotionModelUnitAccess::RestoreCheckpoint( MBoolResponse& _return, const string& data )
{
    this->Adapter->thriftClient.access->RestoreCheckpoint( _return, this->ID, this->sessionId,
                                                           data );
}

void MotionModelUnitAccess::ExecuteFunction( map<string, string>& _return, const string& _name,
                                             const map<string, string>& parameters )
{
    this->Adapter->thriftClient.access->ExecuteFunction( _return, _name, parameters, this->ID,
                                                         this->sessionId );
}
