@echo off
REM SPDX-License-Identifier: MIT
REM The content of this file has been developed in the context of the MOSIM research project.
REM Original author(s): Janis Sprenger
DEL "BasicMMUs\CS-MMUs\CS\UnityIdleMMU\UnityEngine.dll"
DEL "BasicMMUs\CS-MMUs\CS\UnityLocomotionMMU\UnityEngine.dll"
DEL "Framework\EngineSupport\Unity\MMIAdapterUnity\UnityEngine.dll"
DEL "Framework\EngineSupport\Unity\MMIUnity.TargetEngine\MMIUnity.TargetEngine.Editor\UnityEditor.dll"
DEL "Framework\EngineSupport\Unity\MMIUnity.TargetEngine\MMIUnity.TargetEngine.Editor\UnityEngine.dll"
DEL "Framework\EngineSupport\Unity\MMIUnity.TargetEngine\MMIUnity.TargetEngine\UnityEngine.dll"
DEL "Framework\EngineSupport\Unity\MMIUnity\UnityEngine.dll"

ECHO [92mSuccessfully removed all UnityEngine.dlls and UnityEditor.dlls from the framework. [0m


