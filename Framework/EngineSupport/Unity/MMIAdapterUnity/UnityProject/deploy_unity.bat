@echo off
REM SPDX-License-Identifier: MIT
REM The content of this file has been developed in the context of the MOSIM research project.
REM Original author(s): Janis Sprenger, Bhuvaneshwaran Ilanthirayan

if not defined UNITY2018_4_1 (
  ECHO [31mUNITY2018_4_1 Environment variable pointing to the Unity.exe for Unity version 2018.4.1f1 is missing.[0m
  ECHO    e.g. SET "UNITY2018_4_1=C:\Program Files\Unity Environments\2018.4.1f1\Editor\Unity.exe\"
  pause
  exit /b 1
) else (
  ECHO UNITY2018_4_1 defined as: "%UNITY2018_4_1%"
)

IF EXIST build (
  RD /S/Q build
)

REM Build Unity Project:

@REM call "%UNITY2018_4_1%" -batchmode -quit -logFile build.log -projectPath . -buildWindowsPlayer "build/UnityAdapter.exe"
call "%UNITY2018_4_1%" -quit -batchmode -logFile build.log -projectPath "." -executeMethod BuildUnityAdapter.CreateServerBuild 

if %ERRORLEVEL% EQU 0 (
  COPY .\configurations\avatar.mos build\
  COPY ..\description.json build\
  ECHO [92mSuccessfully deployed Unity Adapter[0m
  exit /b 0
) else (
  ECHO [31mDeployment of Unity Adapter failed. Please consider the build.log for more information. [0m
  exit /b 1
)