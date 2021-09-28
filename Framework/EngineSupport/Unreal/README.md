# UnrealDemo

This Demo project is based on Unreal Engine version 4.24.3. In order to run the demo, please download or install the Unreal Engine, e.g. by using the Epic Games Installer. Unreal Engine 4 follows its own [license terms](https://www.unrealengine.com/en-US/faq).

Make sure, that you have the Edit Symbols for Debugging installed in Unreal Engine and the Game Development Tools in Visual Studio to enable Debugging. 

If you want to develop the Plugin, keep in mind that the HotReload of Unreal Engine does not always work properly. If you encounter any issues, close the Engine, then rebuild and reopen your project.

## Environment Setup
The `deploy.bat` script does not include all relevant components needed for the Unreal Engine setup. In addition, please do the following:
- compile `MOSIM\Core\CoSimulation\MMICoSimulation.sln`
- replace `MOSIM\Core\CoSimulation\CoSimulationStandalone\bin\Release\description.json` with `MOSIM\Core\CoSimulation\CoSimulationStandalone\description.json`
- copy the contents of`MOSIM\Core\CoSimulation\CoSimulationStandalone\bin\Release` into `MOSIM\build\Environment\Adapters\CoSimulationStandalone`

The file structure of the Environment should contain at least the following:
- Adapters
	- CSharpAdapter
	- CoSimulationStandalone
	- UnityAdapter
- Launcher
- MMUs
- Services
	- RetargetingService
	- StandaloneSkeletonAccess

## Plugin Structure

- `\Source\MMMIScene` contains the source code
- `\libs` contains the requiered precompiled libraries
	*Note: Manually update if MOSIM framework changes apply.*
- `\Content` contains the necessary assets

## Plugin Integration
There are two ways to integrate this plugin to your Unreal Engine simulation.

### Integrate as Engine Plugin
- Copy the content of this directory to the following path:
 	`<UE4_Path>\Engine\Plugins\UE4_MOSIM`
	where `<UE4_Path>` is your UE4 source directory.
- Execute ".\GenerateProjectFiles.bat" in your UE4 source directory.
- Build the Engine again.
*Note: You need Unreal Engine sources for that*

### Integrate as Level Plugin
- Copy the content of this directory to the following path:
	`<Your_Project>\Plugins\UE4_MOSIM`
	where `<Your_Project>` is your UE4 Project source directory.
- Right-click on < Your_Project >.uproject and select "Generate Visual Studio Project Files".
- Rebuild your project.

## Create Unreal Engine Level wit MOSIM Elements
The scene/map must contain the following MOSIM elements:
- MMIAvatar
	- Create new BlueprintClass with parent `MMIAvatar`:

	![AddNew->BlueprintClass](Images/01_AddNew_BlueprintClass.png?raw=true "Add new Blueprint Class") ![PickParentClass](Images/02_PickParentClass_MMIAvatar.png?raw=true? "Pick Parent Class")

	- Double-click on the Avatar object. In the BlueprintEditor, select your preferred skeletal within `Mesh->Skeletal Mesh` for your avatar:

	![PickSkeletalMesh](Images/03_BluePrintEditor_PickSkeletalMesh.png?raw=true "Pick Skeletal Mesh")
    This must be included within `<Your_Project>/Content` for the Engine to find it.

	- Add AjanAgent to your Avatar to enable Connection with AJAN:

	![AddAjanAgent](Images/05_Avatar_Add_AjanAgent.png?raw=true "Add Ajan Agent")

- Simulation Controller
	- In the `Modes`-Tab, type `SimulationController`in the search field, then drag and drop the object into your scene:

	![SimulationController](Images/04_Modes_DragAndDrop_MOSIM_Classes_in_Scene.png?raw=true "SimulationController")

    _Optional: Add AvatarBehavior, if you do not want to use AJAN_
- Scene Objects
	- Insert any objects into your scene. 
	- In order for the MOSIM framework to recognize them, add a `MMISceneObject`Component to your obejct.
	- In the details panel, select the corresponding Scene Object Type from the drop-down menu.
	
	![AddSceneObjects.png](Images/06_Add_Scene_Objects.png?raw=true "Add Scene Objects")

For detailed information on the usage of the MOSIM plugin, see the documentation/wiki.

## Setup Visual Studio to use Google C++ Coding Style
- download and install Clang (see [Clang 4.0.0](URL ""http://releases.llvm.org/4.0.0/LLVM-4.0.0-win64.exe)).
- in Tools > Extensions and Updates: on the left, choose "online" and search for and install CodeBeautifier (on the top right).
- restart Visual Studio
- in Manobit > CodeBeautifier > Options in text field: enter "clang-format" and press Add
- in clang-format > Application:
	- Type: StdInput
	- Application: (path to clang-format.exe, e.g. C:\Program Files\LLVM\bin\clang-format.exe)
	- Arguments: -style=file -fallback-style=none -assume-filename="$(FileName)"
- in clang-format > in Language: add extensions .cpp and .h
- in clang-format > in General: set "On document save" to true
- save and exit.
- the file "Source\.clang-format" defines the format used for Clang.
