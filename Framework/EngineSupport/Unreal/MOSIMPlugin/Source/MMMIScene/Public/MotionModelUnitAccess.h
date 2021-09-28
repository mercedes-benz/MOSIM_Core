// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam, Janis Sprenger

// This class provides the methods for handling MMUs.

#pragma once

#include <string>

#include "IMotionModelUnitAccess.h"

using namespace std;
using namespace MMIStandard;

// forward declaration
class MMUAccess;
class RemoteAdapterAccess;

class MotionModelUnitAccess : public IMotionModelUnitAccess
{
protected:
    MMUAccess* mmuAccess;
    RemoteAdapterAccess* Adapter;
    string sessionId;
    MMUDescription Description;
    string MotionType;
    string Name;
    string ID;

public:
    // constructor
    MotionModelUnitAccess( MMUAccess* mmuAccess, RemoteAdapterAccess* adapter,
                           const string sessionId, MMUDescription description );

    // inherited methods from MotionModelUnitIf
    virtual void Initialize( MBoolResponse& _return, const MAvatarDescription& avatarDescription,
                             const map<string, string>& properties ) override;
    virtual void AssignInstruction( MBoolResponse& _return, const MInstruction& motionInstruction,
                                    const MSimulationState& simulationState ) override;
    virtual void DoStep( MSimulationResult& _return, const double time,
                         const MSimulationState& simulationState ) override;
    virtual void GetBoundaryConstraints( vector<MConstraint>& _return,
                                         const MInstruction& instruction ) override;
    virtual void CheckPrerequisites( MBoolResponse& _return,
                                     const MInstruction& instruction ) override;
    virtual void Abort( MBoolResponse& _return, const string& instructionId ) override;
    virtual void Dispose( MBoolResponse& _return, const map<string, string>& parameters ) override;
    virtual void CreateCheckpoint( string& _return ) override;
    virtual void RestoreCheckpoint( MBoolResponse& _return, const string& data ) override;
    virtual void ExecuteFunction( map<string, string>& _return, const string& name,
                                  const map<string, string>& parameters ) override;

    // getter
    string getSessionId()
    {
        return this->sessionId;
    }
    string getID()
    {
        return this->ID;
    }
    string getName()
    {
        return this->Name;
    }
    string getMotionType()
    {
        return this->MotionType;
    }
    const RemoteAdapterAccess* getAdapter()
    {
        return this->Adapter;
    }
    const MMUDescription* GetMMUDescription()
    {
        return &this->Description;
    }
};
