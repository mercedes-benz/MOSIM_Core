## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald


# -*- coding: utf-8 -*-
"""

"""

from thrift.transport import TSocket
from thrift.transport import TTransport
from thrift.protocol import TCompactProtocol

from MMIStandard.register.ttypes import MAdapterDescription
#from MMIStandard import MMIAdapter

from MMIPython.abstraction.access.interface.adapter_client import IAdapterClient


class RemoteAdapterClient(IAdapterClient):
    """
    A wrapper for a adapter client connection
    
    Attributes
    ----------
    _address : str
        The address of the service
    _port : str
        The port of the service
    _transport : TTransport
        The thrift transport
    _acces : MMIAdapter.Iface
        The actual access
    """
    
    def __init__(self, address, port):
        """
        Constructor which needs an address, a port and an access_type.
        
        Parameters
        ----------
        address : str
            The address of the service
        port : str
            The port of the service
        """
        
        assert(isinstance(address, str)), "The address is no string"
        assert(isinstance(port, int)), "The port is no int"
        
        super(RemoteAdapterClient, self).__init__()
        
        if address == "127.0.0.1":
            address = "localhost"
        
        self._address = address
        self._port = port
        
        try:
            
            self._transport = TSocket.TSocket(host=self._address, port=self._port)
            self._transport = TTransport.TBufferedTransport(self._transport)
            protocol = TCompactProtocol.TCompactProtocol(self._transport)
            
            self._access = MMIAdapter.Client(protocol)
            self._transport.open()
            
            print('Connected to to adapter {0}:{1}'.format(address, port))
        except:
            print('Could not connect to to adapter {0}:{1}'.format(address, port))
    
    def dispose(self):
        """
        Closes the connection to the adapter.
        """
        
        super(RemoteAdapterClient, self).dispose()
        
        try:
            self._transport.close()
        except:
            print('Could not close connection to adapter {0}:{1}'.format(self._address, self._port))