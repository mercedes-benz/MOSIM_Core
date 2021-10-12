@echo off
REM SPDX-License-Identifier: MIT
REM The content of this file has been developed in the context of the MOSIM research project.
REM Original author(s): Janis Sprenger, Bhuvaneshwaran Ilanthirayan

REM the ESC sign can be created by pressing left alt + 027 on the num-pad. 

ECHO.
ECHO _______________________________________________________
ECHO [33mdeploy_unity.bat[0m at %cd%\deploy_unity.bat Deploying the Unity engine support. 
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

REM Build the MMIUnity programm. 
"%MSBUILD%" .\MMIUnity.sln -t:Build -p:Configuration=Debug -flp:logfile=build.log

REM If the build was sucessfull, copy all files to the respective build folders. 
if %ERRORLEVEL% EQU 0 (
  
  REM Call the Visual Studio Deploy file to deploy 
  IF NOT EXIST .\MMIUnity\build (
    mkdir .\MMIUnity\build 
  ) ELSE (
    RMDIR /S/Q .\MMIUnity\build
    mkdir .\MMIUnity\build
  )
  REM cmd /c has to be called to prevent xcopy to destroy any coloring of outputs
  cmd /c xcopy /S/Y/Q .\MMIUnity\bin\Debug\*.dll .\MMIUnity\build /EXCLUDE:deploy_exclude_UnityEngine.txt
  
  IF NOT EXIST .\MMIUnity.TargetEngine\MMIUnity.TargetEngine\build (
    mkdir .\MMIUnity.TargetEngine\MMIUnity.TargetEngine\build 
  ) ELSE (
    RMDIR /S/Q .\MMIUnity.TargetEngine\MMIUnity.TargetEngine\build
    mkdir .\MMIUnity.TargetEngine\MMIUnity.TargetEngine\build
  )
  REM cmd /c has to be called to prevent xcopy to destroy any coloring of outputs
  cmd /c xcopy /S/Y/Q .\MMIUnity.TargetEngine\MMIUnity.TargetEngine\bin\Debug\*.dll .\MMIUnity.TargetEngine\MMIUnity.TargetEngine\build /EXCLUDE:deploy_exclude_UnityEngine.txt
  
  
  IF NOT EXIST .\MMIUnity.TargetEngine\MMIUnity.TargetEngine.Editor\build (
    mkdir .\MMIUnity.TargetEngine\MMIUnity.TargetEngine.Editor\build 
  ) ELSE (
    RMDIR /S/Q .\MMIUnity.TargetEngine\MMIUnity.TargetEngine.Editor\build
    mkdir .\MMIUnity.TargetEngine\MMIUnity.TargetEngine.Editor\build
  )
  REM cmd /c has to be called to prevent xcopy to destroy any coloring of outputs
  cmd /c xcopy /S/Y/Q .\MMIUnity.TargetEngine\MMIUnity.TargetEngine.Editor\bin\Debug\*.dll .\MMIUnity.TargetEngine\MMIUnity.TargetEngine.Editor\build /EXCLUDE:deploy_exclude_UnityEngine.txt
  
  REM Copy MMIAdapter first to UnityProject
  cmd /c xcopy /S/Y/Q .\MMIAdapterUnity\bin\Debug\*.dll MMIAdapterUnity\UnityProject\Assets\Plugins /EXCLUDE:deploy_exclude_UnityEngine.txt


  cd .\MMIAdapterUnity\UnityProject
  call .\deploy_unity.bat
  
  cd ..\..\

  
  if %ERRORLEVEL% EQU 0 (  
    ECHO [92mSuccessfully deployed Unity Support [0m
    exit /b 0
  ) else (
    ECHO [31mDeployment of Unity Support failed. Please investigate the %cs%MMIAdapterUnity/UnityProject for more information. [0m
    exit /b 1
  )
) else (
  ECHO [31mDeployment of Unity Support failed. Please consider the %cs%/build.log for more information. [0m
  exit /b 1
)

exit /b 0