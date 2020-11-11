#include "MMIScene.h"
#include "Extensions/MVector3Extensions.h"
#include "Extensions/MQuaternionExtensions.h"
#include "boost/exception/diagnostic_information.hpp"
#include "Utils/Logger.h"
#include <iostream>
#include "Extensions/MBoolResponseExtensions.h"

MMIScene::MMIScene():frameID{0},historyBufferSize{20}
{
}

void MMIScene::GetSceneObjects(std::vector<MSceneObject>& _return)
{
	for(const pair <string, MSceneObject> &ob: this->sceneObjectsById)
	{
		_return.emplace_back(ob.second);
	}
}

shared_ptr<vector<MSceneObject>> MMIScene::GetSceneObjects()
{
	auto _return =make_shared<vector< MSceneObject>>();
	this->GetSceneObjects(*_return);
	return _return;
}

void MMIScene::GetSceneObjectByID(MSceneObject & _return, const std::string & id)
{
	auto iter = this->sceneObjectsById.find(id);
	if (iter != sceneObjectsById.end())
		_return = iter->second;
}

shared_ptr<MSceneObject> MMIScene::GetSceneObjectByID(const string & id)
{
	auto _return = make_shared<MSceneObject>();
	this->GetSceneObjectByID(*_return, id);
	return _return;
}

void MMIScene::GetSceneObjectByName(MSceneObject & _return, const std::string & name)
{
	auto iter = nameIdMappingSceneObjects.find(name);
	if (iter != nameIdMappingSceneObjects.end())
		this->GetSceneObjectByID(_return,iter->second[0]);
}

shared_ptr<MSceneObject> MMIScene::GetSceneObjectByName(const string & name)
{
	auto _return = make_shared<MSceneObject>();
	this->GetSceneObjectByName(*_return, name);
	return _return;
}

void MMIScene::GetSceneObjectsInRange(std::vector<MSceneObject>& _return, const::MMIStandard::MVector3 & position, const double range)
{
	vector<MSceneObject> objects;
	this->GetSceneObjects(objects);
	for (const MSceneObject &ob : objects)
	{
		if (MVector3Extensions::EuclideanDistance(ob.Transform.Position, position) <= range)
		{
			_return.emplace_back(ob);
		}	
	}
}

shared_ptr<vector<MSceneObject>> MMIScene::GetSceneObjectsInRange(const MVector3 & position, const double range)
{
	auto _return = make_shared <vector<MSceneObject>>();
	this->GetSceneObjectsInRange(*_return, position, range);
	return _return;
}

void MMIScene::GetColliders(std::vector<MCollider>& _return)
{
	vector<MSceneObject> sceneObjects;
	this->GetSceneObjects(sceneObjects);

	for (const MSceneObject &ob : sceneObjects)
	{
		_return.emplace_back(ob.Collider);
	}
}

shared_ptr<vector<MCollider>> MMIScene::GetColliders()
{
	auto _return = make_shared<vector<MCollider>>();
	this->GetColliders(*_return);
	return _return;
}

void MMIScene::GetColliderById(MCollider & _return, const std::string & id)
{
	MSceneObject sceneObject;
	this->GetSceneObjectByID(sceneObject, id);
	_return = sceneObject.Collider;
}

shared_ptr<MCollider> MMIScene::GetColliderById(const string & id)
{
	auto _return = make_shared<MCollider>();
	this->GetColliderById(*_return,id);
	return _return;
}

void MMIScene::GetCollidersInRange(std::vector<MCollider>& _return, const::MMIStandard::MVector3 & position, const double range)
{
	vector<MSceneObject> sceneObjects;
	this->GetSceneObjectsInRange(sceneObjects, position, range);

	for (const MSceneObject &ob : sceneObjects)
	{
		if (ob.__isset.Collider)
		{
			_return.emplace_back(ob.Collider);
		}
	}
}

shared_ptr<vector<MCollider>> MMIScene::GetCollidersInRange(const::MMIStandard::MVector3 & position, const double range)
{
	auto _return = make_shared<vector<MCollider>>();
	this->GetCollidersInRange(*_return,position, range);
	return _return;
}

void MMIScene::GetMeshes(std::vector<MMesh>& _return)
{
	vector<MSceneObject> sceneObjects;
	this->GetSceneObjects(sceneObjects);
	for (const MSceneObject &sceneObject: sceneObjects)
		_return.emplace_back(sceneObject.Mesh);
}

shared_ptr<vector<MMesh>> MMIScene::GetMeshes()
{
	auto _return = make_shared<vector<MMesh>>();
	this->GetMeshes(*_return);
	return _return;
}

void MMIScene::GetMeshByID(MMesh & _return, const std::string & id)
{
	MSceneObject sceneObject;
	this->GetSceneObjectByID(sceneObject, id);
	_return = sceneObject.Mesh;
}

shared_ptr<MMesh> MMIScene::GetMeshByID(const std::string & id)
{
	auto _return = make_shared<MMesh>();
	this->GetMeshByID(*_return, id);
	return _return;
}

void MMIScene::GetTransforms(std::vector<MTransform>& _return)
{
	vector<MSceneObject> sceneObjects;
	this->GetSceneObjects(sceneObjects);
	for (const MSceneObject &sceneObject : sceneObjects)
		_return.emplace_back(sceneObject.Transform);
}

shared_ptr<vector<MTransform>> MMIScene::GetTransforms()
{
	auto _return = make_shared<vector<MTransform>>();
	this->GetTransforms(*_return);
	return _return;
}

void MMIScene::GetTransformByID(MTransform & _return, const std::string & id)
{
	MSceneObject sceneObject;
	this->GetSceneObjectByID(sceneObject, id);
	_return = sceneObject.Transform;
}

shared_ptr<MTransform> MMIScene::GetTransformByID(string & id)
{
	auto _return = make_shared<MTransform>();
	this->GetTransformByID(*_return,id);
	return _return;
}

void MMIScene::GetAvatars(std::vector<MAvatar>& _return)
{
	for(const auto &ob : this->avatarsById)
	{
		_return.emplace_back(ob.second);
	}
}

shared_ptr<vector<MAvatar>> MMIScene::GetAvatars()
{
	auto _return =make_shared<vector<MAvatar>>();
	this->GetAvatars(*_return);
	return _return;
}

void MMIScene::GetAvatarByID(MAvatar & _return, const std::string & id)
{
	auto iter = this->avatarsById.find(id);
	if (iter != avatarsById.end())
		_return = iter->second;
}

shared_ptr<MAvatar> MMIScene::GetAvatarByID(const string & id)
{
	auto _return = make_shared<MAvatar>();
	this->GetAvatarByID(*_return, id);
	return _return;
}

void MMIScene::GetAvatarByName(MAvatar & _return, const std::string & name)
{
	auto iter = nameIdMappingAvatars.find(name);
	if (iter != nameIdMappingAvatars.end())
		this->GetAvatarByID(_return, iter->second[0]);
}

shared_ptr<MAvatar> MMIScene::GetAvatarByName(const string & name)
{
	auto _return = make_shared<MAvatar>();
	this->GetAvatarByName(*_return, name);
	return _return;
}

void MMIScene::GetAvatarsInRange(std::vector<MAvatar>& _return, const::MMIStandard::MVector3 & position, const double distance)
{
	vector<MAvatar> avatars;
	this->GetAvatars(avatars);

	for (MAvatar avatar : avatars)
	{
		MVector3 avatarPosition = MVector3{};
		avatarPosition.__set_X(avatar.PostureValues.PostureData[0]);
		avatarPosition.__set_Y(avatar.PostureValues.PostureData[1]);
		avatarPosition.__set_Z(avatar.PostureValues.PostureData[3]);

		if (MVector3Extensions::EuclideanDistance(avatarPosition, position) <= distance)
		{
			_return.emplace_back(avatar);
		}
	}
}

shared_ptr<vector<MAvatar>> MMIScene::GetAvatarsInRange(const::MMIStandard::MVector3 & position, const double distance)
{
	auto _return = make_shared<vector<MAvatar>>();
	this->GetAvatarsInRange(*_return, position, distance);
	return _return;
}

double MMIScene::GetSimulationTime()
{
	throw std::runtime_error("Function not implemented yet");
}

void MMIScene::GetSceneChanges(MSceneUpdate & _return)
{
	_return = this->sceneUpdate;
}

shared_ptr<MSceneUpdate> MMIScene::GetSceneChanges()
{
	return make_shared<MSceneUpdate>(this->sceneUpdate);
}


//TODO check
void MMIScene::GetFullScene(MSceneUpdate & _return)
{

	this->GetSceneObjects(_return.AddedSceneObjects);
	this->GetAvatars(_return.AddedAvatars);
}

shared_ptr<MSceneUpdate> MMIScene::GetFullScene()
{
	auto _return = make_shared<MSceneUpdate>();
	this->GetFullScene(*_return);
	return _return;
}

void MMIScene::GetNavigationMesh(MNavigationMesh & _return)
{
	throw std::runtime_error("Function not implemented yet");
}

shared_ptr<MNavigationMesh> MMIScene::GetNavigationMesh()
{
	throw std::runtime_error("Function not implemented yet");
}

void MMIScene::Clear()
{
	this->avatarsById.clear();
	this->sceneObjectsById.clear();
	this->nameIdMappingAvatars.clear();
	this->nameIdMappingSceneObjects.clear();
	this->sceneUpdate.~MSceneUpdate();
	this->sceneUpdate =  MSceneUpdate{};
	this->frameID = 0;
	this->sceneHistory.clear();
}

void MMIScene::Apply(MBoolResponse & _return, const MSceneUpdate & sceneUpdate)
{
	_return.__set_Successful(true);
	this->frameID++;
	this->sceneHistory.emplace_front(pair(frameID, sceneUpdate));
	while (this->sceneHistory.size() > (size_t)this->historyBufferSize)
		sceneHistory.pop_back();

	this->sceneUpdate = sceneUpdate;

	if(sceneUpdate.__isset.AddedAvatars)
		this->AddAvatars(_return,sceneUpdate.AddedAvatars);
	
	if (sceneUpdate.__isset.AddedSceneObjects)
		this->AddSceneObjects(_return, sceneUpdate.AddedSceneObjects);

	if (sceneUpdate.__isset.ChangedAvatars)
		this->UpdataAvatars(_return,sceneUpdate.ChangedAvatars);

	if (sceneUpdate.__isset.ChangedSceneObjects)
		this->UpdateSceneObjects(_return,sceneUpdate.ChangedSceneObjects);

	if (sceneUpdate.__isset.RemovedAvatars)
		this->RemoveAvatars(_return,sceneUpdate.RemovedAvatars);

	if (sceneUpdate.__isset.RemovedSceneObjects)
		this->RemoveSceneObjects(_return,sceneUpdate.RemovedSceneObjects);
}


void MMIScene::AddAvatars(MBoolResponse & _return, const vector<MAvatar>& avatars)
{
	
	for (const MAvatar &avatar : avatars)
	{
		if(!this->avatarsById.insert(std::pair<string, MAvatar>(avatar.ID, avatar)).second) // try to insert new avatar return is false if avatar is already in the map
		{
			string message = "Could not add avatar: " + avatar.Name + " is already registered";
			Logger::printLog(L_ERROR, message);
			MBoolResponseExtensions::Update(_return, message, false);
			continue;
		}

		auto iter1 = this->nameIdMappingAvatars.find(avatar.Name);
		if (iter1 != nameIdMappingAvatars.end()) // contains already the avatar
		{
			
			if (!(std::find(iter1->second.begin(), iter1->second.end(), avatar.ID) != iter1->second.end()))
				this->nameIdMappingAvatars[avatar.Name].emplace_back(avatar.ID);
		}
		else
		{
			nameIdMappingAvatars.insert(std::pair < string, vector<string>>(avatar.Name, vector<string>{avatar.ID}));
		}
	}
}

void MMIScene::AddSceneObjects(MBoolResponse & _return, const vector<MSceneObject>& sceneObjects)
{
	for (const MSceneObject &sceneObject: sceneObjects)
	{
		if (!this->sceneObjectsById.insert(std::pair<string, MSceneObject>(sceneObject.ID, sceneObject)).second)
		{
			string message = "Could not add scene object: " + sceneObject.Name + " is already registered";
			Logger::printLog(L_ERROR, message);
			MBoolResponseExtensions::Update(_return, message, false);
			continue;
		}

		auto iter1 = this->nameIdMappingSceneObjects.find(sceneObject.Name);
		if (iter1 != nameIdMappingSceneObjects.end())
		{
			if (!(std::find(iter1->second.begin(), iter1->second.end(), sceneObject.ID) != iter1->second.end()))
				this->nameIdMappingSceneObjects[sceneObject.Name].emplace_back( sceneObject.ID );
		}
		else
		{
			nameIdMappingSceneObjects.insert(std::pair < string, vector<string>>(sceneObject.Name, vector<string>{sceneObject.ID}));
		}
	}
}

void MMIScene::UpdataAvatars(MBoolResponse & _return, const vector<MAvatarUpdate>& avatars)
{
	for (MAvatarUpdate avatarUpdate : avatars)
	{
		auto iter = this->avatarsById.find(avatarUpdate.ID);
		if (iter != avatarsById.end())
		{
			if (avatarUpdate.__isset.Description)
				iter->second.Description = avatarUpdate.Description;

			if (avatarUpdate.__isset.PostureValues)
				iter->second.PostureValues = avatarUpdate.PostureValues;

			if (avatarUpdate.__isset.SceneObjects)
				iter->second.SceneObjects = avatarUpdate.SceneObjects;
		}
		else
		{
			string message = "Could not update avatar : " + avatarUpdate.ID + " not found";
			Logger::printLog(L_ERROR, message);
			MBoolResponseExtensions::Update(_return, message, false);
		}
	}
}

void MMIScene::UpdateSceneObjects(MBoolResponse & _return, const vector<MSceneObjectUpdate>& sceneObjects)
{
	for (MSceneObjectUpdate sceneObjectUpdate : sceneObjects)
	{
		auto iter = this->sceneObjectsById.find(sceneObjectUpdate.ID);
		if (iter != sceneObjectsById.end())
		{
			if (sceneObjectUpdate.__isset.Transform)
			{
				MTransformUpdate transformUpdate = sceneObjectUpdate.Transform;
				try
				{
					if(transformUpdate.__isset.Position)
						MVector3Extensions::ToMVector3(iter->second.Transform.Position, sceneObjectUpdate.Transform.Position);
					
					if(transformUpdate.__isset.Rotation)
						MQuaternionExtensions::ToMQuaternion(iter->second.Transform.Rotation, sceneObjectUpdate.Transform.Rotation);
				}
				catch (...)
				{
					std::clog << boost::current_exception_diagnostic_information() << std::endl;
					_return.__set_Successful(false);
					if (_return.__isset.LogData)
					{
						_return.LogData.emplace_back(boost::current_exception_diagnostic_information());
					}
					else
					{
						_return.__set_LogData(vector<string>{ boost::current_exception_diagnostic_information() });
					}
				}
				if (transformUpdate.__isset.Parent)
					iter->second.Transform.Parent = sceneObjectUpdate.Transform.Parent;
			}

			if (sceneObjectUpdate.__isset.Collider)
				iter->second.Collider=sceneObjectUpdate.Collider;

			if (sceneObjectUpdate.__isset.Mesh)
				iter->second.Mesh = sceneObjectUpdate.Mesh;

			if (sceneObjectUpdate.__isset.PhysicsProperties)
				iter->second.PhysicsProperties = sceneObjectUpdate.PhysicsProperties;
		}
		else
		{
			string message = "Could not update scene object: " + sceneObjectUpdate.ID + " object not found";
			Logger::printLog(L_ERROR, message);
			MBoolResponseExtensions::Update(_return, message, false);
		}
	}
}

void MMIScene::RemoveAvatars(MBoolResponse & _return, const vector<string>& avatarIDs)
{
	MBoolResponse response{};
	response.Successful = true;

	for (string id : avatarIDs)
	{
		auto avatarIter = this->avatarsById.find(id);
		if (avatarIter != avatarsById.end()) //pos avatar in avatarsById
		{
			auto nameIdIter = this->nameIdMappingAvatars.find(avatarIter->second.Name);
			if (nameIdIter != nameIdMappingAvatars.end()) //pos vector<string> of ids
			{
				auto idIter = std::find(nameIdIter->second.begin(), nameIdIter->second.end(), id); //pos string in the ids vector<string>
				if (idIter != nameIdIter->second.end())
					nameIdIter->second.erase(idIter);
			}

			avatarsById.erase(avatarIter);
		}
		else
		{
			string message = "could not remove avatar: " + id + " not found";
			Logger::printLog(L_ERROR, message);
			MBoolResponseExtensions::Update(_return, message, false);
		}
	}

}

void MMIScene::RemoveSceneObjects(MBoolResponse & _return, const vector<string>& sceneObjectIDs)
{
	MBoolResponse response{};
	response.Successful = true;

	for (string id : sceneObjectIDs)
	{
		auto iter = this->sceneObjectsById.find(id);
		if (iter != sceneObjectsById.end())
		{
			auto iter1 = this->nameIdMappingSceneObjects.find(iter->second.Name);
			if (iter1 != nameIdMappingSceneObjects.end())
			{
				auto iter2 = std::find(iter1->second.begin(), iter1->second.end(), id);
				if (iter2 != iter1->second.end())
					iter1->second.erase(iter2);
			}

			sceneObjectsById.erase(iter);
		}
		else
		{
			string message = "could not remove scene object:  " + id + " not found";
			Logger::printLog(L_ERROR, message);
			MBoolResponseExtensions::Update(_return, message, false);
		}
	}
}
