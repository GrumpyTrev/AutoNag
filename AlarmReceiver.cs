
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Media;

namespace AutoNag
{
	[BroadcastReceiver]
	public class AlarmReceiver : BroadcastReceiver
	{
		public override void OnReceive( Context alarmContext, Intent alarmIntent )
		{
			int taskID = alarmIntent.GetIntExtra( "TaskID", 0 );
			string taskName = alarmIntent.GetStringExtra( "TaskName" );

 			// Create a Notification with a ContentIntent to activate the TaskDetailsScreen. Need to include the taskID in the intent together
			// with a flag indicating that the notificatipon should be turned off.
			PendingIntent viewIntent = PendingIntent.GetActivity( alarmContext, taskID, new Intent( alarmContext, typeof( TaskDetailsScreen ) )
				.PutExtra( "TaskID", taskID )
				.PutExtra( "Notification", 1 ), 0 ) ;

			// Add an intent for when the notification is cancelled
			PendingIntent cancelIntent = PendingIntent.GetBroadcast( alarmContext, taskID, new Intent( alarmContext, typeof( NotificationCancelReceiver ) )
				.PutExtra( "TaskID", taskID ), 0 ) ;

			Notification.Builder notificationBuilder = new Notification.Builder( alarmContext )
				.SetContentTitle( taskName )
				.SetContentText( "Click to view" )
				.SetSmallIcon( Resource.Drawable.icon )
				.SetContentIntent( viewIntent )
				.SetDeleteIntent( cancelIntent )
				.SetSound( RingtoneManager.GetDefaultUri( RingtoneType.Notification ) );

			( ( NotificationManager )alarmContext.GetSystemService( Context.NotificationService ) ).Notify( taskID, notificationBuilder.Build() );
		}
	}
}

