// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

// file contains specializers for the ThriftClient template


#pragma once
#include "ThriftClient.h"
#include "gen-cpp/MMIRegisterService.h"
#include "gen-cpp/MMIAdapter.h"

namespace MMIStandard
{
	template<> 
	class ThriftClient<MMIRegisterServiceClient>
	{
	public:

		//	The access of the client
		shared_ptr<MMIRegisterServiceClient> access;

	private:

		//	The port to connect
		int port;

		// The IP address to connect
		std::string address;

		//	The transport of the client
		shared_ptr<TTransport> transport;

	public:

		//	Basic constructor
		ThriftClient(std::string const &address, int port, bool autoOpen = true);

		//	Basic destructor closes the connection
		~ThriftClient();

		//	Starts the client and opens the connection
		void Start();
	};

	template<>
	class ThriftClient<MMIAdapterClient>
	{
	public:

		//	The access of the client
		shared_ptr<MMIAdapterClient> access;

	private:

		//	The port to connect
		int port;

		// The IP address to connect
		std::string address;

		//	The transport of the client
		shared_ptr<TTransport> transport;

	public:

		//	Basic constructor
		ThriftClient(std::string const &address, int port, bool autoOpen = true);

		//	Basic destructor closes the connection
		~ThriftClient();

		//	Starts the client and opens the connection
		void Start();
	};

}
