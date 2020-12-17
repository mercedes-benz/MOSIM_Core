echo Generating csharp source code...."
for %%f in (mmi\*.thrift) do thrift.exe -gen csharp %%f

echo Generating c++ source code...."
for %%f in (mmi\*.thrift) do thrift.exe -gen cpp %%f

echo Generating java source code...."
for %%f in (mmi\*.thrift) do thrift.exe -gen java %%f

echo Generating pythonsource code...."
for %%f in (mmi\*.thrift) do thrift.exe -gen py %%f

echo Generating pythonsource code...."
for %%f in (mmi\*.thrift) do thrift.exe -gen php %%f