## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald


# -*- coding: utf-8 -*-
"""

"""

class IAdapterClient(object):
    """
    A wrapper for an adapter client connection
    
    Attributes
    ----------
    _acces : MMIAdapter.Iface
        The actual access
    """
    
    def __init__(self):
        """
        Constructor
        """
        
        self._access = None
        
    def get_adapter(self):
        """
        Returns the actual adapter
        
        Returns
        ------
        MMIAdapter.Iface
            The actual adapter
        """
        return self._access
    
    def dispose(self):
        """
        Closes the connection to the adapter.
        """
        pass