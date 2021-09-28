// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

#include "IMMIScene.h"

// TODO: remove following lines, probably unrequired
// required, otherwise error message during compilation, no idea why
//#undef UpdateResource

#define LOCTEXT_NAMESPACE "IMMIScene"

DEFINE_LOG_CATEGORY( LogMOSIM );

void IMMIScene::StartupModule()
{
    // This code will execute after your module is loaded into memory; the exact timing is specified
    // in the .uplugin file per-module
}

void IMMIScene::ShutdownModule()
{
    // This function may be called during shutdown to clean up your module.  For modules that
    // support dynamic reloading,
    // we call this function before unloading the module.
}

#undef LOCTEXT_NAMESPACE

IMPLEMENT_MODULE( IMMIScene, MMIScene )
