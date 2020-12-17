REM @echo off
REM the ESC sign can be created by pressing left alt + 027 on the num-pad. 

REM Checking environment variables
if not defined DEVENV (
  ECHO [31mDEVENV Environment variable pointing to the Visual Studio 2017 devenv.exe is missing.[0m
  ECHO    e.g. "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.com"
  pause
  exit /b 1
) else (
  ECHO DEVENV defined as: "%DEVENV%"
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


REM Build the Visual Studio Project
"%DEVENV%" /Log build.log .\MMILauncher.sln /Build %mode%

REM If the build was sucessfull, copy all files to the respective build folders. 
if %ERRORLEVEL% EQU 0 (
  IF NOT EXIST .\build (
    mkdir .\build 
  ) ELSE (
    RMDIR /S/Q .\build
    mkdir .\build
  )
  REM cmd /c has to be called to prevent xcopy to destroy any coloring of outputs
  cmd /c xcopy /S/Y/Q .\MMILauncher\bin\%mode%\* .\build
  
  ECHO [92mSuccessfully deployed MMILauncher[0m
  exit /b 0
) else (
  ECHO [31mDeployment of MMILauncher failed. Please consider the build.log for more information. [0m
  exit /b 1
)

pause