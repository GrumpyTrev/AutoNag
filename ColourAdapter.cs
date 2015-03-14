// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        Settings
// Filename:    ColourAdapter.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The ColourAdapter class is a custom ArrayAdapter that sets the background of an item according to the colour of the associated list.
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
using Android.Widget;
using Android.Content;
using System.Collections.Generic;
using Android.Views;
using System;
using Android.Graphics;

namespace AutoNag
{
	/// <summary>
	/// The ColourAdapter class is a custom ArrayAdapter that sets the background of an item according to the colour of the associated list.
	/// </summary>
	public class ColourAdapter : ArrayAdapter< string >
	{
		//
		// Public methods
		//

		/// <summary>
		/// Fill the adapter with the names of the colours
		/// </summary>
		/// <param name="adapterContext">Adapter context.</param>
		public ColourAdapter( Context adapterContext ) : base( adapterContext, Android.Resource.Layout.SelectDialogSingleChoice )
		{
			foreach ( string colour in Enum.GetNames( typeof( ListColourEnum ) ) )
			{
				Add( colour );
			}
		}

		/// <summary>
		/// Gets the view.
		/// </summary>
		/// <returns>The view.</returns>
		/// <param name="convertView">Convert view.</param>
		/// <param name="position">The position of the item within the adapter's data set of the item whose view
		///  we want.</param>
		/// <param name="parent">Parent.</param>
		public override View GetView( int position, View convertView, ViewGroup parent )
		{
			// Let the base class provide the view
			View itemView = base.GetView( position, convertView, parent );

			// Set the background and text colour
			itemView.SetBackgroundResource( ListColourHelper.GetDrawableResource( ListColourHelper.StringToColourEnum( GetItem( position ) ) ) );
			itemView.FindViewById< TextView >( Android.Resource.Id.Text1 ).SetTextColor( Color.Black );

			return itemView;
		}
	}
}

