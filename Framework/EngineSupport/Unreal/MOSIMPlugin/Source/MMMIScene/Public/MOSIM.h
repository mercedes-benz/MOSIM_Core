// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Caroline Handel

#pragma once

#include "CoreMinimal.h"

#include <string>

// declare custom log categories. Define in IMMIScene.cpp via "DEFINE_LOG_CATEGORY(...)"
DECLARE_LOG_CATEGORY_EXTERN( LogMOSIM, Log, All );

struct MOSIMException : public exception
{
    string message;

    MOSIMException( string msg ) : message( msg )
    {
    }
    const char* what() const throw()
    {
        return message.c_str();
    }
};