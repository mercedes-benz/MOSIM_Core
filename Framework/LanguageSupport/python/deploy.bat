@echo off
REM SPDX-License-Identifier: MIT
REM The content of this file has been developed in the context of the MOSIM research project.
REM Original author(s): Janis Sprenger

ECHO.
ECHO _______________________________________________________
ECHO [33mdeploy.bat[0m at %cd%\deploy.bat Deploying the MOSIM support from python. 
ECHO _______________________________________________________
ECHO.



SET MOSIMPIP=pip
if "%~1"=="" (
  ECHO No parameters were provided, standard pip environment "%MOSIMPIP%" is utilized
) else (
  ECHO Pip environment was provided as: "%~1"
  SET MOSIMPIP=%~1
)

if EXIST .\MMIStandard\src (
  rd /S/Q .\MMIStandard\src
)
if NOT EXIST .\MMIStandard\src (
  md MMIStandard\src\MMIStandard
)

REM Copy auto-generated files to MMIStandard
cmd /c xcopy /S/Y/Q/E ..\thrift\gen-py\MMIStandard .\MMIStandard\src\MMIStandard

call %MOSIMPIP% install .\MMIStandard
call %MOSIMPIP% install .\MMIPython


if %ERRORLEVEL% EQU 0 (
  REM COPY .\configurations\avatar.mos build\
  ECHO [92mSuccessfully deployed the python support [0m
  exit /b 0
) else (
  ECHO [31mDeployment of the python support failed. [0m
  exit /b 1
)