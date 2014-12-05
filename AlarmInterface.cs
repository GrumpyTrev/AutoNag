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
		/// <param name="taskIdentity">Task identity.</param>
		/// <param name="taskName">Task name</param>
		/// <param name="alarmDate">Alarm date.</param>
		/// <param name="intentContext">Intent context.</param>
		public static void SetAlarm( int taskIdentity, string taskName, DateTime alarmDate, Context intentContext )
		{
			long timeToAlarm = ( long )( alarmDate - new DateTime( 1970, 1, 1 ) ).TotalMilliseconds;

			PendingIntent pendingIntent = PendingIntent.GetBroadcast( intentContext, taskIdentity, new Intent( intentContext, typeof( AlarmReceiver ) )
				.PutExtra( "TaskID", taskIdentity )
				.PutExtra( "TaskName", taskName ), 0);

			( ( AlarmManager )intentContext.GetSystemService( Context.AlarmService ) ).Set( AlarmType.RtcWakeup, timeToAlarm, pendingIntent );
		}

		/// <summary>
		/// Cancels the alarm.
		/// </summary>
		/// <returns><c>true</c> if cancel alarm the specified taskIdentity intentContext; otherwise, <c>false</c>.</returns>
		/// <param name="taskIdentity">Task identity.</param>
		/// <param name="intentContext">Intent context.</param>
		public static void CancelAlarm( int taskIdentity, Context intentContext )
		{
			PendingIntent pendingIntent = PendingIntent.GetBroadcast( intentContext, taskIdentity, new Intent( intentContext, typeof( AlarmReceiver ) ), 0);

			( ( AlarmManager )intentContext.GetSystemService( Context.AlarmService ) ).Cancel( pendingIntent );
		}

		//
		//
		//

		/// <summary>
		/// Private constructor
		/// </summary>
		private AlarmInterface()
		{
		}

	}
}

