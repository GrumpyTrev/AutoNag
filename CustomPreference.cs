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
	public class CustomPreference : Preference, View.IOnCreateContextMenuListener
	{
		//
		// Public methods
		//

		/// <summary>
		/// Delegate to be called when the preference has been selected with a single click
		/// </summary>
		public delegate void PreferenceSelectedDelegate( string itemName );

		/// <summary>
		/// Delegate to be called to customise the context menu when a long click has been detected
		/// </summary>
		public delegate void ContextMenuDelegate( string listName, IContextMenu menuInterface );

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoNag.CustomPreference"/> class.
		/// </summary>
		/// <param name="viewContext">View context.</param>
		/// <param name="preferenceSelected">Selection delegate.</param>
		/// <param name="contextMenuRequired">Menu delegate.</param>
		/// <param name="backGround">Back ground.</param>
		/// <param name="title">Title.</param>
		public CustomPreference( Context viewContext, PreferenceSelectedDelegate preferenceSelected, ContextMenuDelegate contextMenuRequired, int backGround, 
			string title ) : base( viewContext )
		{
			selectionDelegate = preferenceSelected;
			menuDelegate = contextMenuRequired;
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
		/// Raises the create context menu event.
		/// Pass this back to the ContextMenuDelegate so it can create the menu for the selected item
		/// </summary>
		/// <param name="menuInterface">Menu interface.</param>
		/// <param name="theView">The view.</param>
		/// <param name="menuInfo">Menu info.</param>
		public void OnCreateContextMenu( IContextMenu menuInterface, View theView, IContextMenuContextMenuInfo menuInfo )
		{
			if ( menuDelegate != null )
			{
				menuDelegate( Title, menuInterface );
			}
		}

		/// <summary>
		/// Sets the selection delegate interface.
		/// </summary>
		/// <value>The selected property.</value>
		public PreferenceSelectedDelegate SelectionProperty
		{
			set
			{
				selectionDelegate = value;
			}
		}

		/// <summary>
		/// Sets the context menu delegate interface.
		/// </summary>
		/// <value>The selected property.</value>
		public ContextMenuDelegate ContextMenuProperty
		{
			set
			{
				menuDelegate = value;
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

			if ( menuDelegate != null )
			{
				// Set up a listener to catch when a context menu is required
				view.SetOnCreateContextMenuListener( this );

				// For some reason setting up a long click listener prevents the OnClick override from being called.
				// So we need to catch the event itself and process
				view.Click += (object sender, EventArgs e) => OnClick();
			}

			SetBackground();
		}

    	/// <summary>
		/// Processes a click on the preference.
		/// </summary>
		protected override void OnClick()
		{
			base.OnClick();
			if ( selectionDelegate != null )
			{
				selectionDelegate( Title );
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
		/// The selection delegate.
		/// </summary>
		private PreferenceSelectedDelegate selectionDelegate =  null;

		/// <summary>
		/// The menu delegate.
		/// </summary>
		private ContextMenuDelegate menuDelegate = null;

		/// <summary>
		/// The display view.
		/// </summary>
		private View displayView = null;
	}
}

