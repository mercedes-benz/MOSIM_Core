# Launcher

Repository containing the launcher implementation.
The launcher represents the central accessing point of the overall MMI framework.
To build the launcher requires the libraries MMICSharp and the auto-generated code-base MMIStandard (in future using Git Submodules).
After building: Please use the content contained in the debug/release folder to start the actual launcher.
Please ensure that the settings.json file is not available at the first startup. This configuration file stores several user settings including the path of the Launcher & environment.
At the first startup, the launcher automatically creates the respective folders for Adapters, MMUs and Services (if not already defined).
Please put the desired Services, MMUs and Adapters in the respective folders (e.g. Environment/Adapters/CSharpAdapter, Environment/MMUs/TestMMU, Environment/Services/PathPlanningService).
The services and adapters are automatically started if a MExecutionDescription file is provided. 
The launcher permanently listens to incoming registration requests. This means that also external application not being explicitly started by the launcher can register at the central register.