using Android.App;
using Android.OS;
using Android.Views;

namespace AutoNag
{
	[Activity( Label = "AlertDialogueActivity", Theme = "@android:style/Theme.Translucent.NoTitleBar" )]			
	public class AlertDialogueActivity : Activity
	{
		protected override void OnCreate( Bundle bundle )
		{
			base.OnCreate( bundle );

			// Create your application here
		}

		protected override void OnStart()
		{
			base.OnStart();

			AlertDialog dialog = new AlertDialog.Builder( this )
				.SetPositiveButton( "Ok", ( senderAlert, args ) => { Finish(); } )
				.Create();

			dialog.RequestWindowFeature( ( int )WindowFeatures.NoTitle );
			WindowManagerLayoutParams wmlp = dialog.Window.Attributes;

			wmlp.Gravity = GravityFlags.Top | GravityFlags.Left;
			wmlp.X = Intent.SourceBounds.Left; 
			wmlp.Y = Intent.SourceBounds.Top;

			dialog.Show();
		}
	}

}

