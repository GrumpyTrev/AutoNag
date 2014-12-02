// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        Persistence
// Filename:    SortOrderPersistence.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The SortOrderPersistence class controls the persistence of sort order information.
//				 The SortOrderState class represents a single task sort order name and whether or not it is turned on
//
// Description:  The sort order information consists of an ordered set of task sort order names and their on/off states
//
//
//
// File History
// ------------
//
// %version:  1 %
//
// (c) Copyright 2014 Trevor Simmonds.
// This software is protected by copyright, the design of any 
// article recorded in the software is protected by design 
// right and the information contained in the software is 
// confidential. This software may not be copied, any design 
// may not be reproduced and the information contained in the 
// software may not be used or disclosed except with the
// prior written permission of and in a manner permitted by
// the proprietors Trevor Simmonds (c) 2014
//
//    Copyright Holders:
//       Trevor Simmonds,
//       t.simmonds@virgin.net
//
using System;
using System.Collections.Generic;
using Android.Content;

namespace AutoNag
{
	public class SortOrderPersistence
	{
		//
		// Public methods
		//

		/// <summary>
		/// Gets the sort order from the persistent store.
		/// </summary>
		/// <returns>The sort order.</returns>
		/// <param name="persistenceContext">Persistence context.</param>
		/// <param name="widgetId">Widget identifier.</param>
		public static List< SortOrderState > GetSortOrder( Context persistenceContext, int widgetId )
		{
			List< SortOrderState > sortOrders = new List<SortOrderState>();

			ISharedPreferences preferences = persistenceContext.GetSharedPreferences( PreferenceFileName, FileCreationMode.Private );

			// Get the list of Task.SortOrders names from the preferences store 
			// First get the number of items
			int numberOfItems = preferences.GetInt( string.Format( NumberOfItemsFormat, widgetId ), 0 );

			// Now get the names and state for each entry
			for ( int itemIndex = 0; itemIndex < numberOfItems; ++itemIndex )
			{
				string sortName = preferences.GetString( string.Format( NameFormat, widgetId, itemIndex ), "" );
				bool state = preferences.GetBoolean( string.Format( StateFormat, widgetId, itemIndex ), false );
				sortOrders.Add( new SortOrderState( ( Task.SortOrders )Enum.Parse( typeof( Task.SortOrders ), sortName ), state ) );
			}

			return sortOrders;
		}

		/// <summary>
		/// Persist the sort order collection
		/// </summary>
		/// <param name="persistenceContext">Persistence context.</param>
		/// <param name="widgetId">Widget identifier.</param>
		/// <param name="sortOrders">Sort orders.</param>
		public static void SetSortOrder( Context persistenceContext, int widgetId, List< SortOrderState > sortOrders )
		{
			ISharedPreferences preferences = persistenceContext.GetSharedPreferences( PreferenceFileName, FileCreationMode.Private );

			// Get an editor for this
			ISharedPreferencesEditor editor = preferences.Edit();

			// Write the item count first
			editor.PutInt( string.Format( NumberOfItemsFormat, widgetId ), sortOrders.Count );

			for ( int itemIndex = 0; itemIndex < sortOrders.Count; ++itemIndex )
			{
				editor.PutString( string.Format( NameFormat, widgetId, itemIndex ), sortOrders[ itemIndex ].NameProperty );
				editor.PutBoolean( string.Format( StateFormat, widgetId, itemIndex ), sortOrders[ itemIndex ].StateProperty );
			}

			// Commit the changes
			bool result = editor.Commit();
		}

		//
		// Private methods
		//

		/// <summary>
		/// Private constructor
		/// </summary>
		private SortOrderPersistence()
		{
		}

		//
		// Private data
		//

		/// <summary>
		/// The name of the preference file.
		/// </summary>
		private const string PreferenceFileName = "SortOrderPersistence";

		/// <summary>
		/// Format for the state and name items
		/// </summary>
		private const string NumberOfItemsFormat = "{0}ItemCount";
		private const string StateFormat = "{0}State{1}";
		private const string NameFormat = "{0}Name{1}";
	}

	/// <summary>
	/// The SortOrderState class represents a single task sort order name and whether or not it is turned on
	/// </summary>
	public class SortOrderState
	{
		//
		// Public methods
		//

		/// <summary>
		/// Initializes a new instance of the PersistedSortOrder class.
		/// </summary>
		/// <param name="sortOrder">Sort order.</param>
		/// <param name="sortOrderOn">If set to <c>true</c> sort order on.</param>
		public SortOrderState( Task.SortOrders sortOrder, bool sortOrderOn )
		{
			order = sortOrder;
			state = sortOrderOn;
		}

		/// <summary>
		/// Gets the string equivalent of the sort order name.
		/// </summary>
		/// <value>The name property.</value>
		public string NameProperty
		{
			get
			{
				return order.ToString();
			}
		}

		/// <summary>
		/// Gets the order property.
		/// </summary>
		/// <value>The order property.</value>
		public Task.SortOrders OrderProperty
		{
			get
			{
				return order;
			}
		}

		/// <summary>
		/// The state of this SortOrder
		/// </summary>
		/// <value><c>true</c> if state property; otherwise, <c>false</c>.</value>
		public bool StateProperty
		{
			get
			{
				return state;
			}

			set
			{
				state = value;
			}
		}

		//
		// Private methods
		//

		/// <summary>
		/// Private constructor
		/// </summary>
		private SortOrderState()
		{
		}

		//
		// Private data
		//

		/// <summary>
		/// The task sort order identifier
		/// </summary>
		private readonly Task.SortOrders order;

		/// <summary>
		/// Is this sort order turned on
		/// </summary>
		private bool state = false;
	}
}

