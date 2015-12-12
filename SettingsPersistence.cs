// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        Persistence
// Filename:    SettingsPersistence.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The SettingsPersistence class controls the persistence of some miscellaneous user settings.
//
// Description:  Currently used for the notification tone
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
using Android.Media;
using Android.Content;

namespace AutoNag
{
	/// <summary>
	/// The SettingsPersistence class controls the persistence of some miscellaneous user settings.
	/// </summary>
	public class SettingsPersistence
	{
		//
		// Public methods
		//

		/// <summary>
		/// Gets or sets the notification tone property.
		/// </summary>
		/// <value>The notification tone property.</value>
		public static string NotificationToneProperty
		{
			get
			{
				return notificationTone;
			}

			set
			{
				notificationTone = value;
				UpdateOptions();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="AutoNag.SettingsPersistence"/> highlight overdue tasks property.
		/// </summary>
		/// <value><c>true</c> if highlight overdue tasks property; otherwise, <c>false</c>.</value>
		public static bool HighlightOverdueTasksProperty
		{
			get
			{
				return highlightOverdueTasks;
			}

			set
			{
				highlightOverdueTasks = value;
				UpdateOptions();
			}
		}

		//
		// Private methods
		//

		/// <summary>
		/// Private constructor
		/// </summary>
		private SettingsPersistence()
		{
		}

		/// <summary>
		/// Initializes the <see cref="AutoNag.SettingsPersistence"/> class.
		/// </summary>
		static SettingsPersistence()
		{
			notificationTone = TaskRepository.GetOptions( ref highlightOverdueTasks );
			if ( notificationTone.Length == 0 )
			{
				notificationTone = RingtoneManager.GetDefaultUri( RingtoneType.Notification ).ToString();
				UpdateOptions();
			}
		}

		private static void UpdateOptions()
		{
			TaskRepository.SetOptions( notificationTone, highlightOverdueTasks );
		}

		//
		// Private data
		//

		/// <summary>
		/// The name of the selected notification tone (Uri cast to a string)
		/// </summary>
		private static string notificationTone = null;
		 
		/// <summary>
		/// Should overdue tasks be highlighted
		/// </summary>
		private static bool highlightOverdueTasks = false;
	}
}

