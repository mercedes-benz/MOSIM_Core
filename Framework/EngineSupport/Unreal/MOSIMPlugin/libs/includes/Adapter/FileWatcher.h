// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include<filesystem>
#include<unordered_map>
#include "gen-cpp/mmu_types.h"
#include <string>
#include <nlohmann/json.hpp>


using namespace std;
using namespace std::filesystem;
using json = nlohmann::json;
using namespace MMIStandard;


namespace MMIStandard {
	class FileWatcher
	{
		/**
			class which:
				checks the given path for the descriptions
				parses the descriptions
				checks the discriptions if it contains a supported language
				checks the given path for the assembly name and saves the description and the assembly path in SessionData
		*/

	private:
		//	path which should be checked
		path mmuPath;

		//	The supported languages
		vector<string>languages;

	private:
		//	checks thte given path for the descriptions
		void SearchLoadableMMUs(const path & Path = current_path());

		//	parses the description, checks for supported languages
		//	<param name="description">The directory_entry for the description.json</param>
		//	<param name="entries">A map of all file entries in the search path, key=filename </param>
		bool CheckDescriptions(const directory_entry description, std::unordered_map<std::string, directory_entry>& entries);

		//	checks if entries contains the AssemblyName from the description
		//	<param name="description">The directory_entry for the description.json</param>
		//	<param name="entries">A map of all file entries in the search path, key=filename </param>
		bool SearchAssemblyName(const MMUDescription & mmuDescription, std::unordered_map<std::string, directory_entry>& entries);

	public:
		//	Basic constructor
		//	<param name="watchDir">The patch wich schould be checked for MMus</param>
		//	<param name="languages">The supported languages</param>
		FileWatcher(const string &watchDir, const vector <string> &languages);

		//	Starts the check for loadable MMUs
		void Start();
	};

	// functions for parsing json into the MMIStandard datatypes 
	void from_json(const json& j, MConstraint& c);
	void from_json(const json& j, MDependency& d);
	void from_json(const json& j, MDependencyType& d);
	void from_json(const json& j, MVersion& v);
	void from_json(const json &j, MParameter &p);
}


