@echo off
REM the ESC sign can be created by pressing left alt + 027 on the num-pad. 

REM set the environment variable to the path of devenv
REM TODO: check installed version of visual studio

REM Path to Devenv
REM $Env:DEVENV = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.com"


REM Checking environment variables
SET vsPATH = ""
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2021\Professional\Common7\IDE\devenv.com" (
	ECHO "Found VS 2017"
	SET vsPATH="C:\Program Files (x86)\Microsoft Visual Studio\2021\Professional\Common7\IDE\devenv.com"
) else (
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\IDE\devenv.com" (
	ECHO "Found VS 2019"
	SET vsPATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\IDE\devenv.com"
) else (
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.com" (
	ECHO "Found VS 2021"
	SET vsPATH="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.com"
) else (
if defined DEVENV (
	SET vsPATH="%DEVENV%"
) else (
  ECHO [No visual studio install was found, you can set DEVENV Environment variable pointing to the Visual Studio 2017 devenv.exe.
  ECHO    e.g. "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.com"
))))


REM install vcpkg and the required libraries
if not exist "vcpkg" (
	echo Cloning and installing vcpkg
	git clone https://github.com/Microsoft/vcpkg.git
	cd vcpkg
	call .\bootstrap-vcpkg.bat
	echo Installing required libraries	
	.\vcpkg install thrift:x64-windows
	.\vcpkg install boost-program-options:x64-windows
	cd ..
) else (
	cd vcpkg
	if not exist "vcpkg.exe" (
		echo Vcpkg Repo already cloned. Installing vcpkg and required libraries.
		call .\bootstrap-vcpkg.bat
		.\vcpkg install thrift:x64-windows
		.\vcpkg install boost-program-options:x64-windows
	) else (
		echo Using installed vcpkg version to install required libraries.
		.\vcpkg install thrift:x64-windows
		.\vcpkg install boost-program-options:x64-windows
	cd ..
	)
)
	

REM MMIStandard\
echo Building MMIStandard
cd MMIStandard\
if not exist build (
	mkdir build
	cd build
) else (
    RMDIR /S/Q build
	mkdir build
	cd build
	echo MMIStandard\build already existed, replaced old dir for building.	
)
REM generate VS solution
cmake -DCMAKE_TOOLCHAIN_FILE="../../vcpkg/scripts/buildsystems/vcpkg.cmake" -G "Visual Studio 15 2017 Win64" ..
REM build VS solution
%VSPath% .\MMIStandard.sln /Build "Debug|x64"
%VSPath% .\MMIStandard.sln /Build "Release|x64"
REM copy the files to the correct path
mkdir x64-Debug\
mkdir x64-Release\
cmd /c xcopy /S .\Debug\*.lib .\x64-Debug\
cmd /c xcopy /S .\Debug\*.pdb .\x64-Debug\
cmd /c xcopy /S .\Release\*.lib .\x64-Release\
REM return to cpp dir
cd ..\..\


REM MMICPP\
echo Building MMICPP
cd MMICPP\
if not exist build (
	mkdir build
	cd build
) else (
    RMDIR /S/Q build
	mkdir build
	cd build
	echo MMICPP\build already existed, replaced old dir for building.	
)
REM generate VS solution
cmake -DCMAKE_TOOLCHAIN_FILE="../../vcpkg/scripts/buildsystems/vcpkg.cmake" -G "Visual Studio 15 2017 Win64" ..
REM build VS solution
%VSPath% .\MMICPP.sln /Build "Debug|x64"
%VSPath% .\MMICPP.sln /Build "Release|x64"
REM copy the files to the correct path
mkdir x64-Debug\
mkdir x64-Release\
cmd /c xcopy /S .\Debug\*.lib .\x64-Debug\
cmd /c xcopy /S .\Debug\*.pdb .\x64-Debug\
cmd /c xcopy /S .\Release\*.lib .\x64-Release\
REM return to cpp dir
cd ..\..\


REM Adapter\
echo Building MMIAdapter
cd MMIAdapter\
if not exist build (
	mkdir build
	cd build
) else (
    RMDIR /S/Q build
	mkdir build
	cd build
	echo MMIAdapter\build already existed, replaced old dir for building.	
)
REM generate VS solution for the Debug Build
cmake -DCMAKE_TOOLCHAIN_FILE="../../vcpkg/scripts/buildsystems/vcpkg.cmake" -DCMAKE_BUILD_TYPE=Debug -G "Visual Studio 15 2017 Win64" ..
%VSPath% .\MMIAdapter.sln /Build "Debug|x64"
REM generate VS solution for the Release Build
cmake -DCMAKE_TOOLCHAIN_FILE="../../vcpkg/scripts/buildsystems/vcpkg.cmake" -DCMAKE_BUILD_TYPE=Release -G "Visual Studio 15 2017 Win64" ..
%VSPath% .\MMIAdapter.sln /Build "Release|x64"
REM copy the files to the correct path
mkdir x64-Debug\
mkdir x64-Release\
cmd /c xcopy /S .\Debug\*.exe .\x64-Debug\
cmd /c xcopy /S .\Debug\*.pdb .\x64-Debug\
cmd /c xcopy /S .\Debug\*.ilk .\x64-Debug\
cmd /c xcopy /S .\Debug\*.dll .\x64-Debug\
cmd /c xcopy /S .\Release\*.exe .\x64-Release\
cmd /c xcopy /S .\Release\*.dll .\x64-Release\
REM return to cpp dir
cd ..\..\

pause