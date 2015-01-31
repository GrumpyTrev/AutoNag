// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        AlarmManagement
// Filename:    AlarmInterface.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The AlarmInterface class provides an interface to the AlarmManagement system in order to set and cancel alarms.
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

using Android.App;
using Android.Content;

namespace AutoNag
{
	/// <summary>
	/// The AlarmInterface class provides an interface to the AlarmManagement system in order to set and cancel alarms.
	/// </summary>
	public class AlarmInterface
	{
		//
		// Public methods
		//

		/// <summary>
		/// Sets the alarm.
		/// </summary>
		/// <param name="taskListName">Task list name.</param>
		/// <param name="taskIdentity">Task identity.</param>
		/// <param name="taskName">Task name</param>
		/// <param name="alarmDate">Alarm date.</param>
		/// <param name="intentContext">Intent context.</param>
		public static void SetAlarm( string taskListName, int taskIdentity, string taskName, DateTime alarmDate, Context intentContext )
		{
			long timeToAlarm = ( long )( alarmDate - new DateTime( 1970, 1, 1 ) ).TotalMilliseconds;

			( ( AlarmManager )intentContext.GetSystemService( Context.AlarmService ) )
				.Set( AlarmType.RtcWakeup, timeToAlarm, PendingIntent.GetBroadcast( intentContext, GetRequestCode( taskIdentity, taskListName ), 
					new WidgetIntent( intentContext, typeof( AlarmReceiver ) ).SetTaskIdentity( taskIdentity ).SetTaskName( taskName )
						.SetTaskListName( taskListName ), 0) );
		}

		/// <summary>
		/// Cancels the alarm.
		/// </summary>
		/// <returns><c>true</c> if cancel alarm the specified taskIdentity intentContext; otherwise, <c>false</c>.</returns>
		/// <param name="taskListName">Task list name.</param>
		/// <param name="taskIdentity">Task identity.</param>
		/// <param name="intentContext">Intent context.</param>
		public static void CancelAlarm( string taskListName, int taskIdentity, Context intentContext )
		{
			( ( AlarmManager )intentContext.GetSystemService( Context.AlarmService ) )
				.Cancel( PendingIntent.GetBroadcast( intentContext, GetRequestCode( taskIdentity, taskListName ), 
					new Intent( intentContext, typeof( AlarmReceiver ) ), 0) );
		}

		/// <summary>
		/// Get a unique request code from a combination of the task identity and task list name.
		/// </summary>
		/// <returns>The request code.</returns>
		/// <param name="taskIdentity">Task identity.</param>
		/// <param name="taskListName">Task list name.</param>
		public static int GetRequestCode( int taskIdentity, string taskListName )
		{
			return string.Format( "{0}{1}", taskIdentity, taskListName ).GetHashCode();
		}

		//
		// Private methods
		//

		/// <summary>
		/// Private constructor
		/// </summary>
		private AlarmInterface()
		{
		}
	}
}

