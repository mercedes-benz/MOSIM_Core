## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald

# -*- coding: utf-8 -*-
"""

"""

from thrift.transport import TSocket
from thrift.transport import TTransport
from thrift.protocol import TCompactProtocol

class ThriftClient(object):
    """
    A wrapper for a service client connection
    
    Attributes
    ----------
    _address : str
        The address of the service
    _port : str
        The port of the service
    _transport : TTransport
        The thrift transport
    _access_type : type
        The type of the actual client
    _acces : Iface
        The actual access
    """
    
    def __init__(self, address, port, access_type):
        """
        Constructor which needs an address, a port and an access_type.
        
        Parameters
        ----------
        address : str
            The address of the service
        port : str
            The port of the service
        access_type : type
            The type of the actual client
        """
        
        assert(isinstance(address, str)), "The address is no string"
        assert(isinstance(port, int)), "The port is no int"
        assert(isinstance(access_type, type)), "The access_type is no type"
        
        self._address = address
        self._port = port
        self._transport = None
        self._access_type = access_type
        self._access = None
        
    def __enter__(self):
        """
        Used when entering pythons with statement.
        """
        self._transport = TSocket.TSocket(host=self._address, port=self._port)
        self._transport = TTransport.TBufferedTransport(self._transport)
        protocol = TCompactProtocol.TCompactProtocol(self._transport)
        
        self._access = self._access_type(protocol)
        
        self._transport.open()
        
        return self
    
    def __exit__(self, exc_type, exc_value, traceback):
        """
        Used when exiting pythons with statement.
        """
        self.dispose()
    
    def dispose(self):
        self._transport.close()
            
        