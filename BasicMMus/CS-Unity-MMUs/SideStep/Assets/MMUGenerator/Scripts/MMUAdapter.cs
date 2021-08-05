// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Adapter;
using MMICSharp.Common;
using MMICSharp.Common.Communication;
using MMIStandard;
using MMIUnity;
using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A selfhosting adapter for the MMU.
/// This allows easy debugging.
/// </summary>
[RequireComponent(typeof(UnityMMUBase))]
public class MMUAdapter: MonoBehaviour
{
    /// <summary>
    /// A seperate class for setting and accessing the MIPAddress in unity
    /// </summary>
    [Serializable]
    public class UnityMIPAddress
    {
        public string Address;
        public int Port;

        public UnityMIPAddress(string address, int port)
        {
            this.Address = address;
            this.Port = port;
        }
        public MIPAddress ToMipAddress()
        {
            return new MIPAddress(this.Address, this.Port);
        }
    }

    /// <summary>
    /// The address of the adapter which should be hosted
    /// </summary>
    public UnityMIPAddress Address = new UnityMIPAddress("127.0.0.1", 8999);

    /// <summary>
    /// The address of the central register
    /// </summary>
    public UnityMIPAddress RegisterAddress = new UnityMIPAddress("127.0.0.1", 9009);

    #region internal variables
    internal MMUDescription MMUDescription;

    internal UnityMMUBase MMUInstance;

    #endregion

    #region private variables

    private MIPAddress address = new MIPAddress("127.0.0.1", 8999);
    private MIPAddress registerAddress = new  MIPAddress("127.0.0.1", 9009);

    /// <summary>
    /// Instance of the utilized server to communicate with the mmu abstraction
    /// </summary>
    private AdapterController adapterController;

    #endregion

    private void Start()
    {
        this.address = this.Address.ToMipAddress();
        this.registerAddress = this.RegisterAddress.ToMipAddress();

        //Add a main thread dispatcher (if not alreay available)
        if (GameObject.FindObjectOfType<MainThreadDispatcher>() == null)
            this.gameObject.AddComponent<MainThreadDispatcher>();

        this.MMUInstance = this.GetComponent<UnityMMUBase>();
        this.MMUDescription = Serialization.FromJsonString<MMUDescription>(System.IO.File.ReadAllText("Assets/"+ this.MMUInstance.name +"/"+ "description.json"));


        if(this.MMUDescription.Version == null)
        {
            this.MMUDescription.Version = "1.0";
        }
        this.Start(new MAdapterDescription()
        {
            Name ="UnityMMUDebugAdapter",
            ID = "debugid",
            Addresses = new List<MIPAddress>() { this.address },
            Language ="UnityC#"               
        }, new MMUAdapterImplementation(registerAddress,this.MMUDescription, MMUInstance));
    }

    /// <summary>
    /// Starts the adapter controller
    /// Optionally a custom adapterImplementation can be specified in here
    /// </summary>
    /// <param name="adapterImplementation"></param>
    public void Start(MAdapterDescription description, MMIAdapter.Iface adapterImplementation)
    {
        //Create a new session data
        SessionData sessionData = new SessionData
        {
            MMIRegisterAddress = registerAddress
        };

        //Create a new adapter controller
        this.adapterController = new AdapterController(sessionData, description, registerAddress, new LocalMMUProvider(this), new LocalMMUInstantiator(this), new MMUAdapterImplementation(registerAddress, MMUDescription, MMUInstance));
       
        //Start the controller
        this.adapterController.Start();
    }

    /// <summary>
    /// Dipose the controller if the application is closed
    /// </summary>
    private void OnApplicationQuit()
    {
        if(this.adapterController!=null)
            this.adapterController.Dispose();
    }
}

#region classes for loading and mangagin the local MMU

public class LocalMMUProvider : IMMUProvider
{
    public event EventHandler<EventArgs> MMUsChanged;

    private MMUAdapter adapter;

    public LocalMMUProvider(MMUAdapter adapter)
    {
        this.adapter = adapter;
    }

    public Dictionary<string, MMULoadingProperty> GetAvailableMMUs()
    {
        return new Dictionary<string, MMULoadingProperty>() 
        { 
            { adapter.MMUDescription.ID, new MMULoadingProperty() { Description = adapter.MMUDescription } }
        };
    }
}

public class LocalMMUInstantiator : IMMUInstantiation
{
    private UnityMMUBase mmuInstance; 

    public LocalMMUInstantiator(MMUAdapter adapter)
    {
        this.mmuInstance = adapter.MMUInstance;
    }

    public IMotionModelUnitDev InstantiateMMU(MMULoadingProperty loadingProperty)
    {
        return mmuInstance;
    }
}



public class MMUAdapterImplementation : MMIAdapter.Iface
{
    public MMUDescription MMUDescription;
    public IMotionModelUnitDev MMUInstance;

    private MMIScene sceneBuffer = new MMIScene();

    public MMUAdapterImplementation(MIPAddress registerAddress, MMUDescription mmuDescription, IMotionModelUnitDev mmuinstance)
    {
        this.MMUDescription = mmuDescription;
        this.MMUInstance = mmuinstance;

        this.MMUInstance.ServiceAccess = new ServiceAccess(registerAddress, "");

        //To do -> synchronize the scene
        this.MMUInstance.SceneAccess = this.sceneBuffer;
        
        //To do-> implement the skeleton access   
           
    }


    public Dictionary<string, string> GetStatus()
    {
        return new Dictionary<string, string>()
        {
            { "UnityTestAdapter", "Test"}
        };
    }


    public MBoolResponse CreateSession(string sessionID)
    {
        return new MBoolResponse(true);
    }

    public MBoolResponse CloseSession(string sessionID)
    {
        return new MBoolResponse(true);
    }



    public MBoolResponse Abort(string instructionID, string mmuID, string sessionID)
    {
        if(mmuID == MMUDescription.ID)
        {
            return MMUInstance.Abort(instructionID);
        }

        return new MBoolResponse(false);
    }

    public MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties, string mmuID, string sessionID)
    {
        if (mmuID == MMUDescription.ID)
        {
            return MMUInstance.Initialize(avatarDescription,properties);
        }

        return new MBoolResponse(false);
    }

    public MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState, string mmuID, string sessionID)
    {
        if (mmuID == MMUDescription.ID)
        {
            return MMUInstance.AssignInstruction(instruction, simulationState);
        }
        return new MBoolResponse(false);

    }

    public MBoolResponse CheckPrerequisites(MInstruction instruction, string mmuID, string sessionID)
    {
        if (mmuID == MMUDescription.ID)
        {
            return MMUInstance.CheckPrerequisites(instruction);
        }
        return new MBoolResponse(false);
    }



    public byte[] CreateCheckpoint(string mmuID, string sessionID)
    {
        if (mmuID == MMUDescription.ID)
        {
            return MMUInstance.CreateCheckpoint();
        }

        return null;      
    }

    public MBoolResponse RestoreCheckpoint(string mmuID, string sessionID, byte[] checkpointData)
    {
        if (mmuID == MMUDescription.ID)
        {
            return MMUInstance.RestoreCheckpoint(checkpointData);
        }

        return new MBoolResponse(false);
    }


    public MBoolResponse Dispose(string mmuID, string sessionID)
    {
        if (mmuID == MMUDescription.ID)
        {
            return MMUInstance.Dispose(new Dictionary<string, string>());
        }

        return new MBoolResponse(false);
    }

    public MSimulationResult DoStep(double time, MSimulationState simulationState, string mmuID, string sessionID)
    {
        if (mmuID == MMUDescription.ID)
        {
            return MMUInstance.DoStep(time,simulationState);
        }
        return null;
    }

    public Dictionary<string, string> ExecuteFunction(string name, Dictionary<string, string> parameters, string mmuID, string sessionID)
    {
        if (mmuID == MMUDescription.ID)
        {
            return MMUInstance.ExecuteFunction(name, parameters);
        }
        return null;
    }

    public MAdapterDescription GetAdapterDescription()
    {
        return new MAdapterDescription();
    }

    public List<MConstraint> GetBoundaryConstraints(MInstruction instruction, string mmuID, string sessionID)
    {
        if (mmuID == MMUDescription.ID)
        {
            return MMUInstance.GetBoundaryConstraints(instruction);
        }
        return null;
    }

    public MMUDescription GetDescription(string mmuID, string sessionID)
    {
        if (mmuID == MMUDescription.ID)
        {
            return MMUDescription;
        }
        return null;
    }

    public List<MMUDescription> GetLoadableMMUs()
    {
        return new List<MMUDescription>() { MMUDescription };
    }

    public List<MMUDescription> GetMMus(string sessionID)
    {
        return new List<MMUDescription>() { MMUDescription };
    }

    public List<MSceneObject> GetScene(string sessionID)
    {
        return new List<MSceneObject>();
    }

    public MSceneUpdate GetSceneChanges(string sessionID)
    {
        return new MSceneUpdate();
    }


    public Dictionary<string,string> LoadMMUs(List<string> mmus, string sessionID)
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();

        if (mmus.Contains(MMUDescription.ID))
        {
            //Instantiate MMU
            dictionary.Add(MMUDescription.ID, MMUDescription.ID);
        }

        return dictionary;
    }

    public MBoolResponse PushScene(MSceneUpdate sceneUpdates, string sessionID)
    {
        //Update the scene 
        return this.sceneBuffer.Apply(sceneUpdates);
    }


}

#endregion