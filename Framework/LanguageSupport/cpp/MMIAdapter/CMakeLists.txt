# CMakeList.txt : CMake project for MMIStandard_CMAKE, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

project ("MMIAdapter")
if(NOT "${CMAKE_GENERATOR}" MATCHES "(Win64|IA64)")
    set(platform "x64")
else()
	set(platform "x86")
endif()

if("${CMAKE_BUILD_TYPE}" MATCHES "Debug")
set(buildtype "${platform}-Debug")
set(vcFolder "${platform}-windows/debug")
set(thriftLibs thriftmdd thriftzmdd thriftnbmdd)
set(boostLibs boost_program_options-vc140-mt-gd)
else()
set(buildtype "${platform}-Release")
set(vcFolder "${platform}-windows")
set(thriftLibs thriftmd thriftzmd thriftnbmd)
set(boostLibs boost_program_options-vc140-mt)
endif()

set(Boost_USE_STATIC_LIBS ON)
find_package(BOOST REQUIRED COMPONENTS program-options)
find_package(Thrift REQUIRED)


find_path(CPPREST_INCLUDE_DIR "thrift/thrift.h")
include_directories(${CPPREST_INCLUDE_DIR})
# Add source to this project's executable.
FILE(GLOB srcT CppAdapter.cpp)
set(rootFolder ${CMAKE_CURRENT_SOURCE_DIR}/..)
link_directories(${rootFolder}/MMICPP/build/${buildtype}/ ${rootFolder}/MMIStandard/build/${buildtype}/ ${rootFolder}/vcpkg/installed/${vcFolder}/lib)
add_executable(MMIAdapter ${srcT})

target_include_directories(MMIAdapter PUBLIC
   $<BUILD_INTERFACE:${rootFolder}/MMIStandard>
    $<BUILD_INTERFACE:${rootFolder}/MMICPP>
)

set_property(TARGET MMIAdapter PROPERTY CXX_STANDARD 17)

target_link_libraries(MMIAdapter LINK_PRIVATE MMIStandard MMICPP ${boostLibs} ${thriftLibs})