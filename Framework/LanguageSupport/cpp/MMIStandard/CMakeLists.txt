# CMakeList.txt : CMake project for MMIStandard_CMAKE, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

project ("MMIStandard")
add_definitions(-DNOMAIN)
find_package(Thrift REQUIRED)
find_path(CPPREST_INCLUDE_DIR "thrift/thrift.h")
include_directories(${CPPREST_INCLUDE_DIR})
# Add source to this project's executable.
FILE(GLOB hT "../../thrift/gen-cpp/*.h")
FILE(GLOB srcT "../../thrift/gen-cpp/*.cpp")
# remove server skeleton files, as they do not compile
list(FILTER srcT EXCLUDE REGEX ".*server.skeleton.cpp$")
add_library(MMIStandard STATIC ${hT} ${srcT})
target_link_libraries(MMIStandard STATIC thrift thriftz thriftnb)



