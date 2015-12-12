// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        Settings
// Filename:    OverduePreference.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The OverduePreference class allows whether overdue items are highlighted to be specified.
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
	/// The OverduePreference class allows whether overdue items are highlighted to be specified.
	/// </summary>
	public class OverduePreference : CheckBoxPreference
	{
		//
		// Public methods
		//

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoNag.OverduePreference"/> class.
		/// </summary>
		/// <param name="viewContext">View context.</param>
		/// <param name="attr">Attr.</param>
		public OverduePreference( Context viewContext, IAttributeSet attr ) : base( viewContext, attr )
		{
			Checked = SettingsPersistence.HighlightOverdueTasksProperty;
		}

		//
		// Protected methods
		//

		/// <summary>
		/// Processes a click on the preference.
		/// </summary>
		protected override void OnClick()
		{
			base.OnClick();
			SettingsPersistence.HighlightOverdueTasksProperty = Checked;

			// Get the widgets to refresh themselves
			Context.SendBroadcast( new WidgetIntent( AutoNagWidget.HighlightOverdueAction ) );
		}

		//
		// Private methods
		//

		//
		// Private data
		//
	}
}

