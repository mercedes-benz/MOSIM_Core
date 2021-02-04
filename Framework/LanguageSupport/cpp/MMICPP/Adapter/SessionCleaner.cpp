// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#include "SessionCleaner.h"

using namespace MMIStandard;
using namespace std;

SessionCleaner::SessionCleaner(SessionData* SessionDataRef)
	:timeOut( 0 ),
	updateTime( 0 ),
	sessionManagerThread(nullptr),
	SessionDataRef(nullptr)
{
	this->SessionDataRef = SessionDataRef;
	this->sessionManagerThread = new thread(&SessionCleaner::manageSessions, this);	
}

SessionCleaner::~SessionCleaner()
{
	if (this->sessionManagerThread != nullptr)
	{
		this->sessionManagerThread->join();
		this->sessionManagerThread->~thread();
		delete this->sessionManagerThread;
	}
	this->sessionManagerThread = nullptr;
}


void SessionCleaner::start()
{
	if (this->sessionManagerThread != nullptr)
	{
		if (!this->sessionManagerThread->joinable())
		{
			this->sessionManagerThread = new thread(&SessionCleaner::manageSessions, this);
		}
	}
}

void SessionCleaner::dispose()
{
	if (this->sessionManagerThread != nullptr)
	{
		if (!this->sessionManagerThread->joinable())
		{
			this->sessionManagerThread->join();
			this->sessionManagerThread->~thread();
			delete this->sessionManagerThread;
		}
		this->sessionManagerThread = nullptr;
	}
}

void SessionCleaner::manageSessions()
{
	if (this->sessionManagerThread != nullptr)
	{
		//while (this->sessionManagerThread->joinable())
		//{
		//	//TODO: set thread to sleep 
		//	
		//	//check all sessions for timeout
		//	int *numSesCont = this->SessionDataRef->SessionContents->size;

		//	for (*int i = this->SessionDataRef->SessionContents->size - 1; i >= 0; i--)
		//	{
		//		SessionContent sessionContentVal = this->SessionDataRef->;

		//	}

		//}
	}
}


