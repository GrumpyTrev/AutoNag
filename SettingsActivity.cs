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
				// The displayed list preference has changed.
				// Store the changed value, display and broadcast the change
				string taskListName = sharedPreferences.GetString( key, "" );
				ListNamePersistence.SetListName( ApplicationContext, widgetIdentity, taskListName );

				DisplayTaskListName( taskListName );

				SendBroadcast( new WidgetIntent( AutoNagWidget.ListChangedAction ).SetWidgetId( widgetIdentity ) );
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
			widgetIdentity = Intent.GetIntExtra( AppWidgetManager.ExtraAppwidgetId, AppWidgetManager.InvalidAppwidgetId );

			// Get a reference to the preference item displaying the task list
			PreferenceScreen screen = ( PreferenceScreen )FindPreference( GetString( Resource.String.settingsKey ) );
			taskList = ( ListPreference )screen.FindPreference( GetString( Resource.String.listSettingsKey ) );

			// Initialise the task list preference item and display the currently displayed task list
			InitialiseTaskListItem();
		}

		/// <summary>
		/// Raises the resume event.
		/// </summary>
		protected override void OnResume()
		{
			base.OnResume();

			// Listen to changes
			PreferenceManager.GetDefaultSharedPreferences( this ).RegisterOnSharedPreferenceChangeListener( this );
		}

		/// <summary>
		/// Called as part of the activity lifecycle when an activity is going into
		///  the background, but has not (yet) been killed.
		/// </summary>
		protected override void OnPause()
		{
			base.OnPause();

			PreferenceManager.GetDefaultSharedPreferences( this ).UnregisterOnSharedPreferenceChangeListener( this );
		}

		//
		// Private methods
		//

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

			taskList.Title = string.Format( "{0} {1}", GetString( Resource.String.listSettingsTitle ), taskListName );
		}

		/// <summary>
		/// Initialises the task list item.
		/// </summary>
		private void InitialiseTaskListItem()
		{
			// Initialise the collection of available task lists by interrogating the database and pass to the displayedList screen
			IList< string > availableListNames = TaskRepository.GetTaskTables();
			string[] availableListStrings = new string[ availableListNames.Count ];
			availableListNames.CopyTo( availableListStrings, 0 );

			taskList.SetEntries( availableListStrings );
			taskList.SetEntryValues( availableListStrings );

			taskList.DialogTitle = GetString( Resource.String.chooseListTitle );

			// If the currently displayed task list name is known then select it
			string taskListName = ListNamePersistence.GetListName( ApplicationContext, widgetIdentity );
			if ( taskListName.Length > 0 )
			{
				taskList.Value = taskListName;
			}

			// Display the task list name
			DisplayTaskListName( taskListName );
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
	}
}

