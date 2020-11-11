## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald, Janis Sprenger


from MMIPython.core.utils.thrift_client import ThriftClient
from MMIStandard.services import MSkeletonAccess

def initialize(register_ip_address) -> MSkeletonAccess:
	# Get the service descriptions from the mmu register
	client = ThriftClient(register_ip_address.Address, register_ip_address.Port, MSkeletonAccess.Client) 
	client.__enter__() ## Todo: this appears dirty and we should probably clean this up in the future. 
	return client._access
