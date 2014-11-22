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

namespace AutoNag
{
	[BroadcastReceiver (Label = "@string/widgetName")]
	[IntentFilter (new string [] { "android.appwidget.action.APPWIDGET_UPDATE", UpdatedAction, LoadedAction })]
	[MetaData ("android.appwidget.provider", Resource = "@xml/widgetprovider")]
	public class AutoNagWidget : AppWidgetProvider
	{
		// Called when the BroadcastReceiver receives an Intent broadcast.
		// Checks to see whether the intent's action is TOAST_ACTION. If it is, the app widget 
		// displays a Toast message for the current item.
		public override void OnReceive( Context context, Intent intent ) 
		{
			AppWidgetManager mgr = AppWidgetManager.GetInstance( context );
			int[] appWidgetIds = mgr.GetAppWidgetIds( new ComponentName( context, Java.Lang.Class.FromType( typeof( AutoNagWidget ) ) ) );

			if ( intent.Action == UpdatedAction )
			{
				mgr.NotifyAppWidgetViewDataChanged( appWidgetIds, Resource.Id.listView );
			}
			else if ( intent.Action == LoadedAction )
			{
				OnUpdate( context, mgr, appWidgetIds );
			}

			base.OnReceive(context, intent);
		}


		public override void OnUpdate( Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds )
		{
			// Update each of the app widgets with the remote adapter
			for ( int index = 0; index < appWidgetIds.Length; ++index )
			{
				// Set up the intent that starts the UpdateService, which will provide the views for this collection.
				Intent intent = new Intent( context, typeof( UpdateService ) );

				// Add the app widget ID into the intent extras.
				intent.PutExtra( AppWidgetManager.ExtraAppwidgetId, appWidgetIds[ index ] );
				intent.SetData( Android.Net.Uri.Parse( intent.ToUri( Android.Content.IntentUriType.Scheme ) ) );

				// Instantiate the RemoteViews object for the app widget layout.
				RemoteViews views = new RemoteViews( context.PackageName, Resource.Layout.WidgetLayout );

				// Set up the RemoteViews object to use a RemoteViews adapter. 
				// This adapter connects to a RemoteViewsService through the specified intent.
				// This is how you populate the data.
				views.SetRemoteAdapter( appWidgetIds[ index ], Resource.Id.listView, intent );

				// The empty view is displayed when the collection has no items. 
				// It should be in the same layout used to instantiate the RemoteViews object above.
				views.SetEmptyView( Resource.Id.listView, Resource.Id.message );

				// Set up an intent template for the list view items to activate the TaskDetailsScreen activity
				Intent taskIntent = new Intent( context, typeof( TaskDetailsScreen ) );
				taskIntent.PutExtra( AppWidgetManager.ExtraAppwidgetId, appWidgetIds[ index ] );

				// N.B. Use a unique 'requestCode' value for intents that activate the same activity to differentiate between them
				views.SetPendingIntentTemplate( Resource.Id.listView, PendingIntent.GetActivity( context, 0, taskIntent, PendingIntentFlags.UpdateCurrent ) );

				// Set up an intent to fire when the 'new' icon is clicked
				Intent newTaskIntent = new Intent( context, typeof( TaskDetailsScreen ) );
				newTaskIntent.PutExtra( AppWidgetManager.ExtraAppwidgetId, appWidgetIds[ index ] );
				newTaskIntent.PutExtra( "TaskID", 0 );
				views.SetOnClickPendingIntent( Resource.Id.newTask, PendingIntent.GetActivity( context, 1, newTaskIntent, PendingIntentFlags.UpdateCurrent ) );

				// Show task count. This has to be obtained from the static count kept by the ListRemoteViewsFactory class
				views.SetTextViewText( Resource.Id.headerText, string.Format( "{0} ({1})", context.GetString( Resource.String.widgetName ), 
					ListRemoteViewsFactory.GetCount( appWidgetIds[ index ] ) ) );

				// Render these views on the widget
				appWidgetManager.UpdateAppWidget( appWidgetIds[ index ], views );   
			}

			base.OnUpdate(context, appWidgetManager, appWidgetIds);
		}

		public const string EXTRA_ITEM = "com.example.android.stackwidget.EXTRA_ITEM";
		public const string LoadedAction = "com.example.android.stackwidget.LOADED_ACTION";
		public const string UpdatedAction = "com.example.android.stackwidget.UPDATED_ACTION";
	}
}
