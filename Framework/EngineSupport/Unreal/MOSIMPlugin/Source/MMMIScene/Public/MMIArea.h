// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Unrequired class, informs Ajan in case colliders do overlapp

#pragma once

#include <string>
#include <vector>

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "MMIArea.generated.h"

// forward declarations
namespace MMIStandard
{
class MSceneObject;
};

using namespace std;
using namespace MMIStandard;

UCLASS( ClassGroup = ( Custom ), meta = ( BlueprintSpawnableComponent ) )
class MMISCENE_API UMMIArea : public UActorComponent
{
    GENERATED_BODY()

public:
    // Sets default values for this component's properties
    UMMIArea();

    // destructor
    ~UMMIArea();

    string uri;

protected:
    // Called when the game starts
    virtual void BeginPlay() override;

public:
    // Called every frame
    virtual void TickComponent( float DeltaTime, ELevelTick TickType,
                                FActorComponentTickFunction* ThisTickFunction ) override;

private:
    /** called when something enters the sphere component */
    UFUNCTION()
    void OnOverlapBegin( class AActor* OtherActor, class UPrimitiveComponent* OtherComp,
                         int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult );

    /** called when something leaves the sphere component */
    UFUNCTION()
    void OnOverlapEnd( class AActor* OtherActor, class UPrimitiveComponent* OtherComp,
                       int32 OtherBodyIndex );

    string GetListAsString( vector<string>& list );

    AActor* ParentActor;
    MSceneObject* sceneObject;
    vector<string> contains;
};
