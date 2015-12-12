// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        TaskManagement
// Filename:    UpdateService.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The UpdateService class is a service wrapper around the ListRemoteViewsFactory class
//				 The ListRemoteViewsFactory class is used to display a collection of tasks in a remote list
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
using System.Collections.Generic;

using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;
using Android.Views;

using Android.Util;
using System.Text;

namespace AutoNag
{
	[Service(Permission = "android.permission.BIND_REMOTEVIEWS", Exported = false)]
	/// <summary>
	/// The UpdateService class is a service wrapper around the ListRemoteViewsFactory class
	/// </summary>
	public class UpdateService : RemoteViewsService 
	{
		/// <summary>
		/// To be implemented by the derived service to generate appropriate factories for
		/// the data.
		/// </summary>
		/// <param name="intent">To be added.</param>
		/// <returns>To be added.</returns>
		public override IRemoteViewsFactory OnGetViewFactory( Intent itemIntent )
		{
			return new ListRemoteViewsFactory( this.ApplicationContext, new WidgetIntent( itemIntent ) );
		}
	}

	/// <summary>
	/// The ListRemoteViewsFactory class is used to display a collection of tasks in a remote list
	/// </summary>
	public class ListRemoteViewsFactory : Java.Lang.Object, RemoteViewsService.IRemoteViewsFactory
	{
		/// <summary>
		/// Constructor - Extract the widget id and task list name from the intent
		/// </summary>
		/// <param name="appContext">App context.</param>
		/// <param name="itemIntent">Item intent.</param>
		public ListRemoteViewsFactory( Context appContext, WidgetIntent itemIntent )
		{
			savedContext = appContext;
			widgetIdentity = itemIntent.WidgetIdProperty;
		}

		/// <summary>
		/// Raises the create event.
		/// </summary>
		public void OnCreate()
		{
			// Load the tasks from the database
			LoadTasks();
		}

		/// <summary>
		/// Raises the destroy event.
		/// </summary>
		public void OnDestroy()
		{
			// Clear the tasks list
			tasks.Clear();
		}

		/// <summary>
		/// Gets the count.
		/// </summary>
		/// <value>The count.</value>
		public int Count
		{
			get
			{
				return tasks.Count;
			}
		}

		/// <summary>
		/// Gets the view at the specified position.
		/// </summary>
		/// <returns>The <see cref="Android.Widget.RemoteViews"/>.</returns>
		/// <param name="position">Position.</param>
		public RemoteViews GetViewAt( int position )
		{
			RemoteViews view = new RemoteViews( savedContext.PackageName, Resource.Layout.WidgetItem );
			Task taskToDisplay = tasks[ position ];

			view.SetTextViewText( Resource.Id.taskName, taskToDisplay.Name );
			view.SetTextViewText( Resource.Id.taskNote, taskToDisplay.Notes );
			view.SetTextViewText( Resource.Id.lastChangedDate, taskToDisplay.ModifiedDate.ToString( DateDisplayFormat ) );

			if ( taskToDisplay.Done == true )
			{
				// Give it a dimmed background
				view.SetInt( Resource.Id.overlay, BackgroundResourceMethodName, Resource.Color.itemDoneBackground );
				view.SetViewVisibility( Resource.Id.taskDone, ViewStates.Visible );
				view.SetTextColor( Resource.Id.taskName, savedContext.Resources.GetColor( Resource.Color.taskDoneText ) );
			}
			else
			{
				bool overdue = false;
				if ( SettingsPersistence.HighlightOverdueTasksProperty == true )
				{
					// Determine whether or not this task is overdue.
					// A task is overdue if it has a due date and the 'days' value of the current date is > than the 'days' value of the
					// due date
					if ( taskToDisplay.DueDate != DateTime.MinValue )
					{
						overdue = ( DateTime.Now - taskToDisplay.DueDate ).Days > 0;
					}
				}

				// Set the background resource according to the list name or the overdue colour
				view.SetInt( Resource.Id.overlay, BackgroundResourceMethodName, 
					overdue ? ListColourHelper.OverdueColourResourceProperty : ListColourHelper.GetColourResource( taskToDisplay.ListName ) );

				view.SetViewVisibility( Resource.Id.taskDone, ViewStates.Invisible );
				view.SetTextColor( Resource.Id.taskName, savedContext.Resources.GetColor( Resource.Color.taskNormalText ) );
			}

			view.SetImageViewResource( Resource.Id.taskPriority, ( taskToDisplay.Priority == 0 ) ? Resource.Drawable.StarOff : Resource.Drawable.StarOn );

			if ( taskToDisplay.DueDate == DateTime.MinValue )
			{
				view.SetViewVisibility( Resource.Id.taskDueLabel, ViewStates.Invisible );
				view.SetViewVisibility( Resource.Id.taskDue, ViewStates.Invisible );
				view.SetViewVisibility( Resource.Id.taskNotification, ViewStates.Gone );
				view.SetViewVisibility( Resource.Id.taskNotificationTime, ViewStates.Gone );
			}
			else
			{
				view.SetViewVisibility( Resource.Id.taskDueLabel, ViewStates.Visible );
				view.SetViewVisibility( Resource.Id.taskDue, ViewStates.Visible );
				view.SetTextViewText( Resource.Id.taskDue, taskToDisplay.DueDate.ToString( DateDisplayFormat ) );

				if ( taskToDisplay.NotificationRequired == false )
				{
					view.SetViewVisibility( Resource.Id.taskNotification, ViewStates.Gone );
					view.SetViewVisibility( Resource.Id.taskNotificationTime, ViewStates.Gone );
				}
				else
				{
					view.SetViewVisibility( Resource.Id.taskNotification, ViewStates.Visible );
					view.SetViewVisibility( Resource.Id.taskNotificationTime, ViewStates.Visible );
					view.SetTextViewText( Resource.Id.taskNotificationTime, taskToDisplay.DueDate.ToString( NotificationTimeDisplayFormat ) );
				}
			}

			// Next, set a fill-intent, which will be used to fill in the pending intent template that is set on the collection view in AutoNagWidget.
			// Add the task id and the task list name
			view.SetOnClickFillInIntent( Resource.Id.overlay, new WidgetIntent().SetTaskIdentity( taskToDisplay.ID ).SetTaskListName( taskToDisplay.ListName ) );

			return view;
		}

		/// <summary>
		/// Gets the item identifier.
		/// </summary>
		/// <returns>The item identifier.</returns>
		/// <param name="position">Position.</param>
		public long GetItemId( int position )
		{
			return position;
		}

		/// <summary>
		/// Raises the data set changed event.
		/// </summary>
		public void OnDataSetChanged()
		{
			// Reload the tasks
			LoadTasks();
		}

		/// <summary>
		/// Gets a value indicating whether this instance has stable identifiers.
		/// </summary>
		/// <value><c>true</c> if this instance has stable identifiers; otherwise, <c>false</c>.</value>
		public bool HasStableIds
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets the loading view.
		/// </summary>
		/// <value>The loading view.</value>
		public RemoteViews LoadingView
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the view type count.
		/// </summary>
		/// <value>The view type count.</value>
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
			// Clear the tasks first
			tasks.Clear();

			// Get the current task list name associated with this widget (may be different from when this factory was created)
			string taskListName = ListNamePersistence.GetListName( savedContext, widgetIdentity );

			if ( taskListName.Length > 0 )
			{
				// Load the tasks with optional sort order. The task list will either contain all task list names or just the single specified name
				tasks = TaskRepository.GetTasks( taskListName == SettingsActivity.CombinedListName ? ( List< string > )TaskRepository.GetTaskTables() :
					new List<string>{ taskListName }, SortOrder.GetTaskSortOrder( savedContext, widgetIdentity ) );

				// Save the count of items to be displayed on the widget
				TaskCountPersistence.SetTaskCount( savedContext, widgetIdentity, tasks.Count );

				savedContext.SendBroadcast( new WidgetIntent( AutoNagWidget.LoadedAction ).SetWidgetId( widgetIdentity ) );
			}
		}

		/// <summary>
		/// The saved context.
		/// </summary>
		private readonly Context savedContext = null;

		/// <summary>
		/// The widget identity.
		/// </summary>
		private readonly int widgetIdentity = 0;

		/// <summary>
		/// The tasks.
		/// </summary>
		private IList< Task > tasks = new List< Task >();

		/// <summary>
		/// The name of the background resource method.
		/// </summary>
		private const string BackgroundResourceMethodName = "setBackgroundResource";

		/// <summary>
		/// The date display format.
		/// </summary>
		private const string DateDisplayFormat = @"dd/MM/yyyy";

		/// <summary>
		/// The notification time display format.
		/// </summary>
		private const string NotificationTimeDisplayFormat = @"HH:mm";
	}
}
