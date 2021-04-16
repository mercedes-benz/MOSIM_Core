// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

/*
	Template class which represents a thrift client
*/

#ifndef _THRIFT_CLIENT_H_
#define _THRIFT_CLIENT_H_

#pragma once
#include <thrift/transport/TTransportUtils.h>

using namespace apache::thrift;
using namespace apache::thrift::transport;
using namespace std;

namespace MMIStandard {
	template <class T>
	class ThriftClient
	{
	public:

		//	The access of the client
		shared_ptr<T> access;

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
#endif
