// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include "gen-cpp/MSceneAccess.h"
#include "gen-cpp/scene_types.h"
#include <unordered_map>

using namespace MMIStandard;
using namespace std;
namespace MMIStandard {

	class MMIScene : public MSceneAccessIf
	{
		/*
			Class represents a (hyptothetical) scene which can be specifically set up by the developer
		*/
	private:
		//	Map contains all scene objects structured by the specific id
		unordered_map<string, MSceneObject> sceneObjectsById;

		//	Map containing all avatars structured by the specific id
		unordered_map<string, MAvatar> avatarsById;

		//	Mapping between the name of a scene object and a unique id
		unordered_map<string, vector<string>> nameIdMappingSceneObjects;

		//	Mapping between the name of a avatar and a unique id
		unordered_map<string, vector<string>> nameIdMappingAvatars;

		//	MSceneUpdate from the previous frame
		MSceneUpdate sceneUpdate;

		//	ID of the frame
		int frameID;

		//	The size of the HistoryBuffer
		int historyBufferSize;

		//	A list  which contains the history of the last n applied scene manipulations
		list<pair<int, MSceneUpdate>>sceneHistory;

	private:
		//	Removes all scene objects from the scene
		//	<param name="sceneObjectIDs">The IDs of the scene objects which schould be removed</param>
		void RemoveSceneObjects(MBoolResponse & _return, const vector<string>& sceneObjectIDs);

		//	Clears the whole scene
		void Clear();

		//	Adds all avatars to the scene
		//	<param name="avatars">The avatars which should be added</param>
		void AddAvatars(MBoolResponse & _return, const vector<MAvatar>& avatars);

		//	Adds all scene objects to the scene
		//	<param name="sceneObjects>The scene objects which should be added</param>
		void AddSceneObjects(MBoolResponse & _return, const vector<MSceneObject>& sceneObjects);

		//	Updates all avatars
		//	<param name="avatars>The avatars which should be updated</param>
		void UpdataAvatars(MBoolResponse & _return, const vector<MAvatarUpdate>& avatars);

		//	Updates all scene objects
		//	<param name="avatars>The scene objects which should be updated</param>
		void UpdateSceneObjects(MBoolResponse & _return, const vector<MSceneObjectUpdate>& sceneObjects);

		//	Removes all scene objects from the scene
		//	<param name="avatarIDs">The IDs of the avatars which schould be removed</param>
		void RemoveAvatars(MBoolResponse & _return, const vector<string>& avatarIDs);


	public:
		//	Basic Constructor
		MMIScene();

		//Applies the scene manipulation on the scene
		// <param name="sceneUpdates">The scene manipulations to be considered</param>
		void Apply(MBoolResponse &_return, const MSceneUpdate &scene);


		//	Inherited via MSceneAccessIf

		//	Returns the scene objects
		virtual void GetSceneObjects(std::vector<MSceneObject>& _return) override;
		shared_ptr <vector< MSceneObject>> GetSceneObjects();

		//	Returns the scene object based on the id
		virtual void GetSceneObjectByID(MSceneObject & _return, const std::string & id) override;
		shared_ptr<MSceneObject>GetSceneObjectByID(const string &id);

		//	Returns the scene object based on the name
		virtual void GetSceneObjectByName(MSceneObject & _return, const std::string & name) override;
		shared_ptr<MSceneObject>GetSceneObjectByName(const string &name);

		//	Returns the scene object based on range
		virtual void GetSceneObjectsInRange(std::vector<MSceneObject>& _return, const MVector3 & position, const double range) override;
		shared_ptr<vector<MSceneObject>> GetSceneObjectsInRange(const MVector3 & position, const double range);

		//	Returns the collider of the scene objects
		virtual void GetColliders(std::vector<MCollider>& _return) override;
		shared_ptr<vector<MCollider>> GetColliders();

		//	Returns the collider of the scene object based on the id
		virtual void GetColliderById(MCollider & _return, const std::string & id) override;
		shared_ptr<MCollider> GetColliderById(const string &id);

		//	Returns the collider of the scene objects based on the range
		virtual void GetCollidersInRange(std::vector<MCollider>& _return, const::MMIStandard::MVector3 & position, const double range) override;
		shared_ptr <vector<MCollider>> GetCollidersInRange(const::MMIStandard::MVector3 & position, const double range);

		//	Returns the meshes of the scene objects
		virtual void GetMeshes(std::vector<MMesh>& _return) override;
		shared_ptr<vector<MMesh>> GetMeshes();

		//	Returns the meshes of the scene object based on the id
		virtual void GetMeshByID(MMesh & _return, const std::string & id) override;
		shared_ptr<MMesh>GetMeshByID(const std::string & id);

		//	Returns the transforms of the scene objects
		virtual void GetTransforms(std::vector<MTransform>& _return) override;
		shared_ptr<vector<MTransform>> GetTransforms();

		//	Returns the meshes of the scene object based on the id
		virtual void GetTransformByID(MTransform & _return, const std::string & id) override;
		shared_ptr<MTransform> GetTransformByID(string &id);

		//	Returns the avatars
		virtual void GetAvatars(std::vector<MAvatar>& _return) override;
		shared_ptr<vector<MAvatar>> GetAvatars();

		//	Returns the avatar based on the id
		virtual void GetAvatarByID(MAvatar & _return, const std::string & id) override;
		shared_ptr<MAvatar> GetAvatarByID(const string &id);

		//	Returns the avatar based on the name
		virtual void GetAvatarByName(MAvatar & _return, const std::string & name) override;
		shared_ptr<MAvatar> GetAvatarByName(const string &name);

		//	Returns the avatar based on the range
		virtual void GetAvatarsInRange(std::vector<MAvatar>& _return, const::MMIStandard::MVector3 & position, const double distance) override;
		shared_ptr<vector<MAvatar>> GetAvatarsInRange(const::MMIStandard::MVector3 & position, const double distance);


		//virtual double GetSimulationTime() override;
		double GetSimulationTime();   // deleted keyword "virtual", sadam

		// Returns the changes from the privious frame
		virtual void GetSceneChanges(MSceneUpdate & _return) override;
		shared_ptr<MSceneUpdate> GetSceneChanges();

		// Returns all sceneobjects and avatars as MSceneUpdate
		virtual void GetFullScene(MSceneUpdate & _return) override;
		shared_ptr<MSceneUpdate> GetFullScene();


		virtual void GetNavigationMesh(MNavigationMesh & _return) override;
		shared_ptr<MNavigationMesh> GetNavigationMesh();

		// new functions in MSceneAccessIf, sadam
		void GetData(std::string& _return, const std::string& fileFormat, const std::string& selection);
		
		void GetAttachments(std::vector< ::MMIStandard::MAttachment> & _return);
		
		void GetAttachmentsByID(std::vector< ::MMIStandard::MAttachment> & _return, const std::string& id);

		void GetAttachmentsByName(std::vector< ::MMIStandard::MAttachment> & _return, const std::string& name);

		void GetAttachmentsChildrenRecursive(std::vector< ::MMIStandard::MAttachment> & _return, const std::string& id);

		void GetAttachmentsParentsRecursive(std::vector< ::MMIStandard::MAttachment> & _return, const std::string& id);

		// inherited virtual functions from MMIServiceBaseIf, sadam
		void GetStatus(std::map<std::string, std::string> & _return);
		
		void GetDescription(::MMIStandard::MServiceDescription& _return);
		
		void Setup(::MMIStandard::MBoolResponse& _return, const  ::MMIStandard::MAvatarDescription& avatar, const std::map<std::string, std::string> & properties);
		
		void Consume(std::map<std::string, std::string> & _return, const std::map<std::string, std::string> & properties);
		
		void Dispose(::MMIStandard::MBoolResponse& _return, const std::map<std::string, std::string> & properties);
		
		void Restart(::MMIStandard::MBoolResponse& _return, const std::map<std::string, std::string> & properties);
	};
}