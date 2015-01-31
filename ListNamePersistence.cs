// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        Persistence
// Filename:    ListNamePersistence.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The ListNamePersistence class controls the persistence of the list name associated with a widget.
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

namespace AutoNag
{
	/// <summary>
	/// The ListNamePersistence class controls the persistence of the list name associated with a widget.
	/// </summary>
	public class ListNamePersistence
	{
		//
		// Public methods
		//

		/// <summary>
		/// Gets the list name from the persistent store.
		/// </summary>
		/// <returns>The sort order.</returns>
		/// <param name="persistenceContext">Persistence context.</param>
		/// <param name="widgetId">Widget identifier.</param>
		public static string GetListName( Context persistenceContext, int widgetId )
		{
			return persistenceContext.GetSharedPreferences( PreferenceFileName, FileCreationMode.Private )
				.GetString( string.Format( ListNameFormat, widgetId ), "" );
		}

		/// <summary>
		/// Persist the list name
		/// </summary>
		/// <param name="persistenceContext">Persistence context.</param>
		/// <param name="widgetId">Widget identifier.</param>
		/// <param name="listName">List name</param>
		public static void SetListName( Context persistenceContext, int widgetId, string listName )
		{
			persistenceContext.GetSharedPreferences( PreferenceFileName, FileCreationMode.Private )
				.Edit()
				.PutString( string.Format( ListNameFormat, widgetId ), listName )
				.Commit();
		}

		//
		// Private methods
		//

		/// <summary>
		/// Private constructor
		/// </summary>
		private ListNamePersistence()
		{
		}

		//
		// Private data
		//

		/// <summary>
		/// The name of the preference file.
		/// </summary>
		private const string PreferenceFileName = "ListNamePersistence";

		/// <summary>
		/// Format for the task count
		/// </summary>
		private const string ListNameFormat = "{0}ListName";
	}
}

