@echo off
REM SPDX-License-Identifier: MIT
REM The content of this file has been developed in the context of the MOSIM research project.
REM Original author(s): Janis Sprenger, Bhuvaneshwaran Ilanthirayan

REM the ESC sign can be created by pressing left alt + 027 on the num-pad. 

REM Checking environment variables

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
