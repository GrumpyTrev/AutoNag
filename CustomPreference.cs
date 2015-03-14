// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        Settings
// Filename:    CustomPreference.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The CustomPreference class allows the background and text colour of a preference to be changed.
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

namespace AutoNag
{
	/// <summary>
	/// The CustomPreference class allows the background and text colour of a preference to be changed.
	/// </summary>
	public class CustomPreference : Preference
	{
		/// <summary>
		/// List selected delegate.
		/// </summary>
		public delegate void ListSelectedDelegate( string listName );

		//
		// Public methods
		//

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoNag.CustomPreference"/> class.
		/// </summary>
		/// <param name="viewContext">View context.</param>
		/// <param name="selectionDelegate">Selection delegate.</param>
		/// <param name="backGround">Back ground.</param>
		/// <param name="title">Title.</param>
		public CustomPreference( Context viewContext, ListSelectedDelegate selectionDelegate, int backGround, string title ) : base( viewContext )
		{
			listener = selectionDelegate;
			ColourResourceProperty = backGround;
			Title = title;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoNag.CustomPreference"/> class.
		/// </summary>
		/// <param name="viewContext">View context.</param>
		/// <param name="attr">Attr.</param>
		public CustomPreference( Context viewContext, IAttributeSet attr ) : base( viewContext, attr )
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoNag.CustomPreference"/> class.
		/// </summary>
		/// <param name="viewContext">View context.</param>
		public CustomPreference( Context viewContext ) : base( viewContext )
		{
		}

		/// <summary>
		/// Sets the colour resource property.
		/// If a view has already been bound then set the background
		/// </summary>
		/// <value>The colour resource property.</value>
		public int ColourResourceProperty
		{
			set
			{
				backgroundResource = value;
				if ( displayView != null )
				{
					SetBackground();
				}
			}
		}

		/// <summary>
		/// Sets the selected delegate.
		/// </summary>
		/// <value>The selected property.</value>
		public ListSelectedDelegate SelectedProperty
		{
			set
			{
				listener = value;
			}
		}

		//
		// Protected methods
		//

		/// <summary>
		/// Binds the created View to the data for this Preference.
		/// </summary>
		/// <param name="view">The View that shows this Preference.</param>
		protected override void OnBindView(Android.Views.View view)
		{
			base.OnBindView(view);
			displayView = view;

			SetBackground();
		}

		/// <summary>
		/// Processes a click on the preference.
		/// </summary>
		protected override void OnClick()
		{
			base.OnClick();
			if ( listener != null )
			{
				listener( Title );
			}
		}

		//
		// Private methods
		//

		/// <summary>
		/// Sets the background.
		/// </summary>
		private void SetBackground()
		{
			if ( backgroundResource != 0 )
			{
				displayView.SetBackgroundResource( backgroundResource );
				displayView.FindViewById< TextView >( Android.Resource.Id.Title ).SetTextColor( Color.Black );
			}
		}

		//
		// Private data
		//

		/// <summary>
		/// The background resource.
		/// </summary>
		private int backgroundResource = 0;

		/// <summary>
		/// The listener.
		/// </summary>
		private ListSelectedDelegate listener =  null;

		/// <summary>
		/// The display view.
		/// </summary>
		private View displayView = null;
	}
}

