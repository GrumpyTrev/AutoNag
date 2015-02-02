// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        Helper classes
// Filename:    IntentWrappers.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      A set of classes to create Intent objects with specific 'extra' content.
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
using Android.Appwidget;

namespace AutoNag
{
	/// <summary>
	/// The WidgetIntentCreator is the base class for Intent creators that contain at least a widget identity with optional task list name and task identity
	/// </summary>
	public class WidgetIntent : Intent
	{
		/// <summary>
		/// WidgetIntent constructor for an intent targeted at a class"/> class.
		/// </summary>
		/// <param name="intentContext">Intent context.</param>
		/// <param name="target">Target.</param>
		public WidgetIntent( Context intentContext, System.Type target ) : base( intentContext, target )
		{ 
		}

		/// <summary>
		/// WidgetIntent constructor for an intent with a broadcast action"/> class.
		/// </summary>
		/// <param name="action">Action.</param>
		public WidgetIntent( string action ) : base( action )
		{
		}

		/// <summary>
		/// WidgetIntent constructor for an intent wrapped around an existing intent
		/// </summary>
		/// <param name="baseIntent">Base intent.</param>
		public WidgetIntent( Intent baseIntent )
		{
			wrappedIntent = baseIntent;
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public WidgetIntent() : base()
		{
		}

		/// <summary>
		/// Sets the widget identifier.
		/// </summary>
		/// <returns>The widget identifier.</returns>
		/// <param name="widgetId">Identity.</param>
		public virtual WidgetIntent SetWidgetId( int widgetId )
		{
			PutExtra( AppWidgetManager.ExtraAppwidgetId, widgetId );

			return this;
		}

		/// <summary>
		/// Gets the widget identifier property.
		/// </summary>
		/// <value>The widget identifier property.</value>
		public int WidgetIdProperty
		{
			get
			{
				return wrappedIntent.GetIntExtra( AppWidgetManager.ExtraAppwidgetId, AppWidgetManager.InvalidAppwidgetId );
			}
		}

		/// <summary>
		/// Sets the name of the task list.
		/// </summary>
		/// <returns>The task list name.</returns>
		/// <param name="taskListName">Task list name.</param>
		public WidgetIntent SetTaskListName( string taskListName )
		{
			PutExtra( TaskListNameItem, taskListName );
			return this;
		}

		/// <summary>
		/// Gets the task list name property.
		/// </summary>
		/// <value>The task list name property.</value>
		public string TaskListNameProperty
		{
			get
			{
				return wrappedIntent.GetStringExtra( TaskListNameItem );
			}
		}

		/// <summary>
		/// Sets the task identity.
		/// </summary>
		/// <returns>The task identity.</returns>
		/// <param name="taskIdentity">Task identity.</param>
		public WidgetIntent SetTaskIdentity( int taskIdentity )
		{
			PutExtra( TaskIdentityItem, taskIdentity );
			return this;
		}

		/// <summary>
		/// Gets the task identity property.
		/// </summary>
		/// <value>The task list name property.</value>
		public int TaskIdentityProperty
		{
			get
			{
				return wrappedIntent.GetIntExtra( TaskIdentityItem, -1 );
			}
		}

		/// <summary>
		/// Sets the notification.
		/// </summary>
		/// <returns>The notification.</returns>
		/// <param name="notification">Notification.</param>
		public WidgetIntent SetNotification( bool notification )
		{
			PutExtra( NotificationItem, notification );
			return this;
		}

		/// <summary>
		/// Gets the notification property.
		/// </summary>
		/// <value>The notification property.</value>
		public bool NotificationProperty
		{
			get
			{
				return wrappedIntent.GetBooleanExtra( NotificationItem, false );
			}
		}

		/// <summary>
		/// Sets the index of the icon.
		/// </summary>
		/// <returns>The icon index.</returns>
		/// <param name="iconIndex">Icon index.</param>
		public WidgetIntent SetIconIndex( int iconIndex )
		{
			PutExtra( IconIndexItem, iconIndex );
			return this;
		}

		/// <summary>
		/// Gets the icon index.
		/// </summary>
		/// <value>The icon index.</value>
		public int IconIndexProperty
		{
			get
			{
				return wrappedIntent.GetIntExtra( IconIndexItem, -1 );
			}
		}

		/// <summary>
		/// Sets the name of the task.
		/// </summary>
		/// <returns>The task name.</returns>
		/// <param name="taskName">Task name.</param>
		public WidgetIntent SetTaskName( string taskName )
		{
			PutExtra( TaskNameItem, taskName );
			return this;
		}

		/// <summary>
		/// Gets the task name property.
		/// </summary>
		/// <value>The task name property.</value>
		public string TaskNameProperty
		{
			get
			{
				return wrappedIntent.GetStringExtra( TaskNameItem );
			}
		}

		/// <summary>
		/// Get a unique request code from a combination of the task identity and task list name.
		/// </summary>
		/// <returns>The request code.</returns>
		/// <param name="taskIdentity">Task identity.</param>
		/// <param name="taskListName">Task list name.</param>
		public static int GetRequestCode( int taskIdentity, string taskListName )
		{
			return string.Format( "{0} {1}", taskIdentity, taskListName ).GetHashCode();
		}

		/// <summary>
		/// Get a unique request code from a combination of the widget identity and request type.
		/// </summary>
		/// <returns>The request code.</returns>
		/// <param name="taskIdentity">Task identity.</param>
		/// <param name="taskListName">Task list name.</param>
		public static int GetRequestCode( int widgetIdentity, int requestType )
		{
			return string.Format( "{0} {1}", widgetIdentity, requestType ).GetHashCode();
		}

		//
		// Protected data
		// 

		/// <summary>
		/// The wrapped intent.
		/// </summary>
		protected Intent wrappedIntent = null;

		//
		// Private data
		//

		/// <summary>
		/// The task list name item.
		/// </summary>
		private const string TaskListNameItem = "taskListNameItem";

		/// <summary>
		/// The task identity item.
		/// </summary>
		private const string TaskIdentityItem = "TaskID";

		/// <summary>
		/// The notification item.
		/// </summary>
		private const string NotificationItem = "Notification";

		/// <summary>
		/// The icon index item.
		/// </summary>
		private const string IconIndexItem = "IconIndex";

		/// <summary>
		/// The task name item.
		/// </summary>
		private const string TaskNameItem = "taskName";
	}

	/// <summary>
	/// Pending widget intent.
	/// </summary>
	public class PendingWidgetIntent : WidgetIntent
	{
		/// <summary>
		/// PendingWidgetIntent constructor for an intent targeted at a class"/> class.
		/// </summary>
		/// <param name="intentContext">Intent context.</param>
		/// <param name="target">Target.</param>
		public PendingWidgetIntent( Context intentContext, System.Type target ) : base( intentContext, target )
		{ 
		}

		/// <summary>
		/// PendingWidgetIntent constructor for an intent with a broadcast action"/> class.
		/// </summary>
		/// <param name="action">Action.</param>
		public PendingWidgetIntent( string action ) : base( action )
		{
		}

		/// <summary>
		/// Sets the widget identifier( via the base class) and a unique data code
		/// </summary>
		/// <returns>The widget identifier.</returns>
		/// <param name="widgetId">Identity.</param>
		public override WidgetIntent SetWidgetId( int widgetId )
		{
			// Make this intent unique by encoding the widget id in the URI data
			SetData( Android.Net.Uri.FromParts( "content", widgetId.ToString(), null ) );

			return base.SetWidgetId( widgetId );
		}
	}
}

