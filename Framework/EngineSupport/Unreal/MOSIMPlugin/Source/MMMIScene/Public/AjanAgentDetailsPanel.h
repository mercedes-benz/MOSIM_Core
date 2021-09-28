// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Customized details panel for the AjanAgent
// constructs dynamic dropdown for selecting the Ajan template type and
// the checkbox for deleting the Ajan Agent at the simulation end

#pragma once

#include "CoreMinimal.h"
#include "Input/Reply.h"
#include "IDetailCustomization.h"
#include "Components/ComboBox.h"

class UAjanAgent;

class AjanAgentDetailsPanel : public IDetailCustomization
{
private:
    // Callbacks for the ComboBox
    void HandleAjanTemplateComboBoxSelectionChanged( TSharedPtr<FString> NewSelection,
                                                     ESelectInfo::Type SelectInfo );
    TSharedRef<SWidget> HandleComboBoxGenerateWidget( TSharedPtr<FString> InItem );
    FText HandleAjanTemplateComboBoxText() const;

    // callbacks for the delete ajan agent checkbox
    ECheckBoxState IsDeleteAjanAgentChecked() const;
    void DeleteAjanAgentToggled( ECheckBoxState NewState );

    // Holds the selected combo box item.
    TSharedPtr<FString> ComboString;

    // get the correspoding Ajan Agent
    void GetAjanAgentObject( IDetailLayoutBuilder& DetailLayout );

    // access to the corresponding ajan agent
    UAjanAgent* AjanAgent;

public:
    // update the combo box options
    void UpdateAjanTemplateComboBoxOptions();

    // Holds the combo box
    TSharedPtr<SComboBox<TSharedPtr<FString> > > AjanTemplateComboBox;

    // Holds the options for the SComboBox.
    TArray<TSharedPtr<FString> > AjanTemplateComboBoxOptions;

    // Holds the selected text in the SComboBox
    TSharedPtr<FString> AjanTemplateComboBoxSelectedItem;

    /* Makes a new instance of this detail layout class for a specific detail view requesting it */
    static TSharedRef<IDetailCustomization> MakeInstance();

    /* IDetalCustomization interface */
    virtual void CustomizeDetails( IDetailLayoutBuilder& DetailBuilder ) override;
};