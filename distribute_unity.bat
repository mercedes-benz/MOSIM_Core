@echo off
REM SPDX-License-Identifier: MIT
REM The content of this file has been developed in the context of the MOSIM research project.
REM Original author(s): Janis Sprenger

IF EXIST "C:\Program Files\Unity Environments\2018.4.1f1\Editor\Unity.exe" (
ECHO Lost path
)

if not defined UNITY2018_4_1 (
   IF EXIST "C:\Program Files\Unity Environments\2018.4.1f1\Editor\Unity.exe" (
    SET UNITY2018_4_1=C:\Program Files\Unity Environments\2018.4.1f1\Editor\Unity.exe
	ECHO Autodetecting UNITY2018_4_1 is now defined as: "%UNITY2018_4_1%"
   ) ELSE (
	IF EXIST "C:\Program Files\Unity\Hub\Editor\2018.4.1f1\Editor\Unity.exe" (
    SET UNITY2018_4_1=C:\Program Files\Unity\Hub\Editor\2018.4.1f1\Editor\Unity.exe
	ECHO Autodetecting UNITY2018_4_1 is now defined as: "%UNITY2018_4_1%"
   ) ELSE (
	ECHO [31mUNITY2018_4_1 Environment variable pointing to the Unity.exe for Unity version 2018.4.1f1 is missing.[0m
	ECHO e.g. SET "UNITY2018_4_1=C:\Program Files\Unity Environments\2018.4.1f1\Editor\Unity.exe\"
	pause
	exit /b 1
    )
   )
) else (
  ECHO UNITY2018_4_1 defined as: "%UNITY2018_4_1%"
)

if not defined UNITY2019_18_1 (
	IF EXIST "C:\Program Files\Unity Environments\2019.4.18f1\Editor\Unity.exe" (
    SET UNITY2019_18_1=C:\Program Files\Unity Environments\2019.4.18f1\Editor\Unity.exe
	ECHO Autodetecting UNITY2019_18_1 is now defined as: "%UNITY2019_18_1%"
   ) ELSE (
	IF EXIST "C:\Program Files\Unity\Hub\Editor\2019.4.18f1\Editor\Unity.exe" (
    SET UNITY2019_18_1=C:\Program Files\Unity\Hub\Editor\2019.4.18f1\Editor\Unity.exe
	ECHO Autodetecting UNITY2019_18_1 is now defined as: "%UNITY2019_18_1%"
   ) ELSE (
	ECHO [31mUNITY2019_18_1 Environment variable pointing to the Unity.exe for Unity version 2018.4.1f1 is missing.[0m
	ECHO e.g. SET "UNITY2019_18_1=C:\Program Files\Unity Environments\2019.4.18f1\Editor\Unity.exe\"
	pause
	exit /b 1
    )
   )
) else (
  ECHO UNITY2019_18_1 defined as: "%UNITY2019_18_1%"
)

REM D:\Program Files\Unity Environments\2018.4.1f1\Editor\Data\Managed\UnityEngine.dll
set UnityDLL2018=%UNITY2018_4_1:Unity.exe=Data\Managed\UnityEngine.dll%
set UnityEditorDLL2018=%UNITY2018_4_1:Unity.exe=Data\Managed\UnityEditor.dll%

REM D:\Program Files\Unity Environments\2019.4.18f1\Editor\Data\Managed\UnityEngine.dll
set UnityDLL2019=%UNITY2019_18_1:Unity.exe=Data\Managed\UnityEngine.dll%
set UnityEditorDLL2019=%UNITY2019_18_1:Unity.exe=Data\Managed\UnityEditor.dll%

ECHO "%UnityEditorDLL2018%"

XCOPY /y "%UnityDLL2019%" .\BasicMMUs\CS-MMUs\CS\UnityIdleMMU
XCOPY /y "%UnityDLL2019%" .\BasicMMUs\CS-MMUs\CS\UnityLocomotionMMU
XCOPY /y "%UnityDLL2019%" .\Framework\EngineSupport\Unity\MMIAdapterUnity
XCOPY /y "%UnityEditorDLL2019%" .\Framework\EngineSupport\Unity\MMIUnity.TargetEngine\MMIUnity.TargetEngine.Editor
XCOPY /y "%UnityDLL2019%" .\Framework\EngineSupport\Unity\MMIUnity.TargetEngine\MMIUnity.TargetEngine.Editor
XCOPY /y "%UnityDLL2019%" .\Framework\EngineSupport\Unity\MMIUnity.TargetEngine\MMIUnity.TargetEngine
XCOPY /y "%UnityDLL2019%" .\Framework\EngineSupport\Unity\MMIUnity


ECHO [92mSuccessfully copied all UnityEngine.dlls and UnityEditor.dlls from your local system to the framework. [0m


