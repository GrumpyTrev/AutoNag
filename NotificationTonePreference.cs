// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        Settings
// Filename:    NotificationTonePreference.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The NotificationTonePreference class allows the notification tone to be displayed and updated.
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
using Android.Preferences;
using Android.Graphics;
using Android.Content;
using Android.Views;
using Android.Util;
using Android.Widget;
using Android.Media;

namespace AutoNag
{
	/// <summary>
	/// The CustomPreference class allows the background and text colour of a preference to be changed.
	/// </summary>
	public class NotificationTonePreference : RingtonePreference
	{
		//
		// Public methods
		//

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoNag.NotificationTonePreference"/> class.
		/// </summary>
		/// <param name="viewContext">View context.</param>
		/// <param name="attr">Attr.</param>
		public NotificationTonePreference( Context viewContext, IAttributeSet attr ) : base( viewContext, attr )
		{
			RingtoneType = RingtoneType.Notification;
			ShowDefault = false;
			ShowSilent = false;
			SetSummary();
		}

		//
		// Protected methods
		//

		/// <summary>
		/// Called when the chooser is about to be shown and the current ringtone
		/// should be marked.
		/// </summary>
		/// <returns>To be added.</returns>
		protected override Android.Net.Uri OnRestoreRingtone()
		{
			return Android.Net.Uri.Parse( SettingsPersistence.NotificationToneProperty );
		}

		/// <summary>
		/// Raises the save ringtone event.
		/// </summary>
		/// <param name="ringtoneUri">Ringtone URI.</param>
		protected override void OnSaveRingtone( Android.Net.Uri ringtoneUri )
		{
			SettingsPersistence.NotificationToneProperty = ringtoneUri.ToString();
			SetSummary();
		}

		//
		// Private methods
		//

		/// <summary>
		/// Sets the summary to a human readable version of the ringtone uri
		/// </summary>
		private void SetSummary()
		{
			Summary = RingtoneManager.GetRingtone( Context, Android.Net.Uri.Parse( SettingsPersistence.NotificationToneProperty ) ).GetTitle( Context );
		}

		//
		// Private data
		//
	}
}

