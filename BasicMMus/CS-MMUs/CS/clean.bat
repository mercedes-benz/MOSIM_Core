@echo off
REM SPDX-License-Identifier: MIT
REM The content of this file has been developed in the context of the MOSIM research project.
REM Original author(s): Bhuvaneshwaran Ilanthirayan

@REM Checking environment variables
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

if EXIST build (
  RD /S/Q build
)

@REM Clean the Visual Studio Project
"%MSBUILD%" .\CS.sln -t:clean -flp:logfile=clean.log

if %ERRORLEVEL% EQU 0 (
    @REM If the cleaning is 92mSuccessful, delete all files from the respective build folders
    FOR /D %%G in (*) DO (
    if NOT "%%G"==".vs" (
      if EXIST "%%G"\bin (
        if EXIST "%%G"\bin\%mode% (
            if EXIST .\build (
                RMDIR /S/Q .\build\%%G
            )
        )
        @REM RMDIR /S/Q .\bin\%mode% @REM TODO: Ask Janis for removing the debug folder
      )
    )
  )

  ECHO [92mSuccessfully cleaned CS Language Support[0m
  exit /b 0
) else (
  ECHO [31mCleaning of CS Language Support failed. Please consider the build.log for more information. [0m
  exit /b 1
)

pause