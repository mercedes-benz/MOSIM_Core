## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald


# -*- coding: utf-8 -*-
"""

"""
from MMIStandard.register.ttypes import MAdapterDescription
#from MMIStandard.ttypes import AdapterDescription

from MMIPython.abstraction.access.interface.adapter_access import IAdapterAccess

import MMIPython.abstraction.mmu.mmu_access
from MMIPython.abstraction.access.remote.remote_adapter_client import RemoteAdapterClient

class RemoteAdapterAccess(IAdapterAccess):
    
    """
    The adapter access.
    
    Attributes
    ----------
    address : str
        The address of the adapter
    port : dict
        The port of the adapter
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
    
    _client : Iface
        The client adapter
    _mmuAccess : MMUAccess
        The MMU Access
    """
    
    def __init__(self, address, port, mmuAccess):
        
        """
        Basic initialization of an adapter access
        
        Parameters
        ----------
        address : str
            The address of the adapter
        port : dict
            The port of the adapter
        mmuAccess : MMUAccess
            The MMU Access
        """
        
        assert (isinstance(address, str)),"address is no string"
        assert (isinstance(port, int)),"port is no int"
        assert (isinstance(mmuAccess, MMIPython.abstraction.mmu.mmu_access.MMUAccess)),"mmuAccess is no MMUAccess"
        
        super(RemoteAdapterAccess, self).__init__(mmuAccess)
        
        self.description = MAdapterDescription()
        self.description.Name = "Remote Adapter {0}".format(port)
        
        self.address = address
        self.port = port
        
    def start(self):
        """
        Method starts the connection to an adapter
        """
        
        self._client = self.create_client()
        while(not self.initialized and not self.aborted):
            try:
                self.get_status()
                self.initialized = True
            except:
                pass
            
    def create_client(self):
        """
        Creates a client for communication with the adapter
        
        Returns
        -------
        IAdapterClient
            The communication client
        """
        return RemoteAdapterClient(self.address, self.port)