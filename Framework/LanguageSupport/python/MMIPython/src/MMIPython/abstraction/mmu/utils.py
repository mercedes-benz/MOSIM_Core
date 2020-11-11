## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald


# -*- coding: utf-8 -*-
"""
Contains helper methods for the access classes
"""

from threading import Thread

from MMIStandard.register.ttypes import MAdapterDescription
from MMIStandard.core.ttypes import MIPAddress
#from MMIStandard.ttypes import AdapterDescription, MBoneType, MIPAddress
from MMIStandard.register import MMIAdapter
from MMIStandard.register import MMIRegisterService
#from MMIStandard import MMIAdapter, MMURegisterService

from MMIPython.core.utils.thrift_client import ThriftClient

import MMIPython.abstraction.access.interface.adapter_access
import MMIPython.abstraction.access.remote.remote_adapter_access
import MMIPython.abstraction.access.local.local_adapter_access
import MMIPython.abstraction.mmu.mmu_access


#def get_adapter_names(address):
#    """
#    Connects to the given address and returns all available adapter names
#    
#    Parameters
#    ---------
#    address : str
#        The address
#        
#    Returns
#    ---------
#    list<str>
#        All available adapters
#    """
#    
#    assert(isinstance(address, str)), "The given address is no string"
#    
#    print(address)
#    
#    try:
#        r = requests.get("http://{0}/adapters/".format(address))
#        names = r.json()
#        print('Adapter names could be received : {0}'.format(names))
#        return names
#    except:
#        print('Adapter names could not be received.')
#        return None
        
def get_adapter_descriptions(ip_address, session_id):
    """
    Connects to the given address and returns the description for all found adapters
    
    Parameters
    ---------
    ip_address : MIPAddress
        The address
        
    Returns
    ---------
    list<AdapterDescription>
        All available adapters
    """
    
    assert (isinstance(ip_address, MIPAddress)), "The given ip_address is no MIPAddress"
    
    
    # Get the service descriptions from the mmu register
    with ThriftClient(ip_address.Address, ip_address.Port, MMIRegisterService.Client) as client:
        descriptions = client._access.GetRegisteredAdapters(session_id)
    
    if descriptions is None or len(descriptions) == 0:
        print('No adapter descriptions received.')
    
    return descriptions

def create_remote_adapter_accesses(mmu_access, adapter_descriptions):
    """
    Creates adapters accesses from given descriptions.
    
    Parameters
    ------------
    mmu_access : MMUAccess
        The mmu access the adapters are connected to
    adapter_descriptions : list<AdapterDescription>
        The adapter descriptions
        
    Returns
    ---------
    list<IAdapterAccess>
        The created adapter accesses
    """
    
    assert (isinstance(mmu_access, MMIPython.abstraction.mmu.mmu_access.MMUAccess)), "The given mmu_access is no MMUAccess"
    assert (isinstance(adapter_descriptions, list)), "The given adapter_descriptions is no list"
    assert (all(isinstance(x, MAdapterDescription) for x in adapter_descriptions)), "Not all members are of type AdapterDescription in adapters"
    
    adapters = list()
    
    # Fetch all adapters and create a connection
    for description in adapter_descriptions:
        adapter = MMIPython.abstraction.access.remote.remote_adapter_access.RemoteAdapterAccess(description.Address.Address, description.Address.Port, mmu_access)
        adapter.description = description
        adapters.append(adapter)
        
    return adapters

def create_local_adapter_access(mmu_access, adapter_description, adapter):
    """
    Creates a local adapter access.
    
    Parameters
    ------------
    mmu_access : MMUAccess
        The mmu access the adapters are connected to
    adapter_descriptions : AdapterDescription
        The adapter description
    adapter : MMIAdapter.Iface
        The adapter
        
    Returns
    ---------
    IAdapterAccess
        The created adapter access
    """
    
    assert (isinstance(mmu_access, MMIPython.abstraction.mmu.mmu_access.MMUAccess)), "The given mmu_access is no MMUAccess"
    assert (isinstance(adapter_description, MAdapterDescription)), "The given adapter_description is no AdapterDescription"
    assert (isinstance(adapter, MMIAdapter.Iface)), "The given adapter is no MMIAdapter.Iface"
    
    return MMIPython.abstraction.access.local.local_adapter_access.LocalAdapterAccess(adapter_description, adapter, mmu_access)

def start_adapter_accesses(adapters, asynchronously):
    """
    Starts the given adapters.
    
    Parameters
    ------------
    adapters : list<IAdapterAccess>
        The adapters accesses to start
    asynchronously : bool
        Flag which enables/disables asynchronous start
    """
    
    assert (isinstance(adapters, list)), "The given adapters is no list"
    assert (all(isinstance(x, MMIPython.abstraction.access.interface.adapter_access.IAdapterAccess) for x in adapters)), "Not all members are of type AdapterAccess in adapters"
    assert (isinstance(asynchronously, bool)), "The given asynchronously is no bool"
    
    # Start adapters asynchonously
    if asynchronously:
    
        # Start adapter multithreaded
        threads = list()
        for adapter in adapters:
            process = Thread(target=adapter.start)
            process.start()
            threads.append(process)
            
        # Wait for the finishing of all start methods
        for process in threads:
            process.join()
    
    # Start adapters synchonously
    else:
        for adapter in adapters:
            adapter.start()
            
# def convert_bone_mapping(bone_mapping):
#     """
#     Converts the values of a bone mapping dict to the actual bone types instead of string representation.
    
#     Parameters
#     ---------
#     bone_mapping: dict<str,str>
#         The input bone mapping
    
#     Returns
#     dict<str, MBoneType>
#         The converted bone mapping
#     """
    
#     assert (isinstance(bone_mapping, dict)), "bone_mapping is no dict."
#     assert (all(isinstance(x, str) for x in bone_mapping.keys())), "Not all keys are of type string in bone_mapping"
#     assert (all(isinstance(x, str) for x in bone_mapping.values())), "Not all values are of type string in bone_mapping"
    
#     converted = {}
    
#     for k, v in bone_mapping.items():
#         converted[k] = MBoneType[v]
        
#     return converted
    
    
    
    