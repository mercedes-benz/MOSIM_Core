## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald


# -*- coding: utf-8 -*-
"""

"""

from MMIStandard.core.ttypes import MParameter, MServiceDescription
from MMIStandard.mmu.ttypes import MMUDescription
from MMIStandard.constraints.ttypes import MConstraint
from MMIStandard.register.ttypes import MAdapterDescription

#from MMIStandard.ttypes import ServiceDescription, AdapterDescription, MMUDescription

def create_service_description(json_dict):
    """
    Creates a MMIStandard ServiceDescription from a dictionary
    
    Parameters
    -----------
    json_dict : dict
        The dictionary with the parameters
        
    Returns
    ---------
    ServiceDescription
    """
    return ServiceDescription(Name=json_dict['Name'],
                              ID=json_dict['ID'],
                              ExecutableName=json_dict['ExecutableName'],
                              Address=json_dict['Address'],
                              Port=json_dict['Port'],
                              Properties=json_dict['Properties'])
    
def create_adapter_description(json_dict):
    """
    Creates a MMIStandard ServiceDescription from a dictionary
    
    Parameters
    -----------
    json_dict : dict
        The dictionary with the parameters
        
    Returns
    ---------
    ServiceDescription
    """
    assert (isinstance(json_dict, dict)),"json_dict is no dict"
    assert (all(isinstance(x, str) for x in json_dict.keys())), "Not all keys are of type string in mmu_description_json"
    
    return AdapterDescription(Name=json_dict['Name'],
                              ID=json_dict['ID'],
                              ExecutableName=json_dict['ExecutableName'],
                              Language=json_dict['Language'],
                              Address=json_dict['Address'],
                              Port=json_dict['Port'],
                              Properties=json_dict['Properties'])
    
def create_mmu_description(mmu_description_json):
    """
    Returns the description of a MMU based on a json text
    
    Parameters
    ----------
    mmu_description_json : dict
        The json text of the description
    
    Returns
    ----------
    MMUDescription
        The converted MMU description
    """
    assert (isinstance(mmu_description_json, dict)),"mmu_description_json is no dict"
    assert (all(isinstance(x, str) for x in mmu_description_json.keys())), "Not all keys are of type string in mmu_description_json"
    
    
    
    mmu_description = MMUDescription()
    #description_variables = [i for i in vars(mmu_description).keys() if i not in vars(object).keys()]

    def json2struct(struct, struct_json):
        description_variables = [i for i in vars(struct).keys() if i not in vars(object).keys()]
    
        for variable in description_variables:
            if variable in struct_json:
                setattr(struct,variable,struct_json[variable])

    def jsonlist2struct(struct_type, json_list):
        struct_list = []
        if json_list == None:
            return None
        for i in range(len(json_list)):
            struct = struct_type()
            json2struct(struct, json_list[i])
            struct_list.append(struct)
        return struct_list
    
    json2struct(mmu_description, mmu_description_json)

    mmu_description.Parameters = jsonlist2struct(MParameter, mmu_description.Parameters)
    mmu_description.SceneParameters = jsonlist2struct(MParameter, mmu_description.SceneParameters)
    mmu_description.Prerequisites = jsonlist2struct(MConstraint, mmu_description.Prerequisites)


    return mmu_description