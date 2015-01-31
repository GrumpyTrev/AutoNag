// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        AlarmManagement
// Filename:    AlarmRefresh.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The AlarmRefresh activity runs at device bootup and generates alarms for any notifyable tasks.
//				 
// Description:  The entire set of tasks is read in and an aalrm is generated for all taks that have a notificaction time
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

using Android.App;
using Android.Content;
using System.Collections.Generic;

namespace AutoNag
{
	/// <summary>
	/// The AlarmRefresh activity runs at device bootup and generates alarms for any notifyable tasks.
	/// </summary>
	[BroadcastReceiver]
	[IntentFilter( new [] { Intent.ActionBootCompleted }, Categories = new [] { Intent.CategoryDefault } )]
	public class AlarmRefresh : BroadcastReceiver
	{
		/// <summary>
		/// This method is called when the BroadcastReceiver is receiving an Intent
		/// broadcast.
		/// If the intent is the ActionBootCompleted then load the tasks and generate
		/// an alarm for each task that has a notification time
		/// </summary>
		/// <param name="context">The Context in which the receiver is running.</param>
		/// <param name="intent">The Intent being received.</param>
		public override void OnReceive( Context context, Intent intent )
		{
			if ( intent.Action == Intent.ActionBootCompleted )
			{
				IList< string > taskListNames = TaskRepository.GetTaskTables();
				foreach ( string taskListName in taskListNames )
				{
					foreach ( Task currentTask in TaskRepository.GetTasks( taskListName, null ) )
					{
						if ( currentTask.NotificationRequired == true )
						{
							AlarmInterface.SetAlarm( taskListName, currentTask.ID, currentTask.Name, currentTask.DueDate, context );
						}
					}
				}
			}
		}
	}
}

