@echo off
REM SPDX-License-Identifier: MIT
REM The content of this file has been developed in the context of the MOSIM research project.
REM Original author(s): Bhuvaneshwaran Ilanthirayan

REM Checking environment variables
if not defined DEVENV (
  ECHO [31mDEVENV Environment variable pointing to the Visual Studio 2017 devenv.exe is missing.[0m
  ECHO    e.g. "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.com"
  pause
  exit /b 1
) else (
  ECHO DEVENV defined as: "%DEVENV%"
)

REM Clean the MMIUnity program.
"%DEVENV%" /Log clean.log .\MMIUnity.sln /Clean

REM If the clean is successfull, check for directories and remove it.
if %ERRORLEVEL% EQU 0 (
IF EXIST .\MMIUnity\build (
    RMDIR /S/Q .\MMIUnity\build
)

IF EXIST .\MMIUnity.TargetEngine\MMIUnity.TargetEngine\build (
    RMDIR /S/Q .\MMIUnity.TargetEngine\MMIUnity.TargetEngine\build
)

IF EXIST .\MMIUnity.TargetEngine\MMIUnity.TargetEngine.Editor\bin\Debug\ (
    RMDIR /S/Q .\MMIUnity.TargetEngine\MMIUnity.TargetEngine.Editor\bin\Debug\
)

cd .\MMIAdapterUnity\UnityProject
call .\clean.bat

if %ERRORLEVEL% EQU 0 (
    ECHO [92mSuccessfully cleaned Unity Support [0m
    exit /b 0
) else (
    ECHO [31Cleaning of Unity Support failed. Please consider the clean.log for more information. [0m
    exit /b 1
  )
) else (
  ECHO [31mCleaning of Unity Support failed. Please consider the clean.log for more information. [0m
  exit /b 1
)

exit /b 0