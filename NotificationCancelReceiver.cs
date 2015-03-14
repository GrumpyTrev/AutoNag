// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        AlarmManagement
// Filename:    NotificationCancelReceiver.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The NotificationCancelReceiver is called when a notification is cancelled fromthe notification screen.
//				 Turn off the notification in the associated task
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
using System;


namespace AutoNag
{
	[BroadcastReceiver]
	/// <summary>
	/// The NotificationCancelReceiver is called when a notification is cancelled fromthe notification screen.
	/// Turn off the notification in the associated task
	/// </summary>
	public class NotificationCancelReceiver : BroadcastReceiver
	{
		/// <summary>
		/// This method is called when the BroadcastReceiver is receiving an Intent
		///  broadcast.
		/// </summary>
		/// <param name="context">The Context in which the receiver is running.</param>
		/// <param name="intent">The Intent being received.</param>
		public override void OnReceive( Context cancelContext, Intent cancelIntent )
		{
			// Access the task and task list name associated with the intent and turn off the notification
			WidgetIntent intentWrapper = new WidgetIntent( cancelIntent );

			int taskID = intentWrapper.TaskIdentityProperty;
			if ( taskID != 0 )
			{
				string taskListName = intentWrapper.TaskListNameProperty;

				Task cancelTask = TaskRepository.GetTask( taskListName, taskID );

				if ( cancelTask != null )
				{
					if ( cancelTask.NotificationRequired == true )
					{
						cancelTask.NotificationRequired = false;
						cancelTask.DueDate = new DateTime( cancelTask.DueDate.Year, cancelTask.DueDate.Month, cancelTask.DueDate.Day, 0, 0, 0 );

						TaskRepository.SaveTask( taskListName, cancelTask );

						// Refresh the widgets showing this task list
						cancelContext.SendBroadcast( new WidgetIntent( AutoNagWidget.UpdatedAction ).SetTaskListName( taskListName ) );
					}
				}
			}
		}
	}
}

