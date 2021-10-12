REM SPDX-License-Identifier: MIT
REM The content of this file has been developed in the context of the MOSIM research project.
REM Original author(s): Janis Sprenger, Bhuvaneshwaran Ilanthirayan


ECHO.
ECHO _______________________________________________________
ECHO [33mdeploy_vs.bat[0m at %cd%\deploy_vs.bat Deploying the CS language support. 
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

REM restoring nuget
"%MSBUILD%" -t:restore -flp:logfile=restore.log

REM Build the Visual Studio Project
"%MSBUILD%" .\MMICSharp.sln -t:Build -p:Configuration=%mode% -flp:logfile=build.log

REM If the build was sucessfull, copy all files to the respective build folders. 
if %ERRORLEVEL% EQU 0 (
  IF NOT EXIST .\MMIStandard\build (
    mkdir .\MMIStandard\build 
  ) ELSE (
    RMDIR /S/Q .\MMIStandard\build
    mkdir .\MMIStandard\build
  )
  REM cmd /c has to be called to prevent xcopy to destroy any coloring of outputs
  cmd /c xcopy /S/Y/Q .\MMIStandard\bin\%mode%\*.dll .\MMIStandard\build
  
  IF NOT EXIST .\MMICSharp\build (
    mkdir .\MMICSharp\build
  ) ELSE (
    RMDIR /S/Q .\MMICSharp\build
    mkdir .\MMICSharp\build
  )
  cmd /c xcopy /S/Y/Q .\MMICSharp\bin\%mode%\*.dll .\MMICSharp\build
  
  IF NOT EXIST .\MMIAdapterCSharp\build (
    mkdir .\MMIAdapterCSharp\build
  ) ELSE (
    RMDIR /S/Q .\MMIAdapterCSharp\build
    mkdir .\MMIAdapterCSharp\build
  )
  cmd /c xcopy /S/Y/Q .\MMIAdapterCSharp\bin\%mode% .\MMIAdapterCSharp\build
  
  ECHO [92mSuccessfully deployed CS Language Support [0m
  exit /b 0
) else (
  ECHO [31mDeployment of CS Language Support failed. Please consider the build.log for more information.[0m
  exit /b 1
)

exit /b 0