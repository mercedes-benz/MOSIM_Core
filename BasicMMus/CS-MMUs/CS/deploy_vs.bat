@echo off
REM SPDX-License-Identifier: MIT
REM The content of this file has been developed in the context of the MOSIM research project.
REM Original author(s): Janis Sprenger, Bhuvaneshwaran Ilanthirayan

REM the ESC sign can be created by pressing left alt + 027 on the num-pad. 

ECHO.
ECHO _______________________________________________________
ECHO [33mdeploy.bat[0m at %cd%\deploy.bat Deploying CS MMUs. 
ECHO _______________________________________________________
ECHO.


REM Checking environment variables
if not defined DEVENV (
  ECHO [31mDEVENV Environment variable pointing to the Visual Studio 2017 devenv.exe is missing.[0m
  ECHO    e.g. "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.com"
  pause
  exit /b 1
) else (
  if not exist "%DEVENV%" (
    ECHO    DEVENV: [31mMISSING[0m at "%DEVENV%"
    ECHO [31mPlease update the deploy_variables.bat script with a valid path![0m
	exit /b 2
  )
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
  echo Using Debug mode as default.
) else (
    if "%~1"=="Release" (
      SET "mode=Release"
    )
    else (
      echo Unkown parameter "%~1"
    )
  )
)

if EXIST build (
  RD /S/Q build
)
md build

if EXIST deploy.log DEL deploy.log 

>deploy.log (
	REM Build the Visual Studio Project
	"%MSBUILD%" .\CS.sln -t:Build -p:Configuration=%mode% -flp:logfile=build.log
)
if %ERRORLEVEL% EQU 0 (

	>>deploy.log (
	REM If the build was sucessfull, copy all files to the respective build folders. 
	  FOR /D %%G in (*) DO (
		if NOT "%%G"==".vs" (
		  if EXIST "%%G"\bin (
			if EXIST "%%G"\bin\%mode% (
			  md .\build\%%G
			  cmd /c xcopy /S/Y/Q .\%%G\bin\%mode%\* .\build\%%G
			)
		  )
		)
	  )
	)
  ECHO [92mSuccessfully deployed CS MMUs[0m
  exit /b 0
) else (
  type deploy.log
  ECHO [31mDeployment of CS MMUs failed. Please consider the %cs%/build.log for more information. [0m
  exit /b 1
)

exit /b 0