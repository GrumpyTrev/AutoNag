// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        User Interface
// Filename:    ListNamesDisplay.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The ListNamesDisplay class displays a collection of list names as part of the settings activity.
//
// Description:  The list names are displayed in a Preference instances and displayed within a Preference container
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
using System.Collections.Generic;
using Android.Content;

namespace AutoNag
{
	/// <summary>
	/// The ListNamesDisplay class displays a collection of list names as part of the settings activity.
	/// </summary>
	public class ListNamesDisplay
	{
		//
		// Public methods
		//

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoNag.ListNamesDisplay"/> class.
		/// </summary>
		/// <param name="container">Container.</param>
		public ListNamesDisplay( PreferenceGroup container, Context preferenceContext, CustomPreference.ListSelectedDelegate selectionDelegate )
		{
			containerGroup = container;
			containerContext = preferenceContext;
			listener = selectionDelegate;
		}

		/// <summary>
		/// Initialises the display.
		/// </summary>
		/// <param name="listNames">List names.</param>
		public void InitialiseDisplay( IList < string > listNames )
		{
			foreach ( string listName in listNames )
			{
				AddList( listName );
			}
		}

		/// <summary>
		/// Renames the list.
		/// </summary>
		/// <param name="oldName">Old name.</param>
		/// <param name="newName">New name.</param>
		public void RenameList( string oldName, string newName )
		{
			if ( preferenceCollection.ContainsKey( oldName ) == true )
			{
				CustomPreference preference = preferenceCollection[ oldName ];
				preference.Title = newName;
				preferenceCollection.Remove( oldName );
				preferenceCollection[ newName ] = preference;
			}
		}

		/// <summary>
		/// Changes the colour.
		/// </summary>
		/// <param name="listName">List name.</param>
		public void ChangeListColour( string listName )
		{
			if ( preferenceCollection.ContainsKey( listName ) == true )
			{
				preferenceCollection[ listName ].ColourResourceProperty = ListColourHelper.GetColourResource( listName );
			}
		}

		/// <summary>
		/// Deletes the list.
		/// </summary>
		/// <param name="listName">List name.</param>
		public void DeleteList( string listName )
		{
			if ( preferenceCollection.ContainsKey( listName ) == true )
			{
				CustomPreference preference = preferenceCollection[ listName ];
				containerGroup.RemovePreference( preference );
				containerGroup.GetView( null, null ).Invalidate();

				preferenceCollection.Remove( listName );
			}
		}

		/// <summary>
		/// Adds the list.
		/// </summary>
		/// <param name="listName">List name.</param>
		public void AddList( string listName )
		{
			CustomPreference preference = new CustomPreference( containerContext, OnSelected, ListColourHelper.GetColourResource( listName ), listName );
			containerGroup.AddPreference( preference );
			preferenceCollection[ listName ] = preference;
		}

		//
		// Private methods
		//

		/// <summary>
		/// Raises the selected event.
		/// </summary>
		/// <param name="taskListName">Task list name.</param>
		private void OnSelected( string taskListName )
		{
			if ( listener != null )
			{
				listener( taskListName );
			}
		}

		//
		// Private data
		//

		/// <summary>
		/// The container group.
		/// </summary>
		private readonly PreferenceGroup containerGroup = null;

		/// <summary>
		/// The container context.
		/// </summary>
		private readonly Context containerContext = null;

		/// <summary>
		/// The listener.
		/// </summary>
		private readonly CustomPreference.ListSelectedDelegate listener =  null;

		/// <summary>
		/// The preference collection.
		/// </summary>
		private Dictionary< string, CustomPreference > preferenceCollection = new Dictionary< string, CustomPreference >();
	}
}

