// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Class for connecting Ajan to the Unreal Engine. Add the UActorComponent to the respective
// AMMIAvatar.
// Basic methods load information from Ajan (Template types, etc.) via a Http Post.
// Establishes thrift connections for creating, executing and deleting Ajan agents (by constructing
// the respective RDF triplets)
// Requires a customized details panel layout (constructed by AjanAgentDetailsPanel)

#include "AjanAgent.h"
#include "MOSIM.h"
#include "EngineUtils.h"
#include "Engine.h"

#include "MMISettings.h"
#include "MMIAvatar.h"
#include "MMISceneObject.h"
#include "SkeletonAccess.h"
#include "MMUAccess.h"
#include "UnrealSceneAccess.h"

#include "Windows\AllowWindowsPlatformTypes.h"
#include "ThriftClient/ThriftClientBinary.cpp"
#include "Windows\HideWindowsPlatformTypes.h"

#include <algorithm>

#include "Utils/Logger.h"

#include "PropertyEditorModule.h"

// Sets default values for this component's properties
UAjanAgent::UAjanAgent()
    : Settings( nullptr ),
      AJANServer( "127.0.0.1" ),
      AJANPort( 8081 ),
      AgentCLPort( 0 ),
      Report( false ),
      isAjanRunning( false ),
      isAgentExisting( false ),
      deleteAjanAgentInstSimEnd( true ),
      AJANExecute( "execute" ),
      index( 0 ),
      list( vector<string>() ),
      templateList( map<string, string>() ),
      TemplateRepoURI( "http://localhost:8090/rdf4j/repositories/agents" ),
      ParentActor( nullptr ),
      Http( nullptr ),
      DetailsPanel( nullptr ),
      AJANTemplate(
          "http://localhost:8090/rdf4j/repositories/"
          "agents#AG_5b5276cb-bf18-4179-a3b6-f20509fb88a4" ),
      MMIAvatar( nullptr ),
      AgentURI( "" ),
      unrealScene( nullptr ),
      name( "" ),
      KnowledgeRepoURI( "http://localhost:8090/rdf4j/repositories/dummy_knowledge" ),
      mObjects( "" )
{
    Settings = new MMISettings();

    // Set this component to be initialized when the game starts, and to be ticked every frame.  You
    // can turn these features
    // off to improve performance if you don't need them.
    PrimaryComponentTick.bCanEverTick = true;

    // connect to the details panels customization
    InitializeCustomizedLayout();

    // get the parent actor
    ParentActor = GetOwner();

    if( ParentActor == nullptr )
    {
        UE_LOG( LogMOSIM, Warning, TEXT( "AjanAgent component is not set, has no owner." ) );
        // runtime_error("Component is not set, has no owner.");
    }
    else
    {
        MMIAvatar = Cast<AMMIAvatar>( this->ParentActor );
        if( MMIAvatar == nullptr )
        {
            string message = "AjanAgent component is not set to an MMIAvatar as owner.";
            UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *FString( message.c_str() ) );
            GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                              FColor::Red, message.c_str() );
            Logger::printLog( L_ERROR, message );
        }
        else
        {
            name = MMIAvatar->baseName;
        }
    }

    // TODO: is only located here, as begin play is called too late for the simuation controller to
    // be notified
    // --> find different possibility
    this->isAjanRunning = true;

    // When the object is constructed, Get the HTTP module
    Http = &FHttpModule::Get();
    // ...
}

UAjanAgent::~UAjanAgent()
{
    if( this->isAgentExisting && this->deleteAjanAgentInstSimEnd )
        this->DeleteAgent();

    if( this->Settings != nullptr )
    {
        delete Settings;
        this->Settings = nullptr;
    }

    this->UnregisterCustomizedLayout();
}

// Called when the game starts
void UAjanAgent::BeginPlay()
{
    Super::BeginPlay();

    this->Load();
    // ...
}

void UAjanAgent::Load()
{
    this->list.clear();
    // start the loading process in a new thread
    thread* loadingThread = new thread( &UAjanAgent::LoadTemplates, this );
    //// TODO: perhaps detach thread?
    //// loadingThread.detach();
    loadingThread->join();
    delete loadingThread;
}

// Called every frame
void UAjanAgent::TickComponent( float DeltaTime, ELevelTick TickType,
                                FActorComponentTickFunction* ThisTickFunction )
{
    Super::TickComponent( DeltaTime, TickType, ThisTickFunction );
}

void UAjanAgent::LoadTemplates()
{
    // construct the querry
    string query( "query=PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>\n" );
    query.append( "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>\n" );
    query.append( "PREFIX ajan: <http://www.ajan.de/ajan-ns#>\n" );
    query.append( "SELECT ?label ?uri\n" );
    query.append( "WHERE {?uri rdf:type ajan:AgentTemplate. ?uri rdfs:label ?label.}" );

    FString queryFStr( query.c_str() );

    // start the http call
    TSharedRef<IHttpRequest> Request = this->Http->CreateRequest();
    Request->SetContentAsString( queryFStr );
    Request->OnProcessRequestComplete().BindUObject( this,
                                                     &UAjanAgent::OnLoadTemplateResponseReceived );
    // This is the url on which to process the request
    Request->SetURL( this->TemplateRepoURI );
    Request->SetVerb( "POST" );
    Request->SetHeader( TEXT( "Content-Type" ), TEXT( "application/x-www-form-urlencoded" ) );
    Request->ProcessRequest();
}

/*Assigned function on successfull http call*/
void UAjanAgent::OnLoadTemplateResponseReceived( FHttpRequestPtr Request, FHttpResponsePtr Response,
                                                 bool bWasSuccessful )
{
    if( bWasSuccessful )
    {
        string response( TCHAR_TO_UTF8( *Response->GetContentAsString() ) );
        string delimiter( "\n" );
        size_t pos = 0;
        vector<string> responseSplitNewLine = vector<string>();

        while( ( pos = response.find( delimiter ) ) != string::npos )
        {
            responseSplitNewLine.push_back( response.substr( 0, pos ) );
            response.erase( 0, pos + delimiter.length() );
        }

        delimiter = ",";
        pos = 0;
        this->templateList.clear();
        this->list.clear();

        for( int i = 0; i < responseSplitNewLine.size(); i++ )
        {
            while( ( pos = responseSplitNewLine[i].find( delimiter ) ) != string::npos )
            {
                string keyStr = responseSplitNewLine[i].substr( 0, pos );
                UAjanAgent::ReplaceAllString( keyStr, string( "\n" ), string( "" ) );
                UAjanAgent::ReplaceAllString( keyStr, string( "\r" ), string( "" ) );

                string valueStr =
                    responseSplitNewLine[i].substr( pos + 1, responseSplitNewLine[i].length() );
                UAjanAgent::ReplaceAllString( valueStr, string( "\n" ), string( "" ) );
                UAjanAgent::ReplaceAllString( valueStr, string( "\r" ), string( "" ) );

                this->templateList.insert( pair<string, string>{keyStr, valueStr} );
                this->list.push_back( keyStr );

                // delete the parsed part
                responseSplitNewLine[i].erase( 0, pos + 1 );
            }
        }
        this->isAjanRunning = true;

        // notify the details panel about the updated AjanTemplate options
        if( this->DetailsPanel )
            this->DetailsPanel->UpdateAjanTemplateComboBoxOptions();
    }
    else
    {
        string message = "Loading Ajan templates was not succesfull.";
        UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *FString( message.c_str() ) );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Red, message.c_str() );
        Logger::printLog( L_ERROR, message );
    }
}

void UAjanAgent::CreateAgent()
{
    this->AJANTemplate = this->templateList[this->list[this->index]];

    string message = "Index in AjanAgent::CreateAgent() is: " + to_string( this->index );
    Logger::printLog( L_INFO, message );
    UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *FString( message.c_str() ) );

    message = "AjanTemplate in AjanAgent::CreateAgent() is: " + this->AJANTemplate;
    Logger::printLog( L_INFO, message );
    UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *FString( message.c_str() ) );

    // start local clients for the thrift connection, as Ajan is not able to work with multiple
    // client requests in parallel
    ThriftClientBinary<MAJANServiceClient>* thriftClient =
        new ThriftClientBinary<MAJANServiceClient>( AJANServer, AJANPort, false );

    if( thriftClient != nullptr )
    {
        thriftClient->Start();

        try
        {
            MRDFGraph knowledge = MRDFGraph();
            knowledge.__set_ContentType( "text/turtle" );
            knowledge.__set_Graph( this->InitializeGraph() );
            knowledge.__isset.ContentType = true;
            knowledge.__isset.Graph = true;
            thriftClient->access->CreateAgent( this->AgentURI, this->name, this->AJANTemplate,
                                               knowledge );
            this->isAgentExisting = true;
        }
        catch( exception e )
        {
            message =
                "AjanAgent(): Creating Agent failed with error message: " + string( e.what() );
            runtime_error( message.c_str() );
            Logger::printLog( L_ERROR, message );
            UE_LOG( LogMOSIM, Error, TEXT( "%s" ), *FString( message.c_str() ) );
            GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                              FColor::Red, message.c_str() );
            this->AgentURI = string( "" );
        }
    }
    // deleting the client includes closing the connection
    delete thriftClient;
}

void UAjanAgent::SetClPort( int _clPort )
{
    this->AgentCLPort = _clPort;
}

string UAjanAgent::InitializeGraph()
{
    string graph = string( "" );
    string avatar = string( "<127.0.0.1:9000/avatars/" + this->name + ">" );

    // get the avatar location
    FVector avaLoc = this->MMIAvatar->GetActorLocation();
    // coordinate transformation
    avaLoc = avaLoc / 100;
    FVector transformedLoc = FVector( -avaLoc.X, avaLoc.Z, avaLoc.Y );
    // convert to string
    string locString = string( TCHAR_TO_UTF8( *transformedLoc.ToString() ) );

    graph.append( avatar + " " + "<http://www.w3.org/1999/02/22-rdf-syntax-ns#type>" + " " +
                  "<http://www.dfki.de/mosim-ns#Avatar>" + "." );
    graph.append( avatar + " " + "<http://www.dfki.de/mosim-ns#id>" + " " + "'" +
                  this->MMIAvatar->MAvatar.ID + "'" + "." );
    graph.append( avatar + " " + "<http://www.dfki.de/mosim-ns#clPort>" + " " + "'" +
                  to_string( this->AgentCLPort ) + "'" + "." );
    graph.append( avatar + " " + "<http://www.dfki.de/mosim-ns#transform>" + " " + "'" + locString +
                  "'" + "." );
    graph.append( avatar + " " + "<http://www.dfki.de/mosim-ns#isLocatedAt>" + " " +
                  "<http://www.dfki.de/mosim-ns#InitPosition>" + "." );
    if( this->Report )
        graph.append( avatar + " " +
                      "<http://www.ajan.de/ajan-ns#agentReportURI> "
                      "'http://localhost:4202/report'^^<http://www.w3.org/2001/XMLSchema#anyURI> "
                      "." );

    this->setSceneInfos( graph );
    // this->setSceneWriteInfos(graph);   // TODO: scene write server is not implented so far
    this->setSkeletonInfos( graph );
    this->setRegistryInfos( graph );
    this->setCosimInfos( graph );

    return graph;
}

void UAjanAgent::setSceneInfos( string& graph )
{
    string scene = string( "_:scene" );
    graph.append( scene + " " + "<http://www.w3.org/1999/02/22-rdf-syntax-ns#type>" + " " +
                  "<http://www.dfki.de/mosim-ns#Scene>" + "." );
    graph.append( scene + " " + "<http://www.dfki.de/mosim-ns#host>" + " '" +
                  this->Settings->RemoteSceneAccessAddress +
                  "'^^<http://www.w3.org/2001/XMLSchema#string>" + "." );
    graph.append( scene + " " + "<http://www.dfki.de/mosim-ns#port>" + " '" +
                  to_string( this->Settings->RemoteSceneAccessPort ) +
                  "'^^<http://www.w3.org/2001/XMLSchema#string>" +
                  "." );  // hier immer +1 bei neuem agenten
}

void UAjanAgent::setSceneWriteInfos( string& graph )
{
    string scene = string( "_:sceneWrite" );
    graph.append( scene + " " + "<http://www.w3.org/1999/02/22-rdf-syntax-ns#type>" + " " +
                  "<http://www.dfki.de/mosim-ns#SceneWrite>" + "." );
    graph.append( scene + " " + "<http://www.dfki.de/mosim-ns#host>" + " '" +
                  this->Settings->RemoteSceneWriteAddress +
                  "'^^<http://www.w3.org/2001/XMLSchema#string>" + "." );
    graph.append( scene + " " + "<http://www.dfki.de/mosim-ns#port>" + " '" +
                  to_string( this->Settings->RemoteSceneWritePort ) +
                  "'^^<http://www.w3.org/2001/XMLSchema#string>" + "." );
}

void UAjanAgent::setSkeletonInfos( string& graph )
{
    string scene = "_:skeleton";
    graph.append( scene + " " + "<http://www.w3.org/1999/02/22-rdf-syntax-ns#type>" + " " +
                  "<http://www.dfki.de/mosim-ns#SkeletonAccess>" + "." );
    graph.append( scene + " " + "<http://www.dfki.de/mosim-ns#host>" + " '" +
                  this->MMIAvatar->skeletonAccessPtr->skeletonServiceAddr->Address +
                  "'^^<http://www.w3.org/2001/XMLSchema#string>" + "." );
    graph.append( scene + " " + "<http://www.dfki.de/mosim-ns#port>" + " '" +
                  to_string( this->MMIAvatar->skeletonAccessPtr->skeletonServiceAddr->Port ) +
                  "'^^<http://www.w3.org/2001/XMLSchema#string>" + "." );
}

void UAjanAgent::setRegistryInfos( string& graph )
{
    string registry = "_:registry";
    graph.append( registry + " " + "<http://www.w3.org/1999/02/22-rdf-syntax-ns#type>" + " " +
                  "<http://www.dfki.de/mosim-ns#Registry>" + "." );
    graph.append( registry + " " + "<http://www.dfki.de/mosim-ns#host>" + " '" +
                  this->Settings->MMIRegisterAddress +
                  "'^^<http://www.w3.org/2001/XMLSchema#string>" + "." );
    graph.append( registry + " " + "<http://www.dfki.de/mosim-ns#port>" + " '" +
                  to_string( this->Settings->MMIRegisterPort ) +
                  "'^^<http://www.w3.org/2001/XMLSchema#string>" + "." );
}

void UAjanAgent::setCosimInfos( string& graph )
{
    string cosim = "_:cosim";
    graph.append( cosim + " " + "<http://www.w3.org/1999/02/22-rdf-syntax-ns#type>" + " " +
                  "<http://www.dfki.de/mosim-ns#CoSimulator>" + "." );
    graph.append( cosim + " " + "<http://www.dfki.de/mosim-ns#host>" + " '" +
                  this->Settings->RemoteCoSimulationAccessAddress +
                  "'^^<http://www.w3.org/2001/XMLSchema#string>" + "." );
    UE_LOG( LogMOSIM, Warning, TEXT( "RemoteCoSimulationAccessPort: %i" ),
            this->MMIAvatar->RemoteCoSimulationAccessPort );
    graph.append( cosim + " " + "<http://www.dfki.de/mosim-ns#port>" + " '" +
                  to_string( this->MMIAvatar->RemoteCoSimulationAccessPort ) +
                  "'^^<http://www.w3.org/2001/XMLSchema#string>" + "." );
}

void UAjanAgent::ExecuteAgent()
{
    // start local clients for the thrift connection, as Ajan is not able to work with multiple
    // client requests in parallel
    ThriftClientBinary<MAJANServiceClient>* thriftClient =
        new ThriftClientBinary<MAJANServiceClient>( AJANServer, AJANPort, false );

    if( thriftClient != nullptr )
    {
        thriftClient->Start();
        try
        {
            MRDFGraph knowledge = MRDFGraph();
            knowledge.ContentType = "text/turtle";
            knowledge.Graph =
                "_:test <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> "
                "<http://www.w3.org/1999/02/22-rdf-syntax-ns#Resource> .";
            thriftClient->access->ExecuteAgent(
                this->AgentURI, name, string( TCHAR_TO_UTF8( *this->AJANExecute ) ), knowledge );
        }
        catch( ... )
        {
            string message = "Errors in executing Agent.";  // TODO: define error message
            runtime_error( message.c_str() );
            Logger::printLog( L_ERROR, message );
            UE_LOG( LogMOSIM, Error, TEXT( "%s" ), *FString( message.c_str() ) );
            GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                              FColor::Red, message.c_str() );
        }
    }
    // deleting the client includes closing the connection
    delete thriftClient;
}

void UAjanAgent::DeleteAgent()
{
    // start local clients for the thrift connection, as Ajan is not able to work with multiple
    // client requests in parallel
    ThriftClientBinary<MAJANServiceClient>* thriftClient =
        new ThriftClientBinary<MAJANServiceClient>( AJANServer, AJANPort, false );

    if( thriftClient != nullptr )
    {
        thriftClient->Start();
        try
        {
            thriftClient->access->DeleteAgent( this->name );
            this->isAgentExisting = false;
            this->AgentURI = string( "" );
        }
        catch( ... )
        {
            string message = "Errors in deleting Agent.";  // TODO: define error message
            runtime_error( message.c_str() );
            Logger::printLog( L_ERROR, message );
            UE_LOG( LogMOSIM, Error, TEXT( "%s" ), *FString( message.c_str() ) );
            GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                              FColor::Red, message.c_str() );
            this->AgentURI = string( "" );
        }
    }
    // deleting the client includes closing the connection
    delete thriftClient;
}

//////////////////////////////////////////////////////////////
// static methods

void UAjanAgent::ReplaceAllString( string& str, const string& from, const string& to )
{
    size_t start_pos = 0;
    while( ( start_pos = str.find( from, start_pos ) ) != std::string::npos )
    {
        str.replace( start_pos, from.length(), to );
        start_pos += to.length();  // Handles case where 'to' is a substring of 'from'
    }
}

//////////////////////////////////////////////////////////////
// methods for synchronizing the Ajan Editor

void UAjanAgent::SynchronizeMSceneObjects()
{
    this->mObjects = GetAllMSceneObjects();
    string message = "The following MSceneObjects are synchronized with Ajan: " + mObjects;
    Logger::printLog( L_INFO, message );
    UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *FString( message.c_str() ) );
    this->UpdateRepo();
}

string UAjanAgent::GetAllMSceneObjects()
{
    // Get vector ofr storing the MMISceneObjects
    vector<string> sceneObjectNames = vector<string>();
    // collect all avatars
    UWorld* scene = this->GetWorld();
    // collect all MMISceneObjects
    for( TActorIterator<AActor> USceneObjItr( scene ); USceneObjItr; ++USceneObjItr )
    {
        TArray<USceneComponent*> ComponentsArray;
        USceneObjItr->GetComponents<USceneComponent>( ComponentsArray );
        for( USceneComponent* sceneComponent : ComponentsArray )
        {
            UMMISceneObject* mmiSceneObject = Cast<UMMISceneObject>( sceneComponent );
            if( mmiSceneObject )
            {
                // update the names of the Object
                mmiSceneObject->updateNames();
                UE_LOG( LogMOSIM, Display, TEXT( "UMMISceneObject with name %s found." ),
                        *FString( mmiSceneObject->Name.c_str() ) );
                sceneObjectNames.push_back( mmiSceneObject->MSceneObject.Name );
            }
        }
    }
    string RDFObjects = string( "" );
    int numObj = 0;
    for( string mSceneObjName : sceneObjectNames )
    {
        RDFObjects.append( this->GetMSceneObjectRDF( mSceneObjName, numObj++ ) );
    }
    return RDFObjects;
}

void UAjanAgent::UpdateRepo()
{
    this->Delete();
}

string UAjanAgent::GetMSceneObjectRDF( string mSceneObjName, int numObj )
{
    string graph = string( "" );
    string sceneObject = "<tcp://" + this->Settings->RemoteSceneAccessAddress + ":" +
                         to_string( this->Settings->RemoteSceneAccessPort ) + "/" +
                         to_string( numObj ) + ">";
    graph.append( sceneObject + " " + "<http://www.w3.org/1999/02/22-rdf-syntax-ns#type>" + " " +
                  "<http://www.dfki.de/mosim-ns#MSceneObject> ." );
    graph.append( sceneObject + " " + "<http://www.w3.org/2000/01/rdf-schema#label>" + " " + "'" +
                  mSceneObjName + "'" + "." );
    return graph;
}

void UAjanAgent::Delete()
{
    // construct the querry
    FString msgFStr( "update=DELETE {?s ?p ?o} WHERE {?s ?p ?o}" );

    // start the http call
    TSharedRef<IHttpRequest> Request = this->Http->CreateRequest();
    Request->SetContentAsString( msgFStr );
    Request->OnProcessRequestComplete().BindUObject( this, &UAjanAgent::OnDeleteResponseReceived );
    // This is the url on which to process the request
    Request->SetURL( FString( this->KnowledgeRepoURI + "/statements" ) );
    Request->SetVerb( "POST" );
    Request->SetHeader( TEXT( "Content-Type" ), TEXT( "application/x-www-form-urlencoded" ) );
    Request->ProcessRequest();
}

void UAjanAgent::OnDeleteResponseReceived( FHttpRequestPtr Request, FHttpResponsePtr Response,
                                           bool bWasSuccessful )
{
    if( bWasSuccessful )
    {
        string message = "Deleted MSceneObjects in Ajan Repository.";
        UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *FString( message.c_str() ) );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Green, message.c_str() );
        Logger::printLog( L_ERROR, message );

        this->Upload();
    }
    else
    {
        string message = "Delete Request in Ajan Repository was not succesfull.";
        UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *FString( message.c_str() ) );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Red, message.c_str() );
        Logger::printLog( L_ERROR, message );
    }
}

void UAjanAgent::Upload()
{
    // construct the querry
    FString msgFStr( "update=INSERT DATA {" + FString( this->mObjects.c_str() ) + "} " );

    // start the http call
    TSharedRef<IHttpRequest> Request = this->Http->CreateRequest();
    Request->SetContentAsString( msgFStr );
    Request->OnProcessRequestComplete().BindUObject( this, &UAjanAgent::OnUploadResponseReceived );
    // This is the url on which to process the request
    Request->SetURL( FString( this->KnowledgeRepoURI + "/statements" ) );
    Request->SetVerb( "POST" );
    Request->SetHeader( TEXT( "Content-Type" ), TEXT( "application/x-www-form-urlencoded" ) );
    Request->ProcessRequest();
}

void UAjanAgent::OnUploadResponseReceived( FHttpRequestPtr Request, FHttpResponsePtr Response,
                                           bool bWasSuccessful )
{
    if( bWasSuccessful )
    {
        string message = "Synchronization of MSceneObjects with Ajan Repository was succesfull.";
        UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *FString( message.c_str() ) );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Green, message.c_str() );
        Logger::printLog( L_ERROR, message );
    }
    else
    {
        string message =
            "Synchronization of MSceneObjects with Ajan Repository was not succesfull.";
        UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *FString( message.c_str() ) );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Red, message.c_str() );
        Logger::printLog( L_ERROR, message );
    }
}

const vector<string>& UAjanAgent::GetAjanAgentTemplateOptions()
{
    return this->list;
}

void UAjanAgent::InitializeCustomizedLayout()
{
    // Get the property module for loading
    FPropertyEditorModule& PropertyModule =
        FModuleManager::LoadModuleChecked<FPropertyEditorModule>( "PropertyEditor" );

    // Register the custom details panel we have created
    PropertyModule.RegisterCustomClassLayout(
        UAjanAgent::StaticClass()->GetFName(),
        FOnGetDetailCustomizationInstance::CreateStatic( &AjanAgentDetailsPanel::MakeInstance ) );

    PropertyModule.NotifyCustomizationModuleChanged();
}

void UAjanAgent::UnregisterCustomizedLayout()
{
    // unregister customized layouts
    if( FModuleManager::Get().IsModuleLoaded( "PropertyEditor" ) )
    {
        auto& PropertyModule =
            FModuleManager::LoadModuleChecked<FPropertyEditorModule>( "PropertyEditor" );

        PropertyModule.UnregisterCustomClassLayout( UAjanAgent::StaticClass()->GetFName() );
    }
}