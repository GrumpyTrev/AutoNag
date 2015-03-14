// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        Settings
// Filename:    ListPreference.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The ListPreference class is a custom Preference that displays the list names in a ListView dialogue.
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

using Android.OS;
using Android.Widget;
using Android.App;
using Android.Preferences;
using Android.Content;
using Android.Views;
using Android.Util;
using Android.Content.Res;

namespace AutoNag
{
	/// <summary>
	/// The ListPreference class is a custom Preference that displays the list names in a ListView dialogue.
	/// </summary>
	public class ListPreference : Preference
	{
		/// <summary>
		/// List selected delegate.
		/// </summary>
		public delegate void ListSelectedDelegate( string listName );

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoNag.ListPreference"/> class.
		/// </summary>
		/// <param name="viewContext">View context.</param>
		/// <param name="attr">Attr.</param>
		public ListPreference( Context viewContext, IAttributeSet attr ) : base( viewContext, attr )
		{
			adapter = new ListNameAdapter( Context );
		}

		/// <summary>
		/// Sets the selected property.
		/// </summary>
		/// <value>The selected property.</value>
		public ListSelectedDelegate SelectedProperty
		{
			set
			{
				listener = value;
			}
		}

		/// <summary>
		/// Notifies the data set changed.
		/// </summary>
		public void NotifyDataSetChanged()
		{
			adapter.NotifyDataSetChanged();
		}

		/// <summary>
		/// Dismiss the dialogue if it is being shown. 
		/// This must be called when the settings activity pauses otherwise there's a memory leak.
		/// This is called before OnSaveInstanceState, so record here whether or not the dialogue needs to be restoredf
		/// </summary>
		public void OnDismiss() 
		{
			// Record if the dialogue was being shown, assume not
			dialogueWasBeingShown = false;

			// If there is a dialogue dismiss it
			if ( theDialog != null )
			{
				dialogueWasBeingShown = theDialog.IsShowing;
				theDialog.Dismiss();
				theDialog = null;
			}
		}

		//
		// Protected methods
		//

		/// <summary>
		/// Processes a click on the preference.
		/// </summary>
		protected override void OnClick()
		{
			ShowDialogue();
		}

		/// <summary>
		/// Hook allowing a Preference to generate a representation of its internal
		/// state that can later be used to create a new instance with that same
		/// state.
		/// </summary>
		/// <returns>Bundle containing saved state</returns>
		protected override IParcelable OnSaveInstanceState()
		{
			// Have to call the base class otherwise an exception is raised, but don't actually need the result
			base.OnSaveInstanceState();

			// Put the dialogue displayed state in a bundle using the preference key;
			Bundle state = new Bundle();
			state.PutBoolean( Key, dialogueWasBeingShown );

			return state;
		}

		/// <summary>
		/// Raises the restore instance state event.
		/// </summary>
		/// <param name="state">State.</param>
		protected override void OnRestoreInstanceState( IParcelable state )
		{
			// Need to keep the base class happy
			base.OnRestoreInstanceState( base.OnSaveInstanceState() );

			// Show the dialogue if it was being shown before
			if ( ( ( Bundle )state ).GetBoolean( Key ) == true )
			{
				ShowDialogue();
			}
		}

		//
		// Private methods
		//

		/// <summary>
		/// Shows the dialogue.
		/// </summary>
		private void ShowDialogue()
		{
			// Get the view layout to display in the dialogue
			View dialogueView = ( ( LayoutInflater  )Context.GetSystemService( Context.LayoutInflaterService ) ).Inflate( Resource.Layout.ListName, null );

			// Get the ListView and set its adpater and click handler
			ListView list = dialogueView.FindViewById< ListView >( Resource.Id.list );

			list.Adapter = adapter;
			list.ItemClick += ( object sender, AdapterView.ItemClickEventArgs e ) => 
			{
				// Report the list that has been selected
				if ( listener != null )
				{
					listener( ( string )list.GetItemAtPosition( e.Position ) );
				}
			};

			theDialog = new Dialog( Context, Android.Resource.Style.ThemeDeviceDefault );
			theDialog.SetTitle( Title );
			theDialog.SetContentView( dialogueView );

			theDialog.Show();
		}

		//
		// Private data
		//

		/// <summary>
		/// The dialog.
		/// </summary>
		private Dialog theDialog = null;

		/// <summary>
		/// The listener.
		/// </summary>
		private ListSelectedDelegate listener =  null;

		/// <summary>
		/// The adapter.
		/// </summary>
		private readonly ListNameAdapter adapter = null;

		/// <summary>
		/// The dialogue was being shown.
		/// </summary>
		private bool dialogueWasBeingShown = false;
	}
}

