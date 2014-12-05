/*
 * Copyright (C) 2009 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;

using Android.Util;

using System.Collections.Generic;

namespace AutoNag
{
	[BroadcastReceiver (Label = "@string/widgetName")]
	[IntentFilter (new string [] { "android.appwidget.action.APPWIDGET_UPDATE", UpdatedAction, LoadedAction, SortAction, HelpAction })]
	[MetaData ( "android.appwidget.provider", Resource = "@xml/widgetprovider" )]
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
			AppWidgetManager appManager = AppWidgetManager.GetInstance( context );
			int[] appWidgetIds = appManager.GetAppWidgetIds( new ComponentName( context, Java.Lang.Class.FromType( typeof( AutoNagWidget ) ) ) );

			if ( intent.Action == UpdatedAction )
			{
				// Tell all widgets that the data has changed.
				// TODO - Target this to the widgets that are displaying the updated data
				appManager.NotifyAppWidgetViewDataChanged( appWidgetIds, Resource.Id.listView );
			}
			else if ( intent.Action == LoadedAction )
			{
				// Render the contents of the widgets in order to display the initial task count.
				// TODO - Target this to the specific widget that caused this load to occur
				OnUpdate( context, appManager, appWidgetIds );
			}
			else if ( intent.Action == SortAction )
			{
				// Update the SortAction class associated with the specific widget
				int widgetId = intent.GetIntExtra( AppWidgetManager.ExtraAppwidgetId, -1 );
				if ( widgetId != -1 )
				{
					// Get the index of the sort icon that has been clicked on
					int iconIndex = intent.GetIntExtra( "IconIndex", -1 );
					if ( iconIndex != -1 )
					{
						SortOrder.ProcessClickEvent( context, widgetId, iconIndex );

						appManager.UpdateAppWidget( widgetId, RenderWidgetContents( context, widgetId ) );

						// Force the redisplaying of the data
						appManager.NotifyAppWidgetViewDataChanged( widgetId, Resource.Id.listView );
					}
				}
			}
			else if ( intent.Action == HelpAction )
			{
				// Launch the TrackerControlDialogue
				context.StartActivity( new Intent( context, typeof( HelpDialogueActivity ) ).AddFlags( ActivityFlags.NewTask ) );
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
			// Update each of the app widgets
			for ( int index = 0; index < appWidgetIds.Length; ++index )
			{
				int appWidgetId = appWidgetIds[ index ];

				// Render this widget
				appWidgetManager.UpdateAppWidget( appWidgetId, RenderWidgetContents( context, appWidgetId ) );   
			}

			base.OnUpdate(context, appWidgetManager, appWidgetIds);
		}


		//
		// Public data constants
		//

		public const string LoadedAction = "com.example.android.stackwidget.LOADED_ACTION";
		public const string UpdatedAction = "com.example.android.stackwidget.UPDATED_ACTION";
		public const string SortAction = "com.example.android.stackwidget.SORT_ACTION";

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
			// Instantiate the RemoteViews object for the app widget layout.
			RemoteViews views = new RemoteViews( widgetContext.PackageName, Resource.Layout.WidgetLayout );

			// Display the sort icons and pending intents
			SortOrder.DisplayIcons( views, widgetContext, widgetId, sortOrderItems, iconIds );

			// Set up the intent that starts the UpdateService, which will provide the views for the tasks collection.
			// Set up the RemoteViews object to use a RemoteViews adapter. 
			// This adapter connects to a RemoteViewsService through the specified intent.
			// This is how you populate the data.
			views.SetRemoteAdapter( widgetId, Resource.Id.listView, new Intent( widgetContext, typeof( UpdateService ) )
				.PutExtra( AppWidgetManager.ExtraAppwidgetId, widgetId ) );

			// The empty view is displayed when the collection has no items. 
			// It should be in the same layout used to instantiate the RemoteViews object above.
			views.SetEmptyView( Resource.Id.listView, Resource.Id.message );

			// Set up an intent template for the list view items to activate the TaskDetailsScreen activity
			// N.B. Use a unique 'requestCode' value for intents that activate the same activity to differentiate between them.
			// These must be unique for all PendingIntents that have the same type i.e. typeof( TaskDetailsScreen ). As it
			// is used for TaskID elsewhere don't use any positive integers
			views.SetPendingIntentTemplate( Resource.Id.listView, 
				PendingIntent.GetActivity( widgetContext, -1, new Intent( widgetContext, typeof( TaskDetailsScreen ) )
					.PutExtra( AppWidgetManager.ExtraAppwidgetId, widgetId ), 
				PendingIntentFlags.UpdateCurrent ) );

			// Set up an intent to fire when the 'new' icon is clicked
			views.SetOnClickPendingIntent( Resource.Id.newTask, 
				PendingIntent.GetActivity( widgetContext, -2, new Intent( widgetContext, typeof( TaskDetailsScreen ) )
					.PutExtra( AppWidgetManager.ExtraAppwidgetId, widgetId )
					.PutExtra( "TaskID", 0 ), 
				PendingIntentFlags.UpdateCurrent ) );

			// Set up an intent for the header text
			views.SetOnClickPendingIntent( Resource.Id.headerText, 
				PendingIntent.GetBroadcast( widgetContext, 2, new Intent( AutoNagWidget.HelpAction )
					.PutExtra( AppWidgetManager.ExtraAppwidgetId, widgetId ), 
				PendingIntentFlags.UpdateCurrent ) );

			// Show task count. This has to be obtained from the static count kept by the ListRemoteViewsFactory class
			views.SetTextViewText( Resource.Id.headerText, string.Format( "{0} ({1})", widgetContext.GetString( Resource.String.widgetName ), 
				ListRemoteViewsFactory.GetCount( widgetId ) ) );

			return views;
		}

		//
		// Private data
		//

		/// <summary>
		/// The help action.
		/// </summary>
		private const string HelpAction = "com.example.android.stackwidget.HELP_ACTION";

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
