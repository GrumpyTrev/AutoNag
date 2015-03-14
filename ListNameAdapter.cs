// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        Settings
// Filename:    ListNameAdapter.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The ListNameAdapter class is a custom BaseAdapter that sets the background of an item according to the colour of the associated list.
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
using Android.Content.Res;
using Android.Graphics;

namespace AutoNag
{
	/// <summary>
	/// The ListNameAdapter class is a custom BaseAdapter that sets the background of an item according to the colour of the associated list.
	/// </summary>
	public class ListNameAdapter : BaseAdapter< string >
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AutoNag.ListNameAdapter"/> class.
		/// </summary>
		/// <param name="adapterContext">Adapter context.</param>
		public ListNameAdapter( Context adapterContext ) : base()
		{
			availableListNames = TaskRepository.GetTaskTables();
			listContext = adapterContext;
		}

		/// <summary>
		/// Gets the view and sets the background and text
		/// </summary>
		/// <returns>The view.</returns>
		/// <param name="convertView">Convert view.</param>
		/// <param name="position">The position of the item within the adapter's data set of the item whose view
		///  we want.</param>
		/// <param name="parent">Parent.</param>
		public override View GetView( int position, View convertView, ViewGroup parent )
		{
			View itemView = ( convertView != null ) ? convertView :
				( ( LayoutInflater  )listContext.GetSystemService( Context.LayoutInflaterService ) ).Inflate( Resource.Layout.ListNameItem, null );

			itemView.SetBackgroundResource( ListColourHelper.GetDrawableResource( availableListNames[ position ] ) );
			itemView.FindViewById< TextView >( Resource.Id.title ).Text = availableListNames[ position ];

			return itemView;
		}

		/// <summary>
		/// Gets the <see cref="AutoNag.ListNameAdapter"/> at the specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		public override string this[ int index ]
		{
			get
			{
				return availableListNames[ index ];
			}
		}

		/// <summary>
		/// How many items are in the data set represented by this Adapter.
		/// </summary>
		/// <value>To be added.</value>
		public override int Count
		{
			get
			{
				return availableListNames.Count;
			}
		}

		/// <summary>
		/// Get the row id associated with the specified position in the list.
		/// </summary>
		/// <param name="position">The position of the item within the adapter's data set whose row id we want.</param>
		/// <returns>To be added.</returns>
		public override long GetItemId( int position )
		{
			return position;
		}

		/// <summary>
		/// Notifies the attached observers that the underlying data has been changed
		///  and any View reflecting the data set should refresh itself.
		/// </summary>
		public override void NotifyDataSetChanged()
		{
			availableListNames = TaskRepository.GetTaskTables();
			base.NotifyDataSetChanged();
		}

		//
		// Private data
		//

		/// <summary>
		/// The available list names.
		/// </summary>
		private IList< string > availableListNames = null;

		/// <summary>
		/// The list context.
		/// </summary>
		private readonly Context listContext = null;
	}
}

