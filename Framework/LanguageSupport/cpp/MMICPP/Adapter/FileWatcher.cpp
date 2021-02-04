// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#include "FileWatcher.h"
#include "SessionData.h"
#include "boost/exception/diagnostic_information.hpp"
#include "Utils\Logger.h"
#include<fstream>
#include<filesystem>

using namespace std;
using namespace std::filesystem;
using namespace MMIStandard;
using FileWatcher = MMIStandard::FileWatcher;

namespace MMIStandard {
	void FileWatcher::Start()
	{
		this->SearchLoadableMMUs(this->mmuPath);
	}

	FileWatcher::FileWatcher(const string & watchDir, const vector<string>& languages) :mmuPath{ watchDir }, languages{ languages }{
	}


	/// Returns the descriptions of all loadable MMUs
	void FileWatcher::SearchLoadableMMUs(const path & Path)
	{
		if (!std::filesystem::exists(Path))
		{
			Logger::printLog(L_ERROR, "Specified MMUpath does not exist");
			return;
		}
		int loaded = 0;
		std::unordered_map<std::string, directory_entry>entries;
		list<directory_entry> descriptions;

		for (directory_entry entry : recursive_directory_iterator(Path))
		{
			if (is_regular_file(entry))
			{
				entries[entry.path().filename().u8string()] = entry;
				std::string filename = entry.path().filename().u8string();
				std::string searchPattern = "description.json";

				if (filename.size() >= searchPattern.size() && filename.compare(filename.size() - searchPattern.size(), searchPattern.size(), searchPattern) == 0)
				{
					if (this->CheckDescriptions(entry, entries))
					{
						loaded++;
					}
				}
			}
		}
		Logger::printLog(L_INFO, "Scanned for loadable MMUS: " + std::to_string(loaded) + " loadable MMUs found");
	}

	//TODO find easier way for parsing JSON
	bool FileWatcher::CheckDescriptions(const directory_entry description, std::unordered_map<std::string, directory_entry> &entries)
	{
		std::ifstream stream{ description };
		if (!stream.is_open())
		{
			Logger::printLog(L_ERROR, "while opening file " + description.path().filename().u8string() + " an error is encountered");
			return false;
		}
		else
		{
			try {
				json j = json::parse(stream);
				MMUDescription mmuDesc{};

				mmuDesc.__set_Language(j["Language"].get<std::string>());
				if (std::find(this->languages.begin(), this->languages.end(), mmuDesc.Language) == this->languages.end())
					return false;

				mmuDesc.__set_Name(j["Name"].get<std::string>());
				mmuDesc.__set_ID(j["ID"].get<std::string>());
				mmuDesc.__set_AssemblyName(j["AssemblyName"].get<std::string>());
				mmuDesc.__set_MotionType(j["MotionType"].get<std::string>());
				mmuDesc.__set_Language(j["Language"].get<std::string>());
				mmuDesc.__set_Author(j["Author"].get<std::string>());
				mmuDesc.__set_Version(j["Version"].get<std::string>());

				map <std::string, bool> isset = j["__isset"].get<std::map<std::string, bool>>();
				if (isset["Prerequisites"])
					mmuDesc.__set_Prerequisites(j["Prerequisites"].get<const std::vector<MConstraint>>());
				if (isset["Properties"])
					mmuDesc.__set_Properties(j["Properties"].get<const std::map<std::string, std::string>>());
				if (isset["Dependencies"])
					mmuDesc.__set_Dependencies(j["Dependencies"].get<const std::vector<MDependency>>());
				if (isset["Events"])
					mmuDesc.__set_Events(j["Events"].get<std::vector<std::string>>());
				if (isset["LongDescription"])
					mmuDesc.__set_LongDescription(j["LongDescription"].get<std::string>());
				if (isset["ShortDescription"])
					mmuDesc.__set_ShortDescription(j["ShortDescription"].get<std::string>());
				if (isset["Parameters"])
					mmuDesc.__set_Parameters(j["Parameters"].get<const std::vector<MParameter>>());
				if (isset["SceneParameters"])
					//mmuDesc.__set_SceneParameters(j["SceneParameters"].get<const std::vector<MParameter>>());  //TODO: interferes with SearchAssemblyName, sadam

				return this->SearchAssemblyName(mmuDesc, entries);
			}
			catch (...)
			{
				Logger::printLog(L_ERROR, boost::current_exception_diagnostic_information());
				return false;
			}
		}
	}

	bool FileWatcher::SearchAssemblyName(const  MMUDescription & mmuDescription, std::unordered_map<std::string, directory_entry> &entries)
	{
		auto it = entries.find(mmuDescription.AssemblyName);
		if (it != entries.end())
		{
			directory_entry test = it->second;
			SessionData::mmuPaths[mmuDescription.ID] = test.path().u8string();
			SessionData::mmuDescriptions.emplace_back(mmuDescription);
			return true;
		}
		return false;
	}

	void from_json(const json & j, MConstraint & c)
	{
		j.at("ID").get_to(c.ID);
		//TODO: implement functionality, sadam
		//MGeometryConstraint GeometryConstraint;
		//MVelocityConstraint VelocityConstraint;
		//MAccelerationConstraint AccelerationConstraint;
		//MPathConstraint PathConstraint;
		//MJointPathConstraint JointPathConstraint;
		//MPostureConstraint PostureConstraint;
		//MJointConstraint JointConstraint;
		//std::map<std::string, std::string>  Properties;
	}

	void from_json(const json & j, MDependency & d)
	{
		//TODO: untested code, sadam
		j.at("ID").get_to(d.ID);
		j.at("Name").get_to(d.Name);
		j.at("MinVersion").get_to(d.MinVersion);
		j.at("MaxVersion").get_to(d.MaxVersion);
		//TODO: implement conversion, sadam
/*		MDependencyType::type Type;
		std::vector<MVersion>  ExcludedVersions;	*/
	}

	void from_json(const json & j, MDependencyType & d)
	{
		//TODO: implement conversion, sadam
	}

	void from_json(const json & j, MVersion & v)
	{
		j.at("Major").get_to(v.Major);
		j.at("Minor").get_to(v.Minor);
		j.at("Sub").get_to(v.Sub);
		j.at("Subsub").get_to(v.Subsub);
	}

	void from_json(const json &j, MParameter &p)
	{
		j.at("Name").get_to(p.Name);
		j.at("Type").get_to(p.Type);
		j.at("Description").get_to(p.Description);
		j.at("Required").get_to(p.Required);
	}
}