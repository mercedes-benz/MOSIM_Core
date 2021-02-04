// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include <thread>
#include <chrono>
#include "SessionData.h"

using namespace MMIStandard;
using namespace std;

namespace MMIStandard {
	// cleans the session based on a timeout 
	class SessionCleaner
	{
		// member fields
	public:
		int timeOut;
		int updateTime;

	private:

		// the utilized thread
		thread* sessionManagerThread;

		// assigned session data to monitor
		SessionData* SessionDataRef;

		//int* numSesCont;

		// methods
	public:
		// constructor
		SessionCleaner(SessionData* SessionDataRef);

		// destructor
		~SessionCleaner();

		// start the thread
		void start();

		// dispose the thread 
		void dispose();

	private:
		// manage and clean the sessions 
		void manageSessions();
	};
}
