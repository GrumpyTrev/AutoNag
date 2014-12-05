
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

namespace AutoNag
{
	[BroadcastReceiver]
	public class NotificationCancelReceiver : BroadcastReceiver
	{
		public override void OnReceive( Context cancelContext, Intent cancelIntent )
		{
			// Access the task associated with the intent and turn off the notification
			int taskID = cancelIntent.GetIntExtra( "TaskID", 0 );

			if ( taskID != 0 )
			{
				Task cancelTask = TaskManager.GetTask( taskID );

				if ( cancelTask != null )
				{
					if ( cancelTask.NotificationRequired == true )
					{
						cancelTask.NotificationRequired = false;
						cancelTask.DueDate = new DateTime( cancelTask.DueDate.Year, cancelTask.DueDate.Month, cancelTask.DueDate.Day, 0, 0, 0 );

						TaskManager.SaveTask( cancelTask );
						cancelContext.SendBroadcast( new Intent( AutoNagWidget.UpdatedAction ) );
					}
				}
			}

		}
	}
}

