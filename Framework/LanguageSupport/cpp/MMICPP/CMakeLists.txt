# CMakeList.txt : CMake project for MMIStandard_CMAKE, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

project ("MMICPP")
if(NOT "${CMAKE_GENERATOR}" MATCHES "(Win64|IA64)")
    set(platform "x64")
else()
	set(platform "x86")
endif()

if("${CMAKE_BUILD_TYPE}" MATCHES "Debug")
set(buildtype "${platform}-Debug")
else()
set(buildtype "${platform}-Release")
endif()
#message(${buildtype})

find_path(CPPREST_INCLUDE_DIR "thrift/thrift.h")
include_directories(${CPPREST_INCLUDE_DIR})

FILE(GLOB Extensions Extensions/*.cpp ThriftClient/*.cpp ThriftServer/*.cpp Utils/*.cpp Adapter/*.cpp Access/*.cpp)
link_directories(${CMAKE_CURRENT_SOURCE_DIR}/../MMIStandard/build/${buildtype}/)
add_library (MMICPP ${Adapter} ${Access} ${Extensions})

target_link_libraries(MMICPP PUBLIC STATIC MMIStandard)

target_include_directories(MMICPP PUBLIC
  $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}>
   $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/../MMIStandard>
)

set_property(TARGET MMICPP PROPERTY CXX_STANDARD 17)
