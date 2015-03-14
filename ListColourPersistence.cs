// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        Persistence
// Filename:    ListColourPersistence.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The ListColourPersistence class controls the persistence of the colour associated with a list.
//
// Description:  As purpose
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
using System;
using Android.Content;
using System.Collections;
using System.Collections.Generic;

namespace AutoNag
{
	/// <summary>
	/// The ListColourPersistence class controls the persistence of the colour associated with a list.
	/// </summary>
	public class ListColourPersistence
	{
		//
		// Public methods
		//

		/// <summary>
		/// Gets the colour associated with a task list.
		/// </summary>
		/// <returns>The colour of the list order.</returns>
		/// <param name="listName">The list name</param>
		public static ListColourEnum GetListColour( string listName )
		{
			// Load the collection if required
			LoadCollection();

			// Is there an entry for this list
			if ( listColourCollection.ContainsKey( listName ) == false )
			{
				// Add an entry with the default colour
				listColourCollection[ listName ] = ListColourHelper.DefaultColourEnumProperty;

				// And store
				TaskRepository.AddListColour( listName, ListColourHelper.DefaultColourEnumProperty );
			}

			return listColourCollection[ listName ];
		}

		/// <summary>
		/// Sets the list colour.
		/// </summary>
		/// <param name="listName">List name.</param>
		/// <param name="newColour">New colour.</param>
		public static void SetListColour( string listName, ListColourEnum newColour )
		{
			// Load the collection if required
			LoadCollection();

			// If there is an entry for the list then update it, otherwise create an entry
			if ( listColourCollection.ContainsKey( listName ) == false )
			{
				TaskRepository.AddListColour( listName, newColour );
			}
			else
			{
				TaskRepository.UpdateListColour( listName, newColour );
			}

			// Save in the cache
			listColourCollection[ listName ] = newColour;
		}

		//
		// Private methods
		//

		/// <summary>
		/// Private constructor
		/// </summary>
		private ListColourPersistence()
		{
		}

		/// <summary>
		/// Loads the collection.
		/// </summary>
		private static void LoadCollection()
		{
			// Has the collection been loaded yet?
			if ( listColourCollection == null )
			{
				// Create the collection and load it from the database
				listColourCollection = new Dictionary<string, ListColourEnum>();
				IDictionary< string, ListColourEnum > colourEntries = TaskRepository.GetListColours();

				foreach ( KeyValuePair< string, ListColourEnum > pair in colourEntries )
				{
					listColourCollection.Add( pair.Key, pair.Value );
				}
			}
		}

		//
		// Private data
		//

		/// <summary>
		/// The list colour collection.
		/// </summary>
		private static Dictionary< string, ListColourEnum > listColourCollection = null;
	}
}

