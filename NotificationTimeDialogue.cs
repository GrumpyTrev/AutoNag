// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        User Interface
// Filename:    NotificationTimeDialogue.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The NotificationTimeDialogue class allows the user to set the time for a notification event.
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
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content;
using System.Collections.Generic;

namespace AutoNag
{
	/// <summary>
	/// The NotificationTimeDialogue class allows the user to set the time for a notification event.
	/// </summary>
	public class NotificationTimeDialogue : DialogFragment
	{
		/// <summary>
		/// Interface to pass back dialogue results
		/// </summary>
		public interface NotificationTimeDialogueListener
		{
			/// <summary>
			/// Raises the notification set event.
			/// </summary>
			/// <param name="hours">Hours.</param>
			/// <param name="minutes">Minutes.</param>
			void OnNotificationSet( int hours, int minutes );

			/// <summary>
			/// Raises the notification cleared event.
			/// </summary>
			void OnNotificationCleared();
		}

		/// <summary>
		/// Creates a NotificationTimeDialogue passing the initial time as Bundle arguments
		/// </summary>
		/// <returns>The instance.</returns>
		/// <param name="initialTime">Initial time.</param>
		public static NotificationTimeDialogue CreateInstance( DateTime initialTime )
		{
			NotificationTimeDialogue timeDialogue = new NotificationTimeDialogue();

			Bundle dialogueArgs = new Bundle();
			dialogueArgs.PutInt( HourLabel, initialTime.Hour );
			dialogueArgs.PutInt( MinuteLabel, initialTime.Minute );
			timeDialogue.Arguments = dialogueArgs;

			return timeDialogue;
		}

		/// <summary>
		/// Called when a fragment is first attached to its activity.
		/// </summary>
		/// <param name="activity">To be added.</param>
		public override void OnAttach(Activity activity)
		{
			base.OnAttach(activity);

			// Save the NotificationTimeDialogueListener interface
			listener = ( NotificationTimeDialogueListener )activity;
		}

		/// <summary>
		/// Called to have the fragment instantiate its user interface view.
		/// </summary>
		/// <param name="inflater">The LayoutInflater object that can be used to inflate
		///  any views in the fragment,</param>
		/// <param name="container">If non-null, this is the parent view that the fragment's
		///  UI should be attached to. The fragment should not add the view itself,
		///  but this can be used to generate the LayoutParams of the view.</param>
		/// <param name="savedInstanceState">If non-null, this fragment is being re-constructed
		///  from a previous saved state as given here.</param>
		/// <returns>The view.</returns>
		public override View OnCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
		{
			View layoutView = inflater.Inflate( Resource.Layout.NotificationSelection, container );

			// Access the TimePicker and set it to 24-hour format
			notificationTimePicker = layoutView.FindViewById< TimePicker >( Resource.Id.notificationTimePicker );
			notificationTimePicker.SetIs24HourView( Java.Lang.Boolean.True );

			// If this is the first time the view has been created then use the time obtained from the arguements
			// The TimePicker automatically saves its sate so there is no need to restore from the Bundle
			if ( savedInstanceState == null )
			{
				int hour = Arguments.GetInt( HourLabel );
				int minute = Arguments.GetInt( MinuteLabel );

				// If the DueDate contains any non-zero hours and minutes fields then initialise the picker to those values, otherwise
				// it will show the current time
				if ( ( hour != 0 ) || ( minute != 0 ) )
				{
					notificationTimePicker.CurrentHour = ( Java.Lang.Integer )hour;
					notificationTimePicker.CurrentMinute = ( Java.Lang.Integer )minute;
				}
				else
				{
					// Make sure that the hour is in the correct format
					notificationTimePicker.CurrentHour = ( Java.Lang.Integer )DateTime.Now.Hour;
				}
			}

			// Set the title
			Dialog.SetTitle( "Notification time" );

			// Setup the click handlers for the 3 buttons
			layoutView.FindViewById< Button >( Resource.Id.notificationCancelButton ).Click += ( buttonSender, buttonEvents ) =>
			{
				Dismiss();
			};

			layoutView.FindViewById< Button >( Resource.Id.notificationSetButton ).Click += ( buttonSender, buttonEvents ) =>
			{
				// Need to clear the focus here to make sure that any changes in progress are saved to the TimePicker
				notificationTimePicker.ClearFocus();

				// Pass back the notification
				if ( listener != null )
				{
					listener.OnNotificationSet( ( int )notificationTimePicker.CurrentHour, ( int )notificationTimePicker.CurrentMinute );
				}

				Dismiss();
			};

			layoutView.FindViewById< Button >( Resource.Id.notificationClearButton ).Click += ( buttonSender, buttonEvents ) =>
			{
				// Pass back the notification
				if ( listener != null )
				{
					listener.OnNotificationCleared();
				}

				Dismiss();
			};

			return layoutView;
		}

		/// <summary>
		/// Called when the fragment is visible to the user and actively running.
		/// There appears to be a problem with the TimePicker such that after a configuration change, i.e. orientation change, the 
		/// hours and minutes are sometimes displayed as blank.
		/// Setting them to a different value and then back to the correct value gets around this
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			Java.Lang.Integer savedHour = notificationTimePicker.CurrentHour;
			notificationTimePicker.CurrentHour = ( Java.Lang.Integer )99;
			notificationTimePicker.CurrentHour = savedHour;

			Java.Lang.Integer savedMinute = notificationTimePicker.CurrentMinute;
			notificationTimePicker.CurrentMinute = ( Java.Lang.Integer )99;
			notificationTimePicker.CurrentMinute = savedMinute;
		}

		/// <summary>
		/// Called when the Fragment is no longer resumed.
		/// Need to clear the focus here to make sure that any changes in progress are saved to the TimePicker
		/// </summary>
		public override void OnPause()
		{
			base.OnPause();
			notificationTimePicker.ClearFocus();
		}

		/// <summary>
		/// The NotificationTimeDialogueListener instance
		/// </summary>
		private NotificationTimeDialogueListener listener = null;

		/// <summary>
		/// The notification time picker.
		/// </summary>
		private TimePicker notificationTimePicker = null;

		/// <summary>
		/// Keys for storing arguments
		/// </summary>
		private const string HourLabel = "Hour";
		private const string MinuteLabel = "Minute";
	}
}

