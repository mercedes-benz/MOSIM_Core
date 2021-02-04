@echo off
REM SPDX-License-Identifier: MIT
REM The content of this file has been developed in the context of the MOSIM research project.
REM Original author(s): Bhuvaneshwaran Ilanthirayan

if EXIST .\MMIStandard\src (
  rd /S/Q .\MMIStandard\src
)
if %ERRORLEVEL% EQU 0 (
  REM COPY .\configurations\avatar.mos build\
  ECHO [92mSuccessfully cleaned the python support [0m
  exit /b 0
) else (
  ECHO [31mCleaning of the python support failed. [0m
  exit /b 1
)