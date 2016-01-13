// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        AlarmManagement
// Filename:    OverdueTasksRefresher.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The OverdueTasksRefresher class refreshes the displayed state of tasks to allow any overdue tasks to be highlighted.
//				 
// Description:  This gets over the problem of tasks going overdue without the list being refeshed to display the visual indication
//				 A periodic Alarm is set to expire at midnight. On expiry a HighlightOverdueAction intent is broadcast.
//				 An Alarm has to be used as it persists at the system level.
//
//
//
// File History
// ------------
//
// %version:  1 %
//
// (c) Copyright 2016 Trevor Simmonds.
// This software is protected by copyright, the design of any 
// article recorded in the software is protected by design 
// right and the information contained in the software is 
// confidential. This software may not be copied, any design 
// may not be reproduced and the information contained in the 
// software may not be used or disclosed except with the
// prior written permission of and in a manner permitted by
// the proprietors Trevor Simmonds (c) 2016
//
//    Copyright Holders:
//       Trevor Simmonds,
//       t.simmonds@virgin.net
//
using System;


using System.Threading;
using Android.Content;
using Android.App;
using Android.Support.V4.App;

namespace AutoNag
{
	[BroadcastReceiver]
	/// <summary>
	/// The OverdueTasksRefresher class refreshes the displayed state of tasks to allow any overdue tasks to be highlighted.
	/// </summary>
	public class OverdueTasksRefresher : BroadcastReceiver
	{
		//
		// Public methods
		//

		/// <summary>
		/// Default constructor
		/// </summary>
		public OverdueTasksRefresher()
		{
		}

		/// <summary>
		/// This method is called when the BroadcastReceiver is receiving an Intent
		///  broadcast.
		/// </summary>
		/// <param name="alarmContext">Alarm context.</param>
		/// <param name="alarmIntent">Alarm intent.</param>
		public override void OnReceive( Context alarmContext, Intent alarmIntent )
		{
			// Only proceed if overdue highlighting is required
			if ( SettingsPersistence.HighlightOverdueTasksProperty == true )
			{
				Application.Context.SendBroadcast( new WidgetIntent( AutoNagWidget.HighlightOverdueAction ) );
			}
		}

		/// <summary>
		/// Create and start a regular Alarm to expire at midnight
		/// </summary>
		public static void Start()
		{
			// Convert to UTC time and adjust for difference between Java and .net time 
			( ( AlarmManager )Application.Context.GetSystemService( Context.AlarmService ) )
				.SetRepeating( AlarmType.RtcWakeup, 
					TimeZoneInfo.ConvertTimeToUtc( DateTime.Today.AddDays( 1 ) ).AddSeconds( -epochDifferenceInSeconds ).Ticks / 10000, 
					AlarmManager.IntervalDay, 
					PendingIntent.GetBroadcast( Application.Context, requestCode, new Intent( Application.Context, typeof( OverdueTasksRefresher ) ), 0) );
		}

		/// <summary>
		/// Stop the Alarm.
		/// </summary>
		public static void Stop()
		{
			( ( AlarmManager )Application.Context.GetSystemService( Context.AlarmService ) ) 
				.Cancel( PendingIntent.GetBroadcast( Application.Context, requestCode, new Intent( Application.Context, typeof( OverdueTasksRefresher ) ), 0) );
		}

		//
		// Private methods
		//

		//
		// Private data
		//

		/// <summary>
		/// The epoch difference in seconds between epoch (Java) and ticks (.NET)
		/// </summary>
		static double epochDifferenceInSeconds = ( new DateTime( 1970, 1, 1 ) - DateTime.MinValue ).TotalSeconds;

		/// <summary>
		/// The request code to match cancellations with sets
		/// </summary>
		private const int requestCode = 57;
	}
}

