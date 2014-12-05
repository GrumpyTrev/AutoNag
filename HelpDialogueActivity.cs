using System;
using System.IO;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Text;
using Android.Text.Util;
using Android.Graphics;

namespace AutoNag
{
	[Activity (Label = "@string/helpTitle", Theme = "@style/AppThemeActionBarNoIcon" ) ]
	public class HelpDialogueActivity : Activity
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public HelpDialogueActivity()
		{
		}

		protected override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate(savedInstanceState);
			SetContentView( Resource.Layout.About );

			TextView legalView = FindViewById< TextView >( Resource.Id.legal_text );
			legalView.Text = Html.FromHtml( ReadRawTextFile( Resource.Raw.legal ) ).ToString();

			TextView infoView = FindViewById< TextView >( Resource.Id.info_text );
			infoView.Text = Html.FromHtml( ReadRawTextFile( Resource.Raw.info ) ).ToString();
			infoView.SetLinkTextColor( Color.White );

			Linkify.AddLinks( infoView, MatchOptions.All );
		}

		private String ReadRawTextFile( int id ) 
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

