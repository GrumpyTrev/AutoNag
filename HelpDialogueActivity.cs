// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        User Interface
// Filename:    HelpDialogueActivity.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The HelpDialogueActivity activity displays AutoNag information and copyright details.
//				 
// Description:  This nmeeds to ba an Activity as it is invoked from an AppWidget rather than another Activity (in which case it could have been a Fragment)
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

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Text;
using Android.Text.Util;

using System.IO;
using Android.Graphics;

namespace AutoNag
{
	/// <summary>
	/// The HelpDialogueActivity activity displays AutoNag information and copyright details.
	/// </summary>
	[Activity (Label = "@string/helpTitle", Theme = "@style/AppThemeActionBarNoIcon" ) ]
	public class HelpDialogueActivity : Activity
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public HelpDialogueActivity()
		{
		}

		//
		// Protected methods
		//

		/// <summary>
		/// Called when the Activity is created prior to being displayed.
		/// Note that this is also called when the device is rotated.
		/// </summary>
		/// <param name="savedInstanceState">Saved instance state.</param>
		protected override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate(savedInstanceState);

			SetContentView( Resource.Layout.About );

			// Extract the text from the 'legal' file, processing any embedded Html
			TextView legalView = FindViewById< TextView >( Resource.Id.legal_text );
			legalView.Text = Html.FromHtml( ReadRawTextFile( Resource.Raw.legal ) ).ToString();

			// Extract the text from the 'info' file, processing any embedded Html
			TextView infoView = FindViewById< TextView >( Resource.Id.info_text );
			infoView.Text = Html.FromHtml( ReadRawTextFile( Resource.Raw.info ) ).ToString();

			// Set the colour for any links
			infoView.SetLinkTextColor( Color.White );

			// Highlight any links in the info text
			Linkify.AddLinks( infoView, MatchOptions.All );
		}

		//
		// Private methods
		//

		/// <summary>
		/// Reads the raw text file with the specified resource identity
		/// </summary>
		/// <returns>The raw text file.</returns>
		/// <param name="id">Identifier.</param>
		private string ReadRawTextFile( int id ) 
		{
			string inputString = "";

			using ( Stream inputStream = Resources.OpenRawResource( id ) )
			{
				using ( StreamReader reader = new StreamReader( inputStream ) )
				{
					inputString = reader.ReadToEnd();
				}
			}

			return inputString;
		}
	}
}

