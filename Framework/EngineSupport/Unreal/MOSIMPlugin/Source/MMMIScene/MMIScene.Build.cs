// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

namespace UnrealBuildTool.Rules
{
	public class MMIScene : ModuleRules
	{
        public MMIScene(ReadOnlyTargetRules Target) : base(Target)
		{

            // Not sure if needed
            PublicDefinitions.Add("_CRT_SECURE_NO_WARNINGS=1");
            PublicDefinitions.Add("BOOST_DISABLE_ABI_HEADERS=1");

            // Needed configurations in order to run Boost
            bUseRTTI = true;
            bEnableExceptions = true;
            bEnableUndefinedIdentifierWarnings = false;

            PublicIncludePaths.AddRange(
                new string[] {
				}
				);

			PrivateIncludePaths.AddRange(
                new string[] {
				}
				);
                        
			PublicDependencyModuleNames.AddRange(
				new string[]
				{
					"Core",
					"CoreUObject",
					"Engine",
					"InputCore"
				}
				);
            // Http, Json for MMIAvatar, Ajan and Task List Editor
			PublicDependencyModuleNames.AddRange(new string[] { "Json", "JsonUtilities", "Http" });
			
			// Customized Details Panels
			PublicDependencyModuleNames.AddRange(new string[] { "PropertyEditor", "Slate", "SlateCore" });
			
            PrivateDependencyModuleNames.AddRange(
				new string[]
				{				}
				);

			DynamicallyLoadedModuleNames.AddRange(
				new string[]
				{
				}
				);

            // add third party libraries
            if (Target.Platform == UnrealTargetPlatform.Win64)
            {                
                PublicIncludePaths.Add(System.IO.Path.Combine(ModuleDirectory,"../../libs/includes"));
                PublicAdditionalLibraries.Add(System.IO.Path.Combine(ModuleDirectory, "../../libs/MMIStandard.lib"));
                PublicAdditionalLibraries.Add(System.IO.Path.Combine(ModuleDirectory, "../../libs/MMICPP.lib"));
                PublicAdditionalLibraries.Add(System.IO.Path.Combine(ModuleDirectory, "../../libs/thriftmd.lib"));
                PublicAdditionalLibraries.Add(System.IO.Path.Combine(ModuleDirectory, "../../libs/thriftnbmd.lib"));
                PublicAdditionalLibraries.Add(System.IO.Path.Combine(ModuleDirectory, "../../libs/thriftzmd.lib"));
            }
        }
    }
}
