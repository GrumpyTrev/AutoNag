//
// Project:     AutoNag
// Task:        User Interface
// Filename:    AutoNagWidget.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The AutoNagWidget class controls the display of tasks within an AppWidget on the home screen.
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
using Android.App;
using Android.Appwidget;
using Android.Widget;
using System.Collections.Generic;


namespace AutoNag
{
	[BroadcastReceiver (Label = "@string/widgetName")]
	[IntentFilter (new string [] { "android.appwidget.action.APPWIDGET_UPDATE", UpdatedAction, LoadedAction, SortAction, ListChangedAction, ListRenamedAction })]
	[MetaData ( "android.appwidget.provider", Resource = "@xml/widgetprovider" )]
	/// <summary>
	/// The AutoNagWidget class controls the display of tasks within an AppWidget on the home screen.
	/// </summary>
	public class AutoNagWidget : AppWidgetProvider
	{
		/// <summary>
		/// This method is called when the BroadcastReceiver is receiving an Intent
		///  broadcast.
		/// </summary>
		/// <param name="context">The Context in which the receiver is running.</param>
		/// <param name="intent">The Intent being received.</param>
		public override void OnReceive( Context context, Intent intent ) 
		{
			// Get the identities of the widgets showing task lists
			AppWidgetManager appManager = AppWidgetManager.GetInstance( context );
			int[] appWidgetIds = appManager.GetAppWidgetIds( new ComponentName( context, Java.Lang.Class.FromType( typeof( AutoNagWidget ) ) ) );

			// Wrap up the intent to retrieve data
			WidgetIntent wrappedIntent = new WidgetIntent( intent );

			if ( intent.Action == UpdatedAction )
			{
				// Determine the task list name associated with each widget.
				// If it matches the updated list then notify the widget that its data has changed
				string taskListName = wrappedIntent.TaskListNameProperty;
				foreach ( int widgetId in appWidgetIds )
				{
					if ( ListNamePersistence.GetListName( context, widgetId ) == taskListName )
					{
						appManager.NotifyAppWidgetViewDataChanged( widgetId, Resource.Id.listView );
					}
				}
			}
			else if ( intent.Action == LoadedAction )
			{
				// Render the contents of the widget that triggered the load.
				int[] triggeringWidget = new int[ 1 ];
				triggeringWidget[ 0 ] = new WidgetIntent( intent ).WidgetIdProperty;

				OnUpdate( context, appManager, triggeringWidget );
			}
			else if ( intent.Action == SortAction )
			{
				// Update the SortAction class associated with the specific widget
				int widgetId = wrappedIntent.WidgetIdProperty;
				if ( widgetId != AppWidgetManager.InvalidAppwidgetId )
				{
					// Get the index of the sort icon that has been clicked on
					int iconIndex = wrappedIntent.IconIndexProperty;
					if ( iconIndex != -1 )
					{
						// Process the click event associated with the icon
						SortOrder.ProcessClickEvent( context, widgetId, iconIndex );

						// Update the sort icons
						appManager.UpdateAppWidget( widgetId, RenderWidgetContents( context, widgetId ) );

						// Force the redisplaying of the data
						appManager.NotifyAppWidgetViewDataChanged( widgetId, Resource.Id.listView );
					}
				}
			}
			else if ( intent.Action == ListChangedAction )
			{
				// Redraw the header of the associated widget and force it to reload its tasks
				int widgetId = wrappedIntent.WidgetIdProperty;
				appManager.UpdateAppWidget( widgetId, RenderWidgetContents( context, widgetId ) );
				appManager.NotifyAppWidgetViewDataChanged( widgetId, Resource.Id.listView );
			}
			else if ( intent.Action == ListRenamedAction )
			{
				// Redraw the header of any widgets associated with the new task list
				string taskListName = wrappedIntent.TaskListNameProperty;
				foreach ( int widgetId in appWidgetIds )
				{
					if ( ListNamePersistence.GetListName( context, widgetId ) == taskListName )
					{
						appManager.UpdateAppWidget( widgetId, RenderWidgetContents( context, widgetId ) );
					}
				}
			}

			base.OnReceive(context, intent);
		}

		/// <summary>
		/// Render the visual content for all instances of this widget
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="appWidgetManager">App widget manager.</param>
		/// <param name="appWidgetIds">App widget identifiers.</param>
		public override void OnUpdate( Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds )
		{
			// Render each of the app widgets
			foreach ( int widgetId in appWidgetIds )
			{
				appWidgetManager.UpdateAppWidget( widgetId, RenderWidgetContents( context, widgetId ) );   
			}

			base.OnUpdate( context, appWidgetManager, appWidgetIds );
		}

		//
		// Public data constants
		//

		public const string LoadedAction = "AutoNag.LOADED_ACTION";
		public const string UpdatedAction = "AutoNag.UPDATED_ACTION";
		public const string SortAction = "AutoNag.SORT_ACTION";
		public const string ListChangedAction = "AutoNag.LIST_CHANGED_ACTION";
		public const string ListRenamedAction = "AutoNag.LIST_RENAMED_ACTION";

		//
		// Private methods
		//

		/// <summary>
		/// Renders the visual contents of a specific widget contents into a RemoteViews object
		/// </summary>
		/// <param name="widgetContext">Widget context.</param>
		/// <param name="widgetId">Widget identifier.</param>
		private RemoteViews RenderWidgetContents( Context widgetContext, int widgetId )
		{
			// If there is no list name associated with this Widget then select one from those available in the database
			string taskListName = ListNamePersistence.GetListName( widgetContext, widgetId );
			if ( taskListName.Length == 0 )
			{
				// Get the list names from the database
				IList< string > listNames = TaskRepository.GetTaskTables();

				if ( listNames.Count > 0 )
				{	
					// Use the first list name
					taskListName = listNames[ 0 ];
					ListNamePersistence.SetListName( widgetContext, widgetId, taskListName );
				}
			}

			// Instantiate the RemoteViews object for the app widget layout.
			RemoteViews views = new RemoteViews( widgetContext.PackageName, Resource.Layout.WidgetLayout );

			// Display the sort icons and pending intents
			SortOrder.DisplayIcons( views, widgetContext, widgetId, sortOrderItems, iconIds );

			// Set up the intent that starts the UpdateService, which will provide the views for the tasks collection.
			// Set up the RemoteViews object to use a RemoteViews adapter. 
			// This adapter connects to a RemoteViewsService through the specified intent.
			// This is how you populate the data.
			views.SetRemoteAdapter( widgetId, Resource.Id.listView, new WidgetIntent( widgetContext, typeof( UpdateService ) ).SetWidgetId( widgetId ) );

			// The empty view is displayed when the collection has no items. 
			// It should be in the same layout used to instantiate the RemoteViews object above.
			views.SetEmptyView( Resource.Id.listView, Resource.Id.message );

			// Set up an intent template for the list view items to activate the TaskDetailsScreen activity
			// N.B. Use a unique 'requestCode' value for intents that activate the same activity to differentiate between them.
			// These must be unique for all PendingIntents that have the same type i.e. typeof( TaskDetailsScreen ). As it
			// is used for TaskID elsewhere don't use any positive integers
			views.SetPendingIntentTemplate( Resource.Id.listView, 
				PendingIntent.GetActivity( widgetContext, -1, 
					new WidgetIntent( widgetContext, typeof( TaskDetailsScreen ) ).SetWidgetId( widgetId ).SetTaskListName( taskListName ),
					PendingIntentFlags.UpdateCurrent ) );

			// Set up an intent to fire when the 'new' icon is clicked
			views.SetOnClickPendingIntent( Resource.Id.newTask, 
				PendingIntent.GetActivity( widgetContext, -2, 
					new WidgetIntent( widgetContext, typeof( TaskDetailsScreen ) ).SetWidgetId( widgetId ).SetTaskListName( taskListName ).SetTaskIdentity( 0 ),
					PendingIntentFlags.UpdateCurrent ) );

			// Set up an intent for the header text
			views.SetOnClickPendingIntent( Resource.Id.headerText, 
				PendingIntent.GetActivity( widgetContext, 2, new Intent( widgetContext, typeof( SettingsActivity ) )
					.PutExtra( AppWidgetManager.ExtraAppwidgetId, widgetId ), 
					PendingIntentFlags.UpdateCurrent ) );

			// Show task count. This has to be obtained from the static count kept by the ListRemoteViewsFactory class
			views.SetTextViewText( Resource.Id.headerText, string.Format( "{0} ({1})", taskListName, TaskCountPersistence.GetTaskCount( widgetContext, widgetId ) ) );

			return views;
		}

		//
		// Private data
		//

		/// <summary>
		/// SortOrder objects used to display sort order icons and respond to sort order changes.
		/// Indexed by widget identity.
		/// This needs to be static as the AutoNagWidget class does not persist
		/// </summary>
		private static Dictionary< int, SortOrder > sortOrderCollection = new Dictionary< int, SortOrder >();

		/// <summary>
		/// Common set of SortOrderItems for all SortOrder instances
		/// </summary>
		private readonly SortOrderItem[] sortOrderItems = new SortOrderItem[] { 
			new SortOrderItem( Resource.Drawable.SortDoneOff, Resource.Drawable.SortDoneOn, Task.SortOrders.Done, true ),
			new SortOrderItem( Resource.Drawable.SortStarOff, Resource.Drawable.SortStarOn, Task.SortOrders.Priority, true ),
			new SortOrderItem( Resource.Drawable.SortDueDateOff, Resource.Drawable.SortDueDateOn, Task.SortOrders.DueDate, true ) };

		/// <summary>
		/// The identities of the sort image views 
		/// </summary>
		private readonly int[] iconIds = new int[] { Resource.Id.sortDueDate, Resource.Id.sortPriority, Resource.Id.sortDone };
	}
}
