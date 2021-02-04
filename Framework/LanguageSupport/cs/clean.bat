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

SET mode=Debug

REM Clean the Visual Studio Project
"%DEVENV%" /Log clean.log .\MMICSharp.sln /Clean

if %ERRORLEVEL% EQU 0 (


    if EXIST .\MMIStandard\bin\%mode% (
        RMDIR /S/Q .\MMIStandard\bin\%mode%
    )

    if EXIST .\MMIAdapterCSharp\bin\%mode% (
        RMDIR /S/Q .\MMIAdapterCSharp\bin\%mode%
    )

    if EXIST .\MMICSharp\bin\%mode% (
        RMDIR /S/Q .\MMICSharp\bin\%mode%
    )

    if EXIST .\MMIStandard\build (
        RMDIR /S/Q .\MMIStandard\build
    )

    if EXIST .\MMICSharp\build (
        RMDIR /S/Q .\MMICSharp\build
    )

    if EXIST .\MMIAdapterCSharp\build (
        RMDIR /S/Q .\MMIAdapterCSharp\build
    )

    ECHO [92mSuccessfully cleaned CS Language Support [0m
    exit /b 0
) else (
    ECHO [31mCleaning of CS Language Support failed [0m
    exit /b 1
)

pause