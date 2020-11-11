#pragma once
#include<thrift/server/TThreadPoolServer.h>

using namespace apache::thrift::server;
using namespace std;

namespace MMIStandard {
	class ThriftServer
	{
		/**
			Basic class which represents the adapter server
		*/

	private:
		//the server itself
		TThreadPoolServer *server;

	public:
		//Destructor which stops the server
		~ThriftServer();

		//Starts the server 
		// <param name="port">The port at which the server schould listen</param>
		// <param name="entries">The number of working server threads</param>
		void Start(int port, int workerCount);
	};
}

