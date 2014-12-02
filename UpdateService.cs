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

using System;
using System.Collections.Generic;

using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;
using Android.Views;

using Android.Util;

namespace AutoNag
{
	[Service(Permission = "android.permission.BIND_REMOTEVIEWS", Exported = false)]
	public class UpdateService : RemoteViewsService 
	{
		public override IRemoteViewsFactory OnGetViewFactory( Intent itemIntent )
		{
			return new ListRemoteViewsFactory( this.ApplicationContext, itemIntent );
		}
	}

	public class ListRemoteViewsFactory : Java.Lang.Object, RemoteViewsService.IRemoteViewsFactory
	{
		public ListRemoteViewsFactory( Context appContext, Intent itemIntent )
		{
			savedContext = appContext;

			widgetIdentity = itemIntent.GetIntExtra( AppWidgetManager.ExtraAppwidgetId, AppWidgetManager.InvalidAppwidgetId );
		}

		public void OnCreate()
		{
			LoadTasks();
		}

		public void OnDestroy()
		{
			tasks.Clear();
		}

		public int Count
		{
			get
			{
				return tasks.Count;
			}
		}

		public RemoteViews GetViewAt( int position )
		{
			RemoteViews view = new RemoteViews( savedContext.PackageName, Resource.Layout.WidgetItem );
			view.SetTextViewText( Resource.Id.taskName, tasks[ position ].Name );
			view.SetTextViewText( Resource.Id.taskNote, tasks[ position ].Notes );
			view.SetTextViewText( Resource.Id.lastChangedDate, tasks[ position ].ModifiedDate.ToString( @"dd/MM/yyyy" ) );

			if ( tasks[ position ].Done == true )
			{
				view.SetInt( Resource.Id.overlay, "setBackgroundResource", Resource.Drawable.DoneBackground );
				view.SetViewVisibility( Resource.Id.taskDone, ViewStates.Visible );
				view.SetTextColor( Resource.Id.taskName, savedContext.Resources.GetColor( Resource.Color.taskDoneText ) );
			}
			else
			{
				view.SetInt( Resource.Id.overlay, "setBackgroundResource", Resource.Drawable.NotDoneBackground );
				view.SetViewVisibility( Resource.Id.taskDone, ViewStates.Invisible );
				view.SetTextColor( Resource.Id.taskName, savedContext.Resources.GetColor( Resource.Color.taskNormalText ) );
			}

			if ( tasks[ position ].Priority == 0 )
			{
				view.SetImageViewResource( Resource.Id.taskPriority, Resource.Drawable.StarOff );
			}
			else
			{
				view.SetImageViewResource( Resource.Id.taskPriority, Resource.Drawable.StarOn );
			}

			if ( tasks[ position ].DueDate == DateTime.MinValue )
			{
				view.SetViewVisibility( Resource.Id.taskDueLabel, ViewStates.Invisible );
				view.SetViewVisibility( Resource.Id.taskDue, ViewStates.Invisible );
				view.SetViewVisibility( Resource.Id.taskNotification, ViewStates.Invisible );
			}
			else
			{
				view.SetViewVisibility( Resource.Id.taskDueLabel, ViewStates.Visible );

				view.SetViewVisibility( Resource.Id.taskDue, ViewStates.Visible );
				view.SetTextViewText( Resource.Id.taskDue, tasks[ position ].DueDate.ToString( @"dd/MM/yyyy" ) );

				if ( tasks[ position ].NotificationRequired == false )
				{
					view.SetViewVisibility( Resource.Id.taskNotification, ViewStates.Invisible );
				}
				else
				{
					view.SetViewVisibility( Resource.Id.taskNotification, ViewStates.Visible );
				}
			}

			// Next, set a fill-intent, which will be used to fill in the pending intent template
			// that is set on the collection view in StackWidgetProvider.
			Intent fillInIntent = new Intent();
			fillInIntent.PutExtra( "TaskID", tasks[ position ].ID );

			view.SetOnClickFillInIntent( Resource.Id.overlay, fillInIntent );

			return view;
		}

		public long GetItemId( int position )
		{
			return position;
		}

		public void OnDataSetChanged()
		{
			LoadTasks();
		}

		public static int GetCount( int widgetId )
		{
			int count = -1;

			if ( taskCounts.ContainsKey( widgetId ) == true )
			{
				count = taskCounts[ widgetId ];
			}

			return count;
		}

		public bool HasStableIds
		{
			get
			{
				return true;
			}
		}

		public RemoteViews LoadingView
		{
			get
			{
				return null;
			}
		}

		public int ViewTypeCount
		{
			get
			{
				return 1;
			}
		}
			
		/// <summary>
		/// Load the tasks, update the task count and broadcast the 'loaded' message
		/// </summary>
		private void LoadTasks()
		{
			// Load the tasks with optional sort order
			List< Task.SortOrders > sortOrder = SortOrder.GetTaskSortOrder( savedContext, widgetIdentity );

			tasks = TaskManager.GetTasks( sortOrder );

			SetTaskCount( widgetIdentity, tasks.Count );

			savedContext.SendBroadcast( new Intent( AutoNagWidget.LoadedAction ) );
		}

		private static void SetTaskCount( int widgetId, int count )
		{
			taskCounts[ widgetId ] = count;
		}

		private readonly Context savedContext = null;

		private readonly int widgetIdentity = 0;

		private IList<Task> tasks;

		/// <summary>
		/// Static record of the current task count associated with each widget
		/// </summary>
		private static Dictionary< int, int > taskCounts = new Dictionary<int, int>();
	}
}
