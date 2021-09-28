// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#ifndef _THRIFT_CLIENT_CPP_
#define _THRIFT_CLIENT_CPP_

#include "ThriftClient.h"
#include <thrift/protocol/TCompactProtocol.h>
#include <thrift/transport/TSocket.h>
#include <thrift/transport/TTransportUtils.h>
#include "Utils/Logger.h"
//#include "boost/exception/diagnostic_information.hpp"

using namespace std;
using namespace apache::thrift::protocol;
using namespace MMIStandard;

template<class T>
 ThriftClient<T>::ThriftClient(std::string const & address, int port, bool autoOpen) :port(port), address(address)
{
	 shared_ptr<TTransport> socket(new TSocket(this->address, this->port));
	 this->transport = shared_ptr<TTransport>(new TBufferedTransport(socket));
	 shared_ptr<TProtocol> protocol(new TCompactProtocol{ transport });
	 this->access = shared_ptr<T>(new T{ protocol });

	 if (autoOpen)
		 this->Start();
}

template<class T>
ThriftClient<T>::~ThriftClient()
{
	this->transport->close();
}

template<class T>
void ThriftClient<T>::Start()
{
	try {
		if(!this->transport->isOpen())
			this->transport->open();
	}
	catch (exception e)
	{
		//Logger::printLog(L_ERROR, boost::current_exception_diagnostic_information());
		Logger::printLog(L_ERROR, e.what());	// TODO: define error message
	}
	
}
#endif