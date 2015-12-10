//
// Project:     AutoNag
// Task:        User Interface
// Filename:    SortOrder.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The SortOrder class determines the order in which individual tasks are displayed in the Widget
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

using Android.Content;
using System.Collections.Generic;
using Android.Widget;
using Android.App;


namespace AutoNag
{
	/// <summary>
	/// The SortOrder class determines the order in which individual tasks are displayed in the Widget
	/// </summary>
	public class SortOrder
	{
		/// <summary>
		/// Processes the click event.
		/// Toggle the state of the selected SortOrderState and reorder the collection.
		/// </summary>
		/// <param name="clickContext">Click context.</param>
		/// <param name="widgetId">Widget identifier.</param>
		/// <param name="iconIndex">Icon index.</param>
		public static void ProcessClickEvent( Context clickContext, int widgetId, int iconIndex )
		{
			// Get the ordered set of SortOrderState items
			List < SortOrderState > sortStates = SortOrderPersistence.GetSortOrder( clickContext, widgetId );

			// Is it a valid icon
			if ( iconIndex < sortStates.Count )
			{
				SortOrderState item = sortStates[ iconIndex ];

				// Toggle the item on/off state
				item.StateProperty = !item.StateProperty;

				// Remove the item and insert after the last 'on' item
				sortStates.RemoveAt( iconIndex );

				int checkIconIndex = 0;
				bool itemMoved = false;
				while ( ( itemMoved == false ) && ( checkIconIndex < sortStates.Count ) )
				{
					if ( sortStates[ checkIconIndex ].StateProperty == false )
					{
						// An 'off' item has been found. Insert the changed item at this position
						itemMoved = true;
						sortStates.Insert( checkIconIndex, item );
					}
					else
					{
						++checkIconIndex;
					}
				}

				// If not 'off' item was found then add the changed item to the end
				if ( itemMoved == false )
				{
					sortStates.Add( item );
				}

				// Persist the updated sort orders
				SortOrderPersistence.SetSortOrder( clickContext, widgetId, sortStates );
			}
		}

		/// <summary>
		/// Display the sort icons in the correct order and correct image.
		/// </summary>
		/// <param name="views">Views.</param>
		/// <param name="displayContext">Display context.</param>
		/// <param name="widgetId">Widget identifier.</param>
		/// <param name="orderItems">Order items.</param>
		/// <param name="iconIds">Icon identifiers.</param>
		public static void DisplayIcons( RemoteViews views, Context displayContext, int widgetId, SortOrderItem[] orderItems, int[] iconIds )
		{
			// Check whether or not this is the first time the icons have been displayed
			List < SortOrderState > sortStates = SortOrderPersistence.GetSortOrder( displayContext, widgetId );

			if ( sortStates.Count == 0 )
			{
				// Create a List < SortOrderState > from the SortOrderItem[] items and persist them. Use the initial item state.
				foreach ( SortOrderItem item in orderItems )
				{
					sortStates.Add( new SortOrderState( item.OrderTypeProperty, item.OrderOnProperty ) );
				}

				SortOrderPersistence.SetSortOrder( displayContext, widgetId, sortStates );
			}

			// Generate a lookup table from sort order name to index in the SortOrderItem collection
			Dictionary< string, int > lookup = new Dictionary< string, int >();
			for ( int iconIndex = 0; iconIndex < orderItems.Length; ++iconIndex )
			{
				lookup[ orderItems[ iconIndex ].OrderTypeProperty.ToString() ] = iconIndex;
			}

			// Lookup each item in the SortOrderState and set the associated view.
			for ( int itemIndex = 0; itemIndex < sortStates.Count; ++itemIndex )
			{
				SortOrderState orderState = sortStates[ itemIndex ];
			
				if ( lookup.ContainsKey( orderState.NameProperty ) == true )
				{
					// Access the SortOrderItem associated with the sort name
					SortOrderItem item = orderItems[ lookup[ orderState.NameProperty ] ];

					// Set the image associated with the SortOrderItem in the correct position
					item.SetImage( views, iconIds[ itemIndex ], orderState.StateProperty );

					// Setup a click handler. Must use a WidgetIntent request code here to make it unique to a specific widget
					views.SetOnClickPendingIntent( iconIds[ itemIndex ], 
						PendingIntent.GetBroadcast( displayContext, WidgetIntent.GetRequestCode( widgetId, itemIndex ),
								new WidgetIntent( AutoNagWidget.SortAction ).SetWidgetId( widgetId ).SetIconIndex( itemIndex ), 
						PendingIntentFlags.UpdateCurrent ) );
				}
			}
		} 

		/// <summary>
		/// Cancel any handlers set up for the sort icons
		/// </summary>
		/// <param name="displayContext">Display context.</param>
		/// <param name="widgetId">Widget identifier.</param>
		/// <param name="noOfItems">No of items.</param>
		public static void CancelHandlers( Context displayContext, int widgetId, int noOfItems )
		{
			for ( int itemIndex = 0; itemIndex < noOfItems; ++itemIndex )
			{
				PendingIntent.GetBroadcast( displayContext, WidgetIntent.GetRequestCode( widgetId, itemIndex ),
					new WidgetIntent( AutoNagWidget.SortAction ), PendingIntentFlags.UpdateCurrent ).Cancel();
			}
		}

		/// <summary>
		/// Gets the task sort order.
		/// </summary>
		/// <returns>The task sort order.</returns>
		public static List< Task.SortOrders > GetTaskSortOrder( Context orderContext, int widgetId )
		{
			List< Task.SortOrders > taskSortOrder = new List<Task.SortOrders>();

			// Get the ordered set of SortOrderState items and iterate over it
			foreach ( SortOrderState orderState in SortOrderPersistence.GetSortOrder( orderContext, widgetId ) )
			{
				if ( orderState.StateProperty == true )
				{
					taskSortOrder.Add( orderState.OrderProperty );
				}
			}

			return taskSortOrder;
		}

		//
		// Private methods
		//

		/// <summary>
		/// Private constructor
		/// </summary>
		private SortOrder()
		{
		}
	}

	/// <summary>
	/// The SortOrderItem structure contains references to the visual elements necessary to display the icon
	/// associated with a particular sort order
	/// </summary>
	public class SortOrderItem
	{
		/// <summary>
		/// Initializes a new instance of the SortOrderItem class.
		/// </summary>
		/// <param name="offImage">Off image.</param>
		/// <param name="onImage">On image.</param>
		/// <param name="sortOrderType">Sort order type.</param>
		/// <param name="initialOrderOn">If set to <c>true</c> initial order on.</param>
		public SortOrderItem( int offImage, int onImage, Task.SortOrders sortOrderType, bool initialOrderOn )
		{
			orderOnImage = onImage;
			orderOffImage = offImage;
			orderType = sortOrderType;
			orderOn = initialOrderOn;
		}

		/// <summary>
		/// Set the image according to the sort state
		/// </summary>
		/// <param name="views">Views.</param>
		/// <param name="orderIcon">Order icon.</param>
		/// <param name="state">If set to <c>true</c> state.</param>
		public void SetImage( RemoteViews views, int orderIcon, bool state )
		{
			// Display the correct image
			views.SetImageViewResource( orderIcon, ( state == true ) ? orderOnImage : orderOffImage );
		}

		/// <summary>
		/// Sort order state
		/// </summary>
		/// <value><c>true</c> if order on property; otherwise, <c>false</c>.</value>
		public bool OrderOnProperty
		{
			get
			{
				return orderOn;
			}
		}

		/// <summary>
		/// Gets the order type property.
		/// </summary>
		/// <value>The order type property.</value>
		public Task.SortOrders OrderTypeProperty
		{
			get
			{
				return orderType;
			}
		}

		/// <summary>
		/// Private constructor
		/// </summary>
		private SortOrderItem()
		{
		}

		/// <summary>
		/// Resource identity for the sort on icon
		/// </summary>
		private readonly int orderOnImage;

		/// <summary>
		/// Resource identity for the sort off icon
		/// </summary>
		private readonly int orderOffImage;

		/// <summary>
		/// Is this item on or off
		/// </summary>
		private bool orderOn;

		/// <summary>
		/// The type of the order.
		/// </summary>
		private readonly Task.SortOrders orderType;
	}
}

