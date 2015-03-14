// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        AlarmManagement
// Filename:    AlarmReceiver.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The AlarmReceiver class displays a notification when a task alarm has occurred.
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
using Android.App;
using Android.Support.V4.App;
using Android.Media;


namespace AutoNag
{
	[BroadcastReceiver]
	/// <summary>
	/// The AlarmReceiver class displays a notification when a task alarm has occurred.
	/// </summary>
	public class AlarmReceiver : BroadcastReceiver
	{
		/// <summary>
		/// This method is called when the BroadcastReceiver is receiving an Intent
		///  broadcast.
		/// </summary>
		/// <param name="alarmContext">Alarm context.</param>
		/// <param name="alarmIntent">Alarm intent.</param>
		public override void OnReceive( Context alarmContext, Intent alarmIntent )
		{
			WidgetIntent wrappedIntent = new WidgetIntent( alarmIntent );

			int taskID = wrappedIntent.TaskIdentityProperty;
			string taskListName = wrappedIntent.TaskListNameProperty;
			int requestCode = WidgetIntent.GetRequestCode( taskID, taskListName );

 			// Create a Notification with a ContentIntent to activate the TaskDetailsScreen. 
			// The request code needs to be based on the task identity and list name.
			// The intent needs to include the task identity, the task list name and a flag indicating that the notification should be turned off.
			PendingIntent viewIntent = PendingIntent.GetActivity( alarmContext, requestCode, 
				new WidgetIntent( alarmContext, typeof( TaskDetailsScreen ) ).SetTaskIdentity( taskID ).SetTaskListName( taskListName ).SetNotification( true ),
				0 ) ;

			// Add an intent for when the notification is cancelled.
			// The request code needs to be based on the task identity and list name.
			// The intent needs to include the task identity and the task list name.
			PendingIntent cancelIntent = PendingIntent.GetBroadcast( alarmContext, requestCode, 
				new WidgetIntent( alarmContext, typeof( NotificationCancelReceiver ) ).SetTaskIdentity( taskID ).SetTaskListName( taskListName ),
				0 ) ;

			NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder( alarmContext )
				.SetContentTitle( wrappedIntent.TaskNameProperty )
				.SetContentText( "Click to view" )
				.SetSmallIcon( Resource.Drawable.icon )
				.SetContentIntent( viewIntent )
				.SetDeleteIntent( cancelIntent )
				.SetSound( RingtoneManager.GetDefaultUri( RingtoneType.Notification ) );

			( ( NotificationManager )alarmContext.GetSystemService( Context.NotificationService ) ).Notify( requestCode, notificationBuilder.Build() );
		}
	}
}

