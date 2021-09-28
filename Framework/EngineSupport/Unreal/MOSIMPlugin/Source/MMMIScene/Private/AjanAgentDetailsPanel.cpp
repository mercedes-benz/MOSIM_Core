// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Customized details panel for the AjanAgent
// constructs dynamic dropdown for selecting the Ajan template type and
// the checkbox for deleting the Ajan Agent at the simulation end

#include "AjanAgentDetailsPanel.h"
#include "IDetailsView.h"
#include "DetailLayoutBuilder.h"
#include "DetailWidgetRow.h"
#include "DetailCategoryBuilder.h"
#include "Widgets/SNullWidget.h"
#include "Internationalization/Text.h"
#include "AjanAgent.h"
#include "UObject/Class.h"
#include "EngineUtils.h"
#include "Engine.h"
#include "Widgets/Input/SComboBox.h"
#include <algorithm>

TSharedRef<IDetailCustomization> AjanAgentDetailsPanel::MakeInstance()
{
    return MakeShareable( new AjanAgentDetailsPanel );
}

void AjanAgentDetailsPanel::CustomizeDetails( IDetailLayoutBuilder& DetailBuilder )
{
    // get the corresponding compontent
    this->GetAjanAgentObject( DetailBuilder );

    if( !this->AjanAgent )
        this->AjanAgent->DetailsPanel = this;

    // Edits a category. If it doesn't exist it creates a new one
    IDetailCategoryBuilder& ComboBoxCategory =
        DetailBuilder.EditCategory( "Ajan Agent Template Type" );

    // clang-format off
    ComboBoxCategory.AddCustomRow( FText::FromString( "Ajan Agent Template Type" ) )
    [
		SNew( SHorizontalBox ) 
		+ SHorizontalBox::Slot().FillWidth( 0.5f )
		[
             // Ajan Template combo box
             SAssignNew( AjanTemplateComboBox, SComboBox<TSharedPtr<FString> > )
             .OptionsSource( &AjanTemplateComboBoxOptions )
             .OnSelectionChanged(
                     this, &AjanAgentDetailsPanel::HandleAjanTemplateComboBoxSelectionChanged )
             .OnGenerateWidget( this, &AjanAgentDetailsPanel::HandleComboBoxGenerateWidget )
             [
				 SNew( STextBlock )
                 .Text( this, &AjanAgentDetailsPanel::HandleAjanTemplateComboBoxText )
			 ]
		]
	];

    // intialize the combo box
    UpdateAjanTemplateComboBoxOptions();

    // Edits a category. If it doesn't exist it creates a new one
    IDetailCategoryBuilder& AjanSettingsCategory =
        DetailBuilder.EditCategory( "Delete Ajan Agent Instance at Simulation Stop" );

    AjanSettingsCategory.AddCustomRow( FText::FromString( "Ajan Settings" ) )
    [
		SNew( SVerticalBox )
		
		+ SVerticalBox::Slot()
        [
			SNew( SCheckBox )
            .IsChecked( this, &AjanAgentDetailsPanel::IsDeleteAjanAgentChecked )
            .OnCheckStateChanged( this, &AjanAgentDetailsPanel::DeleteAjanAgentToggled )
		]
	];
    // clang-format on
}

// Callback for getting the text of the combo box
FText AjanAgentDetailsPanel::HandleAjanTemplateComboBoxText() const
{
    return ComboString.IsValid() ? FText::FromString( *ComboString ) : FText::GetEmpty();
}

// Callback for generating a widget in the SComboBox
TSharedRef<SWidget> AjanAgentDetailsPanel::HandleComboBoxGenerateWidget(
    TSharedPtr<FString> InItem )
{
    return SNew( STextBlock ).Text( FText::FromString( *InItem ) );
}

// Callback for changing the combo box's selection
void AjanAgentDetailsPanel::HandleAjanTemplateComboBoxSelectionChanged(
    TSharedPtr<FString> NewSelection, ESelectInfo::Type SelectInfo )
{
    ComboString = NewSelection;

    string selectedString = string( TCHAR_TO_UTF8( **NewSelection.Get() ) );

    if( this->AjanAgent )
    {
        vector<string> list = AjanAgent->GetAjanAgentTemplateOptions();
        std::vector<string>::iterator it = std::find( list.begin(), list.end(), selectedString );

        if( it != list.end() )
            AjanAgent->index = std::distance( list.begin(), it );
    }
}

void AjanAgentDetailsPanel::GetAjanAgentObject( IDetailLayoutBuilder& DetailLayout )
{
    // get all objects that are currently selected in the editor:
    const TArray<TWeakObjectPtr<UObject> >& SelectedObjects =
        DetailLayout.GetDetailsView()->GetSelectedObjects();

    bool bEditingActor = false;

    // loop all selected objects:
    this->AjanAgent = nullptr;
    for( int32 ObjectIndex = 0; ObjectIndex < SelectedObjects.Num(); ++ObjectIndex )
    {
        UObject* TestObject = SelectedObjects[ObjectIndex].Get();
        // check if current object is an AActor:
        if( UAjanAgent* CurrentActor = Cast<UAjanAgent>( TestObject ) )
        {
            bEditingActor = true;
            this->AjanAgent = CurrentActor;
            break;
        }
    }
}

void AjanAgentDetailsPanel::UpdateAjanTemplateComboBoxOptions()
{
    if( !this->AjanAgent )
        return;

    int counter = 0;

    for( string templateKey : this->AjanAgent->GetAjanAgentTemplateOptions() )
    {
        if( counter == 0 )
        {
            TSharedPtr<FString> SelectedItem = MakeShareable( new FString( templateKey.c_str() ) );
            AjanTemplateComboBoxOptions.Add( SelectedItem );
            AjanTemplateComboBox->SetSelectedItem( SelectedItem );
            AjanTemplateComboBoxSelectedItem = SelectedItem;
        }
        else
        {
            AjanTemplateComboBoxOptions.Add( MakeShareable( new FString( templateKey.c_str() ) ) );
        }

        counter++;
    }
    AjanTemplateComboBox->RefreshOptions();
}

ECheckBoxState AjanAgentDetailsPanel::IsDeleteAjanAgentChecked() const
{
    if( this->AjanAgent )
        return this->AjanAgent->deleteAjanAgentInstSimEnd ? ECheckBoxState::Checked
                                                          : ECheckBoxState::Unchecked;
    else
        return ECheckBoxState::Unchecked;
}

void AjanAgentDetailsPanel::DeleteAjanAgentToggled( ECheckBoxState NewState )
{
    if( this->AjanAgent )
        NewState == ECheckBoxState::Checked ? this->AjanAgent->deleteAjanAgentInstSimEnd = true
                                            : this->AjanAgent->deleteAjanAgentInstSimEnd = false;
}
