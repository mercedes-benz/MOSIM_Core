// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Class for connecting Ajan to the Unreal Engine. Add the UActorComponent to the respective
// AMMIAvatar.
// Basic methods load information from Ajan (Template types, etc.) via a Http Post.
// Establishes thrift connections for creating, executing and deleting Ajan agents (by constructing
// the respective RDF triplets)
// Requires a customized details panel layout (constructed by AjanAgentDetailsPanel)

#pragma once

#include "Windows\AllowWindowsPlatformTypes.h"
#include "gen-cpp/MAJANService.h"
#include "ThriftClient/ThriftClientBinary.h"
#include "Windows\HideWindowsPlatformTypes.h"

#include <string>

#include "Runtime/Online/HTTP/Public/Http.h"

#include "AjanAgentDetailsPanel.h"

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "AjanAgent.generated.h"

using namespace std;
using namespace MMIStandard;

// forward declarations
class MMISettings;
class AMMIAvatar;
class AjanAgentDetailsPanel;

UCLASS( ClassGroup = ( Custom ), meta = ( BlueprintSpawnableComponent ) )
class MMISCENE_API UAjanAgent : public UActorComponent
{
    GENERATED_BODY()

public:
    // Sets default values for this component's properties
    UAjanAgent();

    // destructor
    ~UAjanAgent();

    MMISettings* Settings;
    string AJANServer;
    int AJANPort;
    int AgentCLPort;

    bool Report;

    bool isAjanRunning;
    bool isAgentExisting;
    bool deleteAjanAgentInstSimEnd;

    UPROPERTY( EditAnywhere, Category = "Ajan Settings" )
    FString AJANExecute;

    int index;
    vector<string> list;
    map<string, string> templateList;

    UPROPERTY( EditAnywhere, Category = "Ajan Settings" )
    FString TemplateRepoURI;  // Agent templates are located there

    UFUNCTION( CallInEditor, Category = "Ajan Actions" )
    void CreateAgent();

    UFUNCTION( CallInEditor, Category = "Ajan Actions" )
    void ExecuteAgent();

    UFUNCTION( CallInEditor, Category = "Ajan Actions" )
    void DeleteAgent();

    // the parent actor;
    AActor* ParentActor;

    // get the http API
    FHttpModule* Http;

    /*Assign this function to call when the GET request processes sucessfully*/
    void OnLoadTemplateResponseReceived( FHttpRequestPtr Request, FHttpResponsePtr Response,
                                         bool bWasSuccessful );

    // load the ajan templates
    void Load();
    void LoadTemplates();

    // getter for the keys list
    const vector<string>& GetAjanAgentTemplateOptions();

    void SetClPort( int _clPort );

    // pointer to the details panel
    AjanAgentDetailsPanel* DetailsPanel;

protected:
    // Called when the game starts
    virtual void BeginPlay() override;

public:
    // Called every frame
    virtual void TickComponent( float DeltaTime, ELevelTick TickType,
                                FActorComponentTickFunction* ThisTickFunction ) override;

private:
    string AJANTemplate;
    AMMIAvatar* MMIAvatar;
    string AgentURI;

    // the scene
    UWorld* unrealScene;

    string InitializeGraph();

    string name;

    void setSceneInfos( string& graph );

    void setSceneWriteInfos( string& graph );

    void setSkeletonInfos( string& graph );

    void setRegistryInfos( string& graph );

    void setCosimInfos( string& graph );

public:
    static void ReplaceAllString( string& str, const string& from, const string& to );

    ////////////////////////////////////////////////////////////////
    // methods for synchronizing the Ajan Editor

public:
    UFUNCTION( CallInEditor, Category = "Ajan Actions" )
    void SynchronizeMSceneObjects();

    UPROPERTY( EditAnywhere, Category = "Ajan Settings" )
    FString KnowledgeRepoURI;  // refers currently to the dummy knowledge (see constructor),
                               // MSceneObjects are placed here

private:
    void InitializeCustomizedLayout();
    void UnregisterCustomizedLayout();

    string mObjects;
    string GetAllMSceneObjects();
    void UpdateRepo();
    string GetMSceneObjectRDF( string mSceneObjName, int i );
    void Delete();
    void OnDeleteResponseReceived( FHttpRequestPtr Request, FHttpResponsePtr Response,
                                   bool bWasSuccessful );
    void Upload();
    void OnUploadResponseReceived( FHttpRequestPtr Request, FHttpResponsePtr Response,
                                   bool bWasSuccessful );
};
