ECHO.
ECHO _______________________________________________________
ECHO [33mdeploy.bat[0m at %cd%\deploy.bat Deploying the Standalone CoSimulator. 
ECHO _______________________________________________________
ECHO.

REM Checking environment variables
if not defined DEVENV (
  ECHO [31mDEVENV Environment variable pointing to the Visual Studio 2017 devenv.exe is missing.[0m
  ECHO    e.g. "SET DEVENV=C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.com"
  pause
  exit /b 1
) else (
  if not exist "%DEVENV%" (
    ECHO Visual Studio does not seem to be installed at "%DEVENV%" or path name in deploy_variables.bat is wrong.
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


"%MSBUILD%" .\MMICoSimulation.sln -t:Build -p:Configuration=%mode% -flp:logfile=build.log

REM If the build was sucessfull, copy all files to the respective build folders. 
if %ERRORLEVEL% EQU 0 (
  IF NOT EXIST .\CoSimulationStandalone\build\ (
    mkdir .\CoSimulationStandalone\build 
  ) ELSE (
    RMDIR /S/Q .\CoSimulationStandalone\build
    mkdir .\CoSimulationStandalone\build
  )
  
  REM cmd /c has to be called to prevent xcopy to destroy any coloring of outputs
  cmd /c xcopy /S/Y/Q .\CoSimulationStandalone\bin\%mode%\* .\CoSimulationStandalone\build
  
  ECHO [92mSuccessfully deployed CoSimulation Standalone to %cd%\CoSimulationStandalone\build [0m
  exit /b 0
) else (
  ECHO [31mDeployment of CoSimulation Standalone failed. Please consider the build.log for more information.[0m
  exit /b 1
)


exit /b 0