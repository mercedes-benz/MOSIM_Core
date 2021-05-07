## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald, Janis Sprenger


import os
import json
import time
import threading
from datetime import datetime

from MMIStandard.core.ttypes import MBoolResponse, MIPAddress
from MMIStandard.register.ttypes import MAdapterDescription
from MMIStandard.register import MMIRegisterService, MMIAdapter

from MMIPython.adapter.session.session_data import SessionData
from MMIPython.core.utils.thrift_client import ThriftClient
from MMIPython.adapter.thrift_adapter_implementation import ThriftAdapterImplementation
from MMIPython.core.utils.description_utils import create_mmu_description


from thrift.transport import TSocket
from thrift.transport import TTransport
from thrift.protocol import TCompactProtocol
from thrift.server import TServer

import uuid

class AdapterController(object):
    """starts all adapter functions"""

    def __init__(self, a_address,r_address,mmuPath, mmuclasses):
        """
            The basic constructor of the controller
        
            Parameters
            ----------
            r_ip_address : MIPAddress the Register MIPAddress
            r_ip_address : MIPAddress the Adapter MIPAddress
            mmuPath: string : the path where the MMUs are loaded
            mmuclasses: list of tuples: (json description, Class)
        """
        assert (isinstance(a_address, MIPAddress)),"aAddress is no MIPAddress"
        assert (isinstance(r_address, MIPAddress)),"rAddress is no MIPAddress"
        assert (isinstance(mmuPath, str)),"aAddress is no String"
       
        self.a_address=a_address
        self.r_address=r_address
        self.mmuPath=mmuPath
        self.session_data = SessionData(r_address)
        #create adapter descritpion
        self.session_data.adapter_description=MAdapterDescription(Name="PythonAdapter",ID=str(uuid.uuid1()),Language="Python",Addresses=[a_address],Properties=None,Parameters=None)
        self.session_data.start_time=datetime.now()
        self.mmuclasses = mmuclasses



    def start(self):
        thread1 = threading.Thread(target=self.prepare_mmus,args=())
        thread1.start()
        thread2=threading.Thread(target=self.register_adapter,args=())
        thread2.start()
        thread3=threading.Thread(target=self.start_server,args=())
        thread3.start()

        thread1.join()
        thread2.join()
        thread3.join()
       

    def register_adapter(self):
        registered = False
        while registered!=True:
            try:
                with ThriftClient(self.r_address.Address, self.r_address.Port, MMIRegisterService.Client) as client:
                        response= client._access.RegisterAdapter(self.session_data.adapter_description)
                        registered=response.Successful
                        if registered==True:
                            print("Sucessfully registered adapter at MMIRegister")
            except Exception as x:
                print("Failed to register at MMIRegister")
                time.sleep(1)
                          
       
    def start_server(self):
        print ("Starting adapter server" + self.r_address.Address + ":" + str(self.r_address.Port))
        processor = MMIAdapter.Processor( ThriftAdapterImplementation(self.session_data))
        transport = TSocket.TServerSocket(host=self.a_address.Address, port=self.a_address.Port)
        tfactory = TTransport.TBufferedTransportFactory()
        pfactory = TCompactProtocol.TCompactProtocolFactory()

        #server = TServer.TThreadedServer(processor, transport, tfactory, pfactory)
        server = TServer.TThreadPoolServer(processor,transport,tfactory,pfactory)
        server.setNumThreads(4)
        server.serve()


    def prepare_mmus(self):
        #assert (isinstance(mmu_path, str)), "mmu_path is no string"
        descriptions = list()

        

        for a in self.mmuclasses:
            (mmudesc, mmuclass) = a
            mmu_description_json = json.loads(mmudesc)
            mmu_description = create_mmu_description(mmu_description_json)
            descriptions.append(mmu_description)
            self.session_data.mmus[mmu_description.ID] = mmuclass
            print('Found MMU : {0}'.format(mmu_description.Name))

        """
        mmu_path = os.path.normpath(self.mmuPath)
        
        #print('Prepare MMUs from path : {0}'.format(mmu_path))
        
        #Load all MMUs
        for folder in os.listdir(mmu_path):
            if os.path.isdir(os.path.join(mmu_path,folder)):
                for f in os.listdir(os.path.join(mmu_path,folder)):
                    if 'description.json' in f:
                        #Load the file 
                        description_file_path = os.path.join(mmu_path,folder,f)
                        
                        with open(description_file_path) as jsonText:
                            
                            
                            mmu_description_json = json.load(jsonText)
                            
                            
                            if  mmu_description_json['Language'] == "Python" or mmu_description_json['Language'] == "python":
                                
                                mmu_description = create_mmu_description(mmu_description_json)
                                descriptions.append(mmu_description)
                            
                                self.session_data.mmu_loading_properties.append((os.path.join(mmu_path,folder,mmu_description.AssemblyName), mmu_description))
                                
                                print('Found MMU : {0}'.format(mmu_description.Name))
       
        #self._session_data.mmu_descriptions = description
        """
        self.session_data.mmu_descriptions=descriptions    
    
      

      


