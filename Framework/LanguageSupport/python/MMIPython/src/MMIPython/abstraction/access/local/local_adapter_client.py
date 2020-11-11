## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald


# -*- coding: utf-8 -*-
"""

"""

from MMIStandard.register import MMIAdapter
#from MMIStandard import MMIAdapter

from MMIPython.abstraction.access.interface.adapter_client import IAdapterClient


class LocalAdapterClient(IAdapterClient):
    """
    A wrapper for an adapter client connection
    
    Attributes
    ----------
    _acces : MMIAdapter.Iface
        The actual access
    """
    
    def __init__(self, instance):
        """
        Constructor which needs an address, a port and an access_type.
        
        Parameters
        ----------
        instance : MMIAdapter.Iface
            The local instance
        """
        
        assert(isinstance(instance, MMIAdapter.Iface)), "The instance is no MMIAdapter"
        
        super(LocalAdapterClient, self).__init__()
        
        self._access = instance