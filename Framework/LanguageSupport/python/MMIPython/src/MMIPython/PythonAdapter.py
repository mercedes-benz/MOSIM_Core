## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald, Janis Sprenger


#import json
import argparse
#import os

from thrift.transport import TSocket
from thrift.transport import TTransport
from thrift.protocol import TCompactProtocol
from thrift.server import TServer

from MMIStandard.core.ttypes import MIPAddress
from MMIStandard.register import MMIAdapter
from MMIPython.adapter.thrift_adapter_implementation import ThriftAdapterImplementation
from MMIPython.adapter import AdapterController
from MMIPython.adapter.session.session_data import SessionData


def __get_arguments(args):
    
    #Check command arguments
    mmuPath = None
    address = None
    port = None
    r_address = None
    r_port = None
    
    ip_address = None
    r_ip_address = None
    
    executable = True
    
    try:
        r_address_port = args.raddress
        r_address_port = r_address_port.split(':')
        r_address = r_address_port[0]
        r_port = int(r_address_port[1])
        r_ip_address = MIPAddress(r_address, r_port)
    except:
        pass
    if r_ip_address is None:
        executable = False
        print('Register address not defined. Adapter is not executable!')
        
    try:
        mmuPath = args.mmupath
    except:
        pass
    if mmuPath is None:
        executable = False
        print('MMUPath not defined. Adapter is not executable!')
        
    try:
        address_port = args.address
        address_port = address_port.split(':')
        address = address_port[0]
        port = int(address_port[1])
        ip_address = MIPAddress(address, port)
    except:
        pass
    if ip_address is None:
        executable = False
        print('Network adress not defined. Adapter is not executable!')
    
#    if description_needed:
#        #Open the description file of the module
#        directory = os.path.dirname(os.path.abspath(__file__))
#        filepath = os.sep.join((directory, r'/../', r'description.json'))
#        
#        with open(filepath) as f:
#            data = json.load(f)
#            print("Load description file: ")
#            print(data)
#            print("_________________________________________")
#            
#            
#            if mmuPath is None:
#                mmuPath = data["MMUPath"]
#            if web_address is None:
#                address = data["Web_Address"]
#            
#            moduleEndpoint = data["Module"]
#            
#            if address is None:
#                address = moduleEndpoint["Address"]
#            if port is None:
#                port = moduleEndpoint["Port"]
        
        
        
                
    return mmuPath, ip_address, r_ip_address, executable
    


def start_adapter(mmuclasses):
    parser = argparse.ArgumentParser(description = 'Start the python adapter')
    parser.add_argument('-address', help='Enter the network of this adapter.', default="127.0.0.1:9012")
    parser.add_argument('-mmupath', help='Enter the MMU path.', default=r"D:\code\MOSIM\MMUScrum\python")
    parser.add_argument('-raddress', help='Register Address + Port.', default="127.0.0.1:9009")
    args = parser.parse_args()

    print("Python MMU Adapter")
    print("_________________________________________")

    # Get parameters from command line or description file
    mmuPath, ip_address, r_ip_address, executable = __get_arguments(args)
    
    if not executable:
        print('Start of Adapter is aborted')
        exit()
    
    if __debug__:
        print("Adapter started in debug mode")
        print("MMU path = {0}".format(mmuPath))
        print("Address = {0}".format(ip_address.Address))
        print("Port = {0}".format(ip_address.Port))
        print("Register Address = {0}".format(r_ip_address.Address))
        print("Register Port = {0}".format(r_ip_address.Port))
        print("-----------------------")
       
    #    service_address = "http://" + webaddress + "/services/"
    adapter_controler = AdapterController.AdapterController(ip_address,r_ip_address,mmuPath, mmuclasses)
    adapter_controler.start()
    print('done.')
