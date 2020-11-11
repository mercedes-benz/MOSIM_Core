## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald, Janis Sprenger


# -*- coding: utf-8 -*-
"""

"""

#import uuid
from threading import Thread
from queue import Queue

from MMIStandard.register.ttypes import MAdapterDescription
from MMIStandard.core.ttypes import MIPAddress
# from MMIStandard.services import MServiceAccess
#from MMIStandard.ttypes import AdapterDescription, MIPAddress
from MMIStandard.register import MMIAdapter
#from MMIStandard import MMIAdapter

from MMIPython.core.services.service_access import ServiceAccess

from MMIPython.abstraction.mmu import utils

class MMUAccess(object):
    """
    Class for accessing Motion Model Units through different programming languages
    
    Attributes
    ----------
    
    mmus : list<MotionModelUnit>
        The available Motion Model Units
    intermediate_avatar_description : MAvatarDescription
        The descirption and specifications of the intermediate avatar
    
    initialized : bool
        Specifies whether the MMUs are initialized
    scene_access : ISceneAccess
        The assigned scene access
    service_access : IServiceAccess
        The assigned service access
    retargeting : IAvatarRetargeting
        The retargeting implementation used by this access
    __sessionID : str
        The unique session ID
    __services : list<MMIService>
        The available Services
    _adapters : list<IAdapterAccess>
        The available adapters
    _mmu_descriptions : list<MMUDescription>
        All gathered MMU descriptions
    """
    
    def __init__(self, sessionID, intermediate_avatar_description, retargeting = None, scene_access = None, service_access = None):
        """
        Constructor which sets a given session ID and optionally the scene access 
        and service access.
        
        Parameters
        ----------
        sessionID : str
            The session ID
        intermediate_avatar_description : MAvatarDescription, optional
            The description and specifications of the intermediate avatar
        retargeting : IAvatarRetargeting, optional
            The retargeting implementation which should be used by this access
        scene_access : SceneAccess, optional
            The scene access
        service_access : ServiceAccess, optional
            The service access
        """
        assert(isinstance(sessionID, str)), "The given ID is no string"
        
        self.__sessionID = sessionID
        
        self.__services = list()
        self._adapters = list()
        self.mmus = list()
        self.intermediate_avatar_description = intermediate_avatar_description
        self.retargeting = retargeting
        self.initialized = False
        self.scene_access = scene_access
        self.service_access = service_access
        self._mmu_descriptions = list()
        
        
    def _connect_to_adapters(self, ip_address, asynchronously):
        """
        Connects to adapters from the given address.
        
        Parameters
        ----------
        ip_address : MIPAddress
            The address
        asynchronously : bool
            Flag which enables/disables asynchronous start
            
        Returns
        ---------
        bool
            True if the connections were succefully established
        """
        
        assert(isinstance(ip_address, MIPAddress)), "The given ip_address is no MIPAddress"
        assert(isinstance(asynchronously, bool)), "asynchronously is no bool"
        
        
        # Get the descriptions of all fetched adapters
        adapter_descriptions = utils.get_adapter_descriptions(ip_address, self.__sessionID)
        
        # No adapters available
        if adapter_descriptions is None or len(adapter_descriptions) == 0:
            return False
        
        # Create adapter accesses
        self._adapters = utils.create_remote_adapter_accesses(self, adapter_descriptions)
        
        utils.start_adapter_accesses(self._adapters, asynchronously)
        
        self.initialized = True
        
        print ("Connections to adapters succefully estblished")
        
        return True
    
    def connect(self, ip_address, asynchronously = True, connect_to_services = False):
        """
        Connects to adapters and services from the given address.
        
        Parameters
        ----------
        ip_address : MIPAddress
            The address
        asynchronously : bool, optional
            Flag which enables/disables asynchronous start
        connect_to_services : bool, optional
            Flag which enables/disables the connection to the services
            
        Returns
        ---------
        bool
            True if the connections were succefully established
        """
        
        # if connect_to_services:
        #     self.service_access = MServiceAccess(ip_address)
            
        return self._connect_to_adapters(ip_address, asynchronously)
    
    def add_adapter(self, description, adapter):
        """
        Adds a new adapter to the access.
        
        Parameters
        ----------
        description : AdapterDescription
            The description
        adapter : MMIAdapter.Iface
            The adapter
        """
        
        assert (isinstance(description, MAdapterDescription)), "The given description is no AdapterDescription"
        assert (isinstance(adapter, MMIAdapter.Iface)), "The given adapter is no MMIAdapter.Iface"
        
        # Create a new local adapter
        new_adapter = utils.create_local_adapter_access(self, description, adapter)
        
        # Check if adapter is already registered (probably remote)
        old_adapter = next((a for a in self._adapters if (a.description.Name == description.Name 
                and a.description.Address == description.Address
                and a.description.Port == description.Port)), None)
        
        # adapter found
        if old_adapter is not None:
            
            #Find MMUs which utilize the old adapter and change it to the new one
            [mmu.change_adapter(new_adapter) for mmu in self.mmus if mmu.get_adapter() == old_adapter]
            
            # Dispose the unused old adapter
            old_adapter.dispose()
            
            # Remove the old adapter
            self._adapters.remove(old_adapter)
            
        self._adapters.append(new_adapter)

        
    def get_loadable_mmus(self, asynchronously = True):
        """
        Returns all loadable MMUs in form of their description
        
        Parameters
        ----------
        asynchronously : bool, optional
            Flag which enables/disables asynchronous start
        
        Returns
        ---------
        list<MMUDescription>
            The loadable MMUs
        """
        mmus = list()
        
        for access in self._adapters:
            mmus.extend(access.get_loadable_mmus())
        
        self._mmu_descriptions = mmus
        
        return mmus
    
    def _load_mmu_asynchronously(self, adapter, IDs, descriptions, queue):
        """
        Loads specified MMUs asynchronously and creates connections
        
        Parameters
        --------
        adapter : IAdapterAccess
            The adapter access
        IDs : list<string>
            The mmus IDs to load
        descriptions : list<MMUDescription>
            The mmus descriptions to load
        queue : Queue
            Queue to write the results asynchronously
        """
        
        adapter.load_mmus(IDs, self.__sessionID)
        queue.put(adapter.create_mmu_connections(descriptions, self.__sessionID))
        
    
    def _synchronize_scene_asynchronously(self, adapter, events, ID, queue):
        """
        Loads specified MMUs asynchronously and creates connections
        
        Parameters
        --------
        adapter : IAdapterAccess
            The adapter access
        events : list<MSceneManipulation>
            The scene changes to synchronize
        asynchronously : bool, optional
            Flag which enables/disables asynchronous processing
        queue : Queue
            Queue to write the results asynchronously
            
        Returns
        --------
        bool
        """
        
        
        queue.put(adapter.synchronize_scene(events, ID))
            
    
    def load_mmus(self, IDs, asynchronously = True):
        """
        Loads specified MMUs and creates connections
        
        Parameters
        --------
        IDs : list<string>
            The mmus IDs to load.
        asynchronously : bool, optional
            Flag which enables/disables asynchronous start
            
        
        Returns
        ---------
        list<MotionModelUnit>
            The list of mmu connections.
        """
        
        result = list()
        
        descriptions = [x for x in self._mmu_descriptions if x.Name in IDs]
        
        if asynchronously:
            
            # load mmus multithreaded
            threads = list()
            
            # Container for multithreaded return values
            result_queue = Queue()
            
            for adapter in self._adapters:
                process = Thread(target=self._load_mmu_asynchronously, args = (adapter, IDs, descriptions, result_queue))
                process.start()
                threads.append(process)
                
            # Wait for the finishing of all load methods
            for process in threads:
                process.join()
            
            # Collect all results from the threads
            while not result_queue.empty():
                result.extend(result_queue.get())
                    
        else:
            
            # Single threaded
            for adapter in self._adapters:
                adapter.load_mmus(IDs, self.__sessionID)
                result.extend(adapter.create_mmu_connections(descriptions, self.__sessionID))
                
        self.mmus = result
        
        return result
        
    def synchronize_scene(self, events, asynchronously = True):
        """
        Loads specified MMUs
        
        Parameters
        --------
        events : list<MSceneManipulation>
            The scene changes to synchronize
        asynchronously : bool, optional
            Flag which enables/disables asynchronous processing
            
        Returns
        --------
        bool
        """
        
        synchronized = True
        
        
        if asynchronously:
            
            # Start adapter multithreaded
            threads = list()
            
            # Container for multithreaded return values
            result_queue = Queue()
            
            result = list()
            
            for adapter in self._adapters:
                process = Thread(target=self._synchronize_scene_asynchronously, args = (adapter, events, self.__sessionID, result_queue))
                process.start()
                threads.append(process)
                
            # Wait for the finishing of all start methods
            for process in threads:
                process.join()
                
            # Collect all results from the threads
            while not result_queue.empty():
                result.append(result_queue.get())
                
            if not all(result):
                synchronized = False
                    
        else:
            for adapter in self._adapters:
                if not adapter.synchronize_scene(events, self.__sessionID):
                    synchronized = False
        
        return synchronized

    