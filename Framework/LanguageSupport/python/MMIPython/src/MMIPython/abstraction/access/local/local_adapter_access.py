## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald

# -*- coding: utf-8 -*-
"""

"""

from MMIStandard.register.ttypes import MAdapterDescription
from MMIStandard.register import MMIAdapter

from MMIPython.abstraction.access.interface.adapter_access import IAdapterAccess
import MMIPython.abstraction.mmu.mmu_access
from MMIPython.abstraction.access.local.local_adapter_client import LocalAdapterClient

class LocalAdapterAccess(IAdapterAccess):
    
    """
    The adapter access.
    
    Attributes
    ----------
    description : AdapterDescription
        The description of the adapter
    initialized : bool
        Flag which indicates whether the communciaton has been successfully initialized
    aborted : bool
        Flag which indicates whether the initialization has been aborted
    scene_synchronized : bool
        Flag which indicates whether the scene has been successfully synchronized
    loaded : bool
        Flag which indicates whether the mmus of this adapter have been succesfully loaded
    mmu_descriptions : list<MMUDescription>
        The list of mmus of this adpater
    
    _client : IAdapterClient
        The client adapter
    _mmuAccess : MMUAccess
        The MMU Access
    _instance : MMIAdapter.Iface
        The local adapter
    """
    
    def __init__(self, description, instance, mmuAccess):
        
        """
        Basic initialization of an adapter access
        
        Parameters
        ----------
        description : AdapterDescription
            The description of the adapter
        instance : MMIAdapter.Iface
            The port of the adapter
        mmuAccess : MMUAccess
            The MMU Access
        """
        
        assert (isinstance(mmuAccess, MMIPython.abstraction.mmu.mmu_access.MMUAccess)),"mmuAccess is no MMUAccess"
        assert (isinstance(description, MAdapterDescription)),"description is no AdapterDescription"
        assert (isinstance(instance, MMIAdapter.Iface)), "instance is no MMIAdapter.Iface"
        
        super(LocalAdapterAccess, self).__init__(mmuAccess)
        
        self.description = description
        self._instance = instance
        
    def start(self):
        """
        Method starts the process
        """
        self._client = self.create_client()
        self.initialized = True
        
    def create_client(self):
        """
        Creates a client for communication with the adapter
        
        Returns
        -------
        IAdapterClient
            The communication client
        """
        return LocalAdapterClient(self._instance)