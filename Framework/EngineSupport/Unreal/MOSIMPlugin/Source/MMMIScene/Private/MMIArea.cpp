// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Unrequired class, informs Ajan in case colliders do overlapp

#include "MMIArea.h"
#include "MOSIM.h"

#include "MMISceneObject.h"
#include "Utils/Logger.h"

// Sets default values for this component's properties
UMMIArea::UMMIArea() : uri( string( "" ) ), ParentActor( nullptr ), sceneObject( nullptr )
{
    // Set this component to be initialized when the game starts, and to be ticked every frame.  You
    // can turn these features
    // off to improve performance if you don't need them.
    PrimaryComponentTick.bCanEverTick = true;

    // get the root component of the top level actor
    ParentActor = GetOwner();
    if( ParentActor )
    {
        TArray<UMMISceneObject*> MMISceneObjects;
        ParentActor->GetComponents<UMMISceneObject>( MMISceneObjects );
        if( MMISceneObjects.Num() > 0 )
        {
            sceneObject = &MMISceneObjects[0]->MSceneObject;
            if( sceneObject )
            {
                if( sceneObject->Properties.size() == 0 )
                {
                    sceneObject->__set_Properties( map<string, string>() );
                    sceneObject->Properties.insert( pair<string, string>{"type", "Area"} );
                    sceneObject->Properties.insert( pair<string, string>{"contains", "{}"} );
                }
            }
        }
        else
        {
            string message = "MMIArea component is not set to an MMISceneObject as owner.";
            Logger::printLog( L_ERROR, message );
            UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *FString( message.c_str() ) );
        }
    }
}

UMMIArea::~UMMIArea()
{
    if( this->sceneObject )
        delete sceneObject;
}

// Called when the game starts
void UMMIArea::BeginPlay()
{
    Super::BeginPlay();
}

// Called every frame
void UMMIArea::TickComponent( float DeltaTime, ELevelTick TickType,
                              FActorComponentTickFunction* ThisTickFunction )
{
    Super::TickComponent( DeltaTime, TickType, ThisTickFunction );
}

void UMMIArea::OnOverlapBegin( class AActor* OtherActor, class UPrimitiveComponent* OtherComp,
                               int32 OtherBodyIndex, bool bFromSweep,
                               const FHitResult& SweepResult )
{
    if( this->ParentActor && OtherActor && ( OtherActor != this->ParentActor ) && OtherComp )
    {
        TArray<UMMISceneObject*> OtherMMISceneObjects;
        OtherActor->GetComponents<UMMISceneObject>( OtherMMISceneObjects );
        if( OtherMMISceneObjects.Num() > 0 )
        {
            MSceneObject* otherSceneObject = &OtherMMISceneObjects[0]->MSceneObject;
            if( otherSceneObject )
            {
                if( this->contains.end() ==
                    find( this->contains.begin(), this->contains.end(), otherSceneObject->ID ) )
                {
                    UE_LOG( LogMOSIM, Warning, TEXT( "%s contains %s." ),
                            *FString( this->sceneObject->Name.c_str() ),
                            *FString( otherSceneObject->Name.c_str() ) );
                    Logger::printLog( L_INFO, string( this->sceneObject->Name + " contains " +
                                                      otherSceneObject->Name ) );

                    contains.push_back( otherSceneObject->ID );
                    otherSceneObject->Properties.insert(
                        pair<string, string>{"contains", this->GetListAsString( contains )} );
                }
            }
        }
    }
}

void UMMIArea::OnOverlapEnd( class AActor* OtherActor, class UPrimitiveComponent* OtherComp,
                             int32 OtherBodyIndex )
{
    if( this->ParentActor && OtherActor && ( OtherActor != this->ParentActor ) && OtherComp )
    {
        TArray<UMMISceneObject*> OtherMMISceneObjects;
        OtherActor->GetComponents<UMMISceneObject>( OtherMMISceneObjects );
        if( OtherMMISceneObjects.Num() > 0 )
        {
            MSceneObject* otherSceneObject = &OtherMMISceneObjects[0]->MSceneObject;
            if( otherSceneObject )
            {
                vector<string>::iterator it =
                    find( this->contains.begin(), this->contains.end(), otherSceneObject->ID );
                if( this->contains.end() != it )
                {
                    contains.erase( it );
                    otherSceneObject->Properties.insert(
                        pair<string, string>{"contains", this->GetListAsString( contains )} );
                }
            }
        }
    }
}

string UMMIArea::GetListAsString( vector<string>& list )
{
    string stringList = "{";

    for( vector<string>::iterator iter = list.begin(); iter != list.end(); iter++ )
    {
        stringList += *iter;
        if( iter == list.end() - 1 )
        {
            stringList += ",";
        }
    }
    return stringList + "}";
}
