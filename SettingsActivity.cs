﻿// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        Settings
// Filename:    SettingsActivity.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The SettingsActivity activity displays the current configuration of an AutoNag widget.
//				 
// Description:  The settings are AutoNag wide (creation amd deletion of lists) and AppWidget specific
//
//
//
// File History
// ------------
//
// %version:  1 %
//
// (c) Copyright 2015 Trevor Simmonds.
// This software is protected by copyright, the design of any 
// article recorded in the software is protected by design 
// right and the information contained in the software is 
// confidential. This software may not be copied, any design 
// may not be reproduced and the information contained in the 
// software may not be used or disclosed except with the
// prior written permission of and in a manner permitted by
// the proprietors Trevor Simmonds (c) 2015
//
//    Copyright Holders:
//       Trevor Simmonds,
//       t.simmonds@virgin.net
//


using Android.Preferences;
using Android.OS;
using Android.App;
using Android.Appwidget;
using System.Collections.Generic;
using Android.Content;
using Android.Widget;
using System;
using Android.Views.InputMethods;
using Android.Views;

namespace AutoNag
{
	/// <summary>
	/// The SettingsActivity activity displays the current configuration of an AutoNag widget.
	/// </summary>
	[Activity (Label="AutoNag settings") ]
	public class SettingsActivity : PreferenceActivity
	{
		/// <summary>
		/// Display delegate.
		/// </summary>
		public delegate void DisplayDelegate( Dialog dialogue );

		/// <summary>
		/// Default constructor
		/// </summary>
		public SettingsActivity()
		{
		}

		//
		// Protected methods
		//

		/// <summary>
		/// Called when the Activity is created prior to being displayed.
		/// Note that this is also called when the device is rotated.
		/// </summary>
		/// <param name="savedInstanceState">Saved instance state.</param>
		protected override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );

			// Load the preferences definition
			AddPreferencesFromResource( Resource.Xml.Preferences );

			// Get the identity of the widget that launched this activity
			widgetIdentity = new WidgetIntent( Intent ).WidgetIdProperty;

			// Get a reference to the preference items used elsewhere in this class
			PreferenceScreen screen = ( PreferenceScreen )FindPreference( GetString( Resource.String.settingsKey ) );

			currentListPreference = ( CustomPreference )screen.FindPreference( GetString( Resource.String.widgetCurrentListKey ) );
			createPreference = ( CustomPreference )screen.FindPreference( GetString( Resource.String.createListKey ) );
			deletePreference = ( ListPreference )screen.FindPreference( GetString( Resource.String.deleteListKey ) );
			colourPreference = ( ListPreference )screen.FindPreference( GetString( Resource.String.colourListKey ) );
			renamePreference = ( ListPreference )screen.FindPreference( GetString( Resource.String.renameScreenKey ) );

			availableListsCategory = ( PreferenceCategory )screen.FindPreference( GetString( Resource.String.widgetSelectListCategoryKey ) );

			// Initialise any items that show task list names
			InitialiseTaskListItem();

			// Set up the click handlers
			deletePreference.SelectedProperty = OnDeleteList;
			colourPreference.SelectedProperty = OnColourList;
			renamePreference.SelectedProperty = OnRenameList;
			createPreference.SelectedProperty = OnCreateList;
		}

		/// <summary>
		/// Called as part of the activity lifecycle when an activity is going into
		///  the background, but has not (yet) been killed.
		/// Dismiss any dialogues being shown by the ListPreference classes
		/// </summary>
		protected override void OnPause()
		{
			base.OnPause();

			deletePreference.OnDismiss();
			colourPreference.OnDismiss();
			renamePreference.OnDismiss();

			if ( dialogueBeingShown != null )
			{
				dialogueBeingShown.Dismiss();
				dialogueBeingShown = null;
			}
		}

		//
		// Private methods
		//

		/// <summary>
		/// Initialises the available lists collection and refreshes the other preferences that contain list names
		/// </summary>
		private void InitialiseTaskListItem()
		{
			// Display the currently selected task list name
			DisplayTaskListName();

			// Remove all existing entries from the available Lists Category
			availableListsCategory.RemoveAll();
		
			foreach ( string listName in TaskRepository.GetTaskTables() )
			{
				// Don't show the selected list
				if ( listName != currentListName )
				{
					availableListsCategory.AddPreference( new CustomPreference( this, OnSelectList, ListColourHelper.GetColourResource( listName ), listName ) );
				}
			}

			deletePreference.NotifyDataSetChanged();
			colourPreference.NotifyDataSetChanged();
			renamePreference.NotifyDataSetChanged();
		}

		/// <summary>
		/// The user has changed the list associated with the widget that initiated this activity
		/// Get the new list name. Store it and broadcast the change to the widget
		/// </summary>
		/// <param name="listName">List name.</param>
		private void OnSelectList( string taskListName )
		{
			// Update the persisted association
			ListNamePersistence.SetListName( this, widgetIdentity, taskListName );

			// Display it 
			DisplayTaskListName();

			// Tell the widget
			SendBroadcast( new WidgetIntent( AutoNagWidget.ListChangedAction ).SetWidgetId( widgetIdentity ) );

			// This is usually all the user wishes to do, so get out of here
			Finish();
		}

		/// <summary>
		/// The user has selected a list to rename. Allow the user to rename the lsit
		/// Check that this is not a duplicate name and if unique update the table name in the database
		/// Update any ListNamePersistence items that contain this name and then inform the AutoNagWidget of the change 
		/// </summary>
		/// <param name="changedItemKey">Changed item key.</param>
		private void OnRenameList( string taskListName )
		{
			// Display the rename dialogue and process the new name
			ShowEditTextDialogue( string.Format( "Rename <{0}>", taskListName ), taskListName, ( newName ) =>
			{
				// Check if the new name is valid
				if ( TaskRepository.GetTaskTables().Contains( newName ) == true )
				{
					// A list with this name already exists
					// Tell the user
					ShowDialogue( new AlertDialog.Builder( this )
						.SetMessage( string.Format( "Task list <{0}> already exists.", newName ) )
						.SetPositiveButton( "Ok", ( buttonSender, buttonEvents ) => {} )
						.Create() );
				}
				else
				{
					// Attempt the rename
					if ( TaskRepository.RenameTaskList( taskListName, newName ) == true )
					{
						// Get all the ListNamePersistence entries and update any with the old name
						foreach( KeyValuePair< int, string > item in ListNamePersistence.GetItems( this ) )
						{
							if ( item.Value == taskListName )
							{
								ListNamePersistence.SetListName( this, item.Key, newName );
							}
						}

						// Re-initialise any items showing the list names
						InitialiseTaskListItem();

						// Tell the widget
						SendBroadcast( new WidgetIntent( AutoNagWidget.ListRenamedAction ).SetTaskListName( newName ) );
					}
				}
			});
		}

		/// <summary>
		/// The user wishes to create a new list.  Allow the user to enter a new namew.
		/// If the list name is unique create the associated table
		/// </summary>
		private void OnCreateList( string taskListName )
		{
			// Get new name from the user
			ShowEditTextDialogue( "Create new list", "", ( listName ) =>
			{
				if ( TaskRepository.GetTaskTables().Contains( listName ) == true )
				{
					// A list with this name already exists
					// Tell the user
					ShowDialogue( new AlertDialog.Builder( this )
						.SetMessage( string.Format( "Task list <{0}> already exists.", listName ) )
						.SetPositiveButton( "Ok", ( buttonSender, buttonEvents ) => {} )
						.Create() ); 
				}
				else
				{
					if ( TaskRepository.CreateTaskList( listName ) == true )
					{
						// Re-initialise any items showing the list names
						InitialiseTaskListItem();
					}
				}
			});
		}

		/// <summary>
		/// Edit text changed delegate.
		/// </summary>
		private delegate void EditTextChangedDelegate( string listName );

		/// <summary>
		/// Shows the edit text dialogue.
		/// </summary>
		/// <param name="title">Title.</param>
		/// <param name="editTextDefault">Edit text default.</param>
		/// <param name="changedDelegate">Changed delegate.</param>
		private void ShowEditTextDialogue( string title, string editTextDefault, EditTextChangedDelegate changedDelegate )
		{
			// Get the layout that contains our EditText and initialise its text
			View contentView = LayoutInflater.Inflate( Resource.Layout.DialogueEditText, null );

			EditText newNameEdit = contentView.FindViewById< EditText >( Resource.Id.textView );
			newNameEdit.Text = editTextDefault;

			// Display the keyboard when the dialogue is displayed. The EditText view will be given the focus by the framework, so just need to wait 
			// for that to happen
			newNameEdit.PostDelayed( () => 
			{
				( ( InputMethodManager )GetSystemService( Context.InputMethodService ) ).ShowSoftInput( newNameEdit, ShowFlags.Implicit );
			}, 200 );

			// Build the dialogue
			ShowDialogue( new AlertDialog.Builder( this )
				.SetTitle( title )
				.SetView( contentView )
				.SetPositiveButton( "OK", ( buttonSender, buttonEvents ) =>
				{
					// Check if the name has changed
					if ( newNameEdit.Text != editTextDefault )
					{
						changedDelegate( newNameEdit.Text );
					}
				} )
				.SetNegativeButton( "Cancel", ( buttonSender, buttonEvents ) => {} )
				.Create() ); 
		}

		/// <summary>
		/// The user has selected a task list to delete
		/// Confirm the deletion with the user
		/// </summary>
		private void OnDeleteList( string taskListName )
		{
			// Form a list of those widgets displaying the task list
			List< int > widgetsDisplayingList = new List< int >();
			foreach( KeyValuePair< int, string > item in ListNamePersistence.GetItems( this ) )
			{
				if ( item.Value == taskListName )
				{
					widgetsDisplayingList.Add( item.Key );
				}
			}

			// Set the confirmation string according to whether or not the list is being displayed
			string message = "";
			if ( widgetsDisplayingList.Count == 0 )
			{
				message = string.Format( "Do you really want to delete list <{0}>?", taskListName );
			}
			else
			{
				message = string.Format( "List <{0}> is currently being displayed. Do you really want to delete it?", taskListName );
			}

			// Display the dialogue
			ShowDialogue( new AlertDialog.Builder( this )
				.SetMessage( message )
				.SetPositiveButton( "Yes", ( buttonSender, buttonEvents ) =>
				{
					// Delete the list
					if ( TaskRepository.DeleteTaskList( taskListName ) == true )
					{
						// Clear the associations for this task list name
						foreach( int widgetId in widgetsDisplayingList )
						{
							ListNamePersistence.SetListName( this, widgetId, "" );
						}

						// Re-initialise any lists showing the task list names
						InitialiseTaskListItem();

						// Tell the widgets
						SendBroadcast( new WidgetIntent( AutoNagWidget.ListDeletedAction ) );
					}
				} )
				.SetNegativeButton( "No", ( buttonSender, buttonEvents ) => {} )
				.Create() ); 
		}

		/// <summary>
		/// Called when the user wishes to change the colour of a task list.
		/// Display a dialogue showing all the possible colours
		/// </summary>
		/// <param name="taskListName">Task list name.</param>
		private void OnColourList( string taskListName )
		{
			// Create a ColourAdapter to supply the colours to the list
			ColourAdapter adapter = new ColourAdapter( this );

			// Determine the index of the current colour
			int index = adapter.GetPosition( ListColourPersistence.GetListColour( taskListName ).ToString() );

			ShowDialogue( new AlertDialog.Builder( this )
				.SetTitle( string.Format( "Select colour for <{0}>", taskListName ) )
				.SetSingleChoiceItems( adapter, index, ( buttonSender, buttonEvents ) => {} )
				.SetPositiveButton( "Ok", ( buttonSender, buttonEvents ) => 
				{
					// Has the selection changed
					int checkedItem = ( ( AlertDialog )buttonSender ).ListView.CheckedItemPosition;
					if ( checkedItem != index )
					{
						ListColourPersistence.SetListColour( taskListName, ListColourHelper.StringToColourEnum( adapter.GetItem( checkedItem ) ) );

						// Re-initialise any lists showing the task list names
						InitialiseTaskListItem();

						// Tell the widget
						SendBroadcast( new WidgetIntent( AutoNagWidget.ListColourAction ).SetTaskListName( taskListName ) );
					}
				} )
				.Create() ); 
		}

		/// <summary>
		/// Display the task list being displayed by the widget
		/// </summary>
		private void DisplayTaskListName()
		{
			currentListName = ListNamePersistence.GetListName( this, widgetIdentity );

			if ( currentListName.Length == 0 )
			{
				currentListPreference.ColourResourceProperty = ListColourHelper.NoTaskListColourProperty;
				currentListPreference.Title = "None";
			}
			else
			{
				currentListPreference.ColourResourceProperty = ListColourHelper.GetColourResource( currentListName );
				currentListPreference.Title = currentListName;
			}
		}

		/// <summary>
		/// Shows the dialogue and records the dialogue being shown in order to dismiss it.
		/// </summary>
		/// <param name="dialogueToShow">Dialogue to show.</param>
		private void ShowDialogue( Dialog dialogueToShow )
		{
			dialogueBeingShown = dialogueToShow;
			dialogueToShow.Show();
			dialogueToShow = null;
		}

		// 
		// Private data
		//

		/// <summary>
		/// The widget identity that launched this activity
		/// </summary>
		private int widgetIdentity = 0;

		/// <summary>
		/// The curren tlist name preference item
		/// </summary>
		private CustomPreference currentListPreference = null;

		/// <summary>
		/// The name of the current list.
		/// </summary>
		private string currentListName = "";

		/// <summary>
		/// The list rename preference.
		/// </summary>
		private ListPreference renamePreference = null;

		/// <summary>
		/// The create list preference.
		/// </summary>
		private CustomPreference createPreference = null;

		/// <summary>
		/// The delete list.
		/// </summary>
		private ListPreference deletePreference = null;

		/// <summary>
		/// The change colour preference
		/// </summary>
		private ListPreference colourPreference = null;

		/// <summary>
		/// The available lists category.
		/// </summary>
		private PreferenceCategory availableListsCategory = null;

		/// <summary>
		/// Keep track of any dialogues being shown so that they can be dismissed on configuration change
		/// </summary>
		private Dialog dialogueBeingShown = null;
	}
}
