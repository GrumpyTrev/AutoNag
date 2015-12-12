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
			// Convert to UTC time and adust for difference between Java and .net time 
			( ( AlarmManager )intentContext.GetSystemService( Context.AlarmService ) )
				.Set( AlarmType.RtcWakeup, TimeZoneInfo.ConvertTimeToUtc( alarmDate ).AddSeconds( -epochDifferenceInSeconds ).Ticks / 10000, 
					PendingIntent.GetBroadcast( intentContext, WidgetIntent.GetRequestCode( taskIdentity, taskListName ), 
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
				.Cancel( PendingIntent.GetBroadcast( intentContext, WidgetIntent.GetRequestCode( taskIdentity, taskListName ), 
					new Intent( intentContext, typeof( AlarmReceiver ) ), 0) );
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

		//
		// Private data
		//

		/// <summary>
		/// The epoch difference in seconds between epoch (Java) and ticks (.NET)
		/// </summary>
		static double epochDifferenceInSeconds = ( new DateTime( 1970, 1, 1 ) - DateTime.MinValue ).TotalSeconds;
	}
}

