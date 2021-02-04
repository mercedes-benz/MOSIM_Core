# Cpp
To build the code Visual Studio 2017 or newer is required. CMAKE as a stand alone software is also required

To automatically build the repo run deploy.bat

Repository containing the Adapter, MMI and MMU implementation for c++\
Packages\
in core root where you see MMIStandard and MMICPP run\
git clone https://github.com/Microsoft/vcpkg.git\
cd vcpkg\
\
(bellow switch \ with / for linux terminal or gitbash)\
.\bootstrap-vcpkg.bat\
.\vcpkg install thrift:x64-windows\
\
The auto generated code of the thrift files of the MMI Standard are included in ../thrift/gen-cpp.




