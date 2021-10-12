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

if not defined MSBUILD (
  ECHO [31mMSBUILD Environment variable pointing to the Visual Studio 2017 MSBuild.exe is missing.[0m
  ECHO    e.g. "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
  pause
  exit /b 1
) else (
  if not exist "%MSBUILD%" (
    ECHO    MSBUILD: [31mMISSING[0m at "%MSBUILD%"
    ECHO [31mPlease update the deploy_variables.bat script with a valid path![0m
	exit /b 2
    )
  )
)

SET mode=Debug

if "%~1"=="" (
  REM no parameter provided, assuming debug mode. 
  echo Using Debug as default.
) else (
    if "%~1"=="Release" (
      SET "mode=Release"
    )
    else (
      echo Unkown parameter "%~1"
    )
  )
)

@REM CLean the Visual Studio Project
"%MSBUILD%" .\MMILauncher.sln -t:clean -flp:logfile=clean.log

@REM If the clean is sucessfull, remove all the build folders
if %ERRORLEVEL% EQU 0 (
    if EXIST .\build (
        RMDIR /S/Q .\build
    )
    ECHO [92mSuccessfully cleaned MMILauncher[0m
    exit /b 0
) else (
      ECHO [31mCleaning of MMILauncher failed. Please consider the clean.log for more information. [0m
  exit /b 1
)

pause