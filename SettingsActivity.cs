// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        User Interface
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

namespace AutoNag
{
	/// <summary>
	/// The SettingsActivity activity displays the current configuration of an AutoNag widget.
	/// </summary>
	[Activity (Label="AutoNag settings") ]
	public class SettingsActivity : PreferenceActivity, ISharedPreferencesOnSharedPreferenceChangeListener
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public SettingsActivity()
		{
		}

		/// <summary>
		/// Raises the shared preference changed event.
		/// </summary>
		/// <param name="sharedPreferences">Shared preferences.</param>
		/// <param name="key">Key.</param>
		public void OnSharedPreferenceChanged( ISharedPreferences sharedPreferences, string key )
		{
			if ( key == GetString( Resource.String.listSettingsKey ) )
			{
				ProcessDisplayedListChange( sharedPreferences.GetString( key, "" ) );
			}
			else if ( key.StartsWith( RenameKeyPrefix ) == true )
			{
				ProcessTaskListNameChange( key );
			}
			else if ( key == GetString( Resource.String.createListKey ) )
			{
				ProcessCreateList();
			}
			else if ( key == GetString( Resource.String.deleteListKey ) )
			{
				ProcessDeleteList( sharedPreferences.GetString( key, "" ) );
			}
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
			taskList = ( ListPreference )screen.FindPreference( GetString( Resource.String.listSettingsKey ) );
			renameScreen = ( PreferenceScreen )screen.FindPreference( GetString( Resource.String.renameScreenKey ) );
			textPreference = ( EditTextPreference )screen.FindPreference( GetString( Resource.String.createListKey ) );
			deleteList = ( ListPreference )screen.FindPreference( GetString( Resource.String.deleteListKey ) );

			// Clear any preferenecs that may be left from the last time this activity was run
			taskList.Value = "";

			// Initialise any items that show task list names
			InitialiseItemsShowingTaskListNames();
		}

		/// <summary>
		/// Raises the resume event.
		/// </summary>
		protected override void OnResume()
		{
			base.OnResume();

			// Listen to changes
			RegisterListener();
		}

		/// <summary>
		/// Called as part of the activity lifecycle when an activity is going into
		///  the background, but has not (yet) been killed.
		/// </summary>
		protected override void OnPause()
		{
			base.OnPause();

			// Stop listening to changes
			DeregisterListener();
		}

		//
		// Private methods
		//

		/// <summary>
		/// Initialises the items showing task list names.
		/// </summary>
		private void InitialiseItemsShowingTaskListNames()
		{
			IList< string > availableListNames = TaskRepository.GetTaskTables();

			InitialiseTaskListItem( availableListNames );
			InitialiseRenameScreen( availableListNames );
		}


		/// <summary>
		/// Initialises the display task list and delete list items.
		/// </summary>
		private void InitialiseTaskListItem( IList< string > availableListNames )
		{
			// Initialise the collection of available task lists
			string[] availableListStrings = new string[ availableListNames.Count ];
			availableListNames.CopyTo( availableListStrings, 0 );

			// Initialise the display task list
			taskList.SetEntries( availableListStrings );
			taskList.SetEntryValues( availableListStrings );

			taskList.DialogTitle = GetString( Resource.String.chooseListTitle );

			// If the currently displayed task list name is known then select it
			string taskListName = ListNamePersistence.GetListName( this, widgetIdentity );
			if ( taskListName.Length > 0 )
			{
				taskList.Value = taskListName;
			}

			// Display the task list name
			DisplayTaskListName( taskListName );

			// Initialise the delete list
			deleteList.SetEntries( availableListStrings );
			deleteList.SetEntryValues( availableListStrings );

			deleteList.DialogTitle = GetString( Resource.String.deleteListDialogTitle );
		}

		/// <summary>
		/// Initialises the rename screen.
		/// </summary>
		/// <param name="availableListNames">Available list names.</param>
		private void InitialiseRenameScreen( IList< string > availableListNames )
		{
			renameScreen.RemoveAll();

			// Add an EditPreference item to the screen for every task list name
			foreach( string listName in availableListNames )
			{
				EditTextPreference textPreference = new EditTextPreference( this );

				string key = string.Format( "{0} <{1}>", RenameKeyPrefix, listName );
				textPreference.Title = listName;
				textPreference.Key = key;
				textPreference.Text = listName;
				textPreference.DialogTitle = key;

				renameScreen.AddPreference( textPreference );
			}
		}

		/// <summary>
		/// The user has changed the name of a task list.
		/// Check that this is not a duplicate name and if unique update the table name in the database
		/// Update any ListNamePersistence items that contain this name and then inform the AutoNagWidget of the change 
		/// </summary>
		/// <param name="changedItemKey">Changed item key.</param>
		private void ProcessTaskListNameChange( string changedItemKey )
		{
			EditTextPreference textPreference = ( EditTextPreference )renameScreen.FindPreference( changedItemKey );
			if ( textPreference != null )
			{
				// Get the new and old names
				string newName = textPreference.Text;
				string oldName = textPreference.Title;

				// Check for unique new name
				if ( TaskRepository.GetTaskTables().Contains( newName ) == true )
				{
					// A list with this name already exists
					// Set the default value for this item back to the original name
					// Must do this with notifications turned off
					DeregisterListener();
					textPreference.Text = oldName;
					RegisterListener();

					// Tell the user
					new AlertDialog.Builder( this )
						.SetMessage( string.Format( "Task list <{0}> already exists.", newName ) )
						.SetPositiveButton( "Ok", ( buttonSender, buttonEvents ) => {} )
						.Show(); 
				}
				else
				{
					// Attempt the rename
					if ( TaskRepository.RenameTaskList( oldName, newName ) == true )
					{
						bool anyUpdatesMade = false;

						// Get all the ListNamePersistence entries
						foreach( KeyValuePair< int, string > item in ListNamePersistence.GetItems( this ) )
						{
							// Update any entries with the old list name
							if ( item.Value == oldName )
							{
								ListNamePersistence.SetListName( this, item.Key, newName );
								anyUpdatesMade = true;
							}
						}

						if ( anyUpdatesMade == true )
						{
							// Re-initialise any items showing the list names
							InitialiseItemsShowingTaskListNames();

							// Tell the widget
							SendBroadcast( new WidgetIntent( AutoNagWidget.ListRenamedAction ).SetTaskListName( newName ) );
						}
					}
				}
			}

		}

		/// <summary>
		/// The user has changed the list associated with the widget that initiated this activity
		/// Get the new list name. Store it and broadcast the change to the widget
		/// </summary>
		private void ProcessDisplayedListChange( string taskListName )
		{
			// Update the persisted association
			ListNamePersistence.SetListName( this, widgetIdentity, taskListName );

			// Display it 
			DisplayTaskListName( taskListName );

			// Tell the widget
			SendBroadcast( new WidgetIntent( AutoNagWidget.ListChangedAction ).SetWidgetId( widgetIdentity ) );
		}

		/// <summary>
		/// The user has supplied the name for a new task list.
		/// If the list name is unique create the associated table
		/// </summary>
		private void ProcessCreateList()
		{
			string listName = textPreference.Text;
			if ( TaskRepository.GetTaskTables().Contains( listName ) == true )
			{
				// A list with this name already exists
				// Tell the user
				new AlertDialog.Builder( this )
					.SetMessage( string.Format( "Task list <{0}> already exists.", listName ) )
					.SetPositiveButton( "Ok", ( buttonSender, buttonEvents ) => {} )
					.Show(); 
			}
			else
			{
				if ( TaskRepository.CreateTaskList( listName ) == true )
				{
					// Re-initialise any items showing the list names
					InitialiseItemsShowingTaskListNames();
				}
			}

			// Set the default value for this item back to an empty string
			// Must do this with notifications turned off
			DeregisterListener();
			textPreference.Text = "";
			RegisterListener();
		}

		/// <summary>
		/// The user has selected a task list to delete
		/// Confirm the deletion with the user
		/// </summary>
		private void ProcessDeleteList( string taskListName )
		{
			// Form a list of those widgets displaying the task list
			List< int > widgetsDisplayingList = new List<int>();
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
			new AlertDialog.Builder( this )
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

						// Tell the widgets
						if ( widgetsDisplayingList.Count > 0 )
						{
							SendBroadcast( new WidgetIntent( AutoNagWidget.ListDeletedAction ) );
						}

						// Re-initialise any lists showing the task list names
						InitialiseItemsShowingTaskListNames();
					}
				} )
				.SetNegativeButton( "No", ( buttonSender, buttonEvents ) => {} )
				.Show(); 

			// Set the default value for this item back to an empty string
			// Must do this with notifications turned off
			DeregisterListener();
			deleteList.Value = "";
			RegisterListener();
		}

		/// <summary>
		/// Display the task list being displayed by the widget
		/// </summary>
		/// <param name="taskListName">Task list name.</param>
		private void DisplayTaskListName( string taskListName )
		{
			if ( taskListName.Length == 0 )
			{
				taskListName = "None";
			}

			taskList.Title = string.Format( "{0} <{1}>", GetString( Resource.String.listSettingsTitle ), taskListName );
		}

		/// <summary>
		/// Registers the listener.
		/// </summary>
		private void RegisterListener()
		{
			PreferenceManager.GetDefaultSharedPreferences( this ).RegisterOnSharedPreferenceChangeListener( this );
		}

		/// <summary>
		/// Deregisters the listener.
		/// </summary>
		private void DeregisterListener()
		{
			PreferenceManager.GetDefaultSharedPreferences( this ).UnregisterOnSharedPreferenceChangeListener( this );
		}

		// 
		// Private data
		//

		/// <summary>
		/// The widget identity that launched this activity
		/// </summary>
		private int widgetIdentity = 0;

		/// <summary>
		/// The task list preference item
		/// </summary>
		private ListPreference taskList = null;

		/// <summary>
		/// The list rename preference screen.
		/// </summary>
		private PreferenceScreen renameScreen = null;

		/// <summary>
		/// The text preference.
		/// </summary>
		private EditTextPreference textPreference = null;

		/// <summary>
		/// The delete list.
		/// </summary>
		private ListPreference deleteList = null;

		/// <summary>
		/// The prefix for all rename items
		/// </summary>
		private const string RenameKeyPrefix = "Rename";
	}
}

