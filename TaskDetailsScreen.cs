using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

using Android.Views;
using Android.Views.InputMethods;

using System;


namespace AutoNag
{
	/// <summary>
	/// View/edit a Task
	/// </summary>
	[Activity (Label = "Task Details", WindowSoftInputMode = SoftInput.AdjustResize )]			
	public class TaskDetailsScreen : Activity 
	{
		public override bool OnCreateOptionsMenu( IMenu menu ) 
		{
			// Inflate the menu items for use in the action bar
			MenuInflater.Inflate( Resource.Menu.DetailsScreenMenu, menu );

			// Hide delete item if new task
			if ( task.ID == 0 )
			{
				IMenuItem item = menu.FindItem( Resource.Id.deleteTask );
				if ( item != null )
				{
					item.SetVisible( false );
				}
			}

			// Hide or show the save item
			saveItem = menu.FindItem( Resource.Id.saveTask );
			UpdateSaveState();

			return base.OnCreateOptionsMenu( menu );
		}

		public override bool OnOptionsItemSelected( IMenuItem item )
		{
			base.OnOptionsItemSelected( item );

			switch ( item.ItemId )
			{
				case Resource.Id.saveTask:
				{
					Save();
					break;
				}

				case Resource.Id.deleteTask:
				{
					Delete();
					break;
				}

				default: break;
			}

			return true;
		}

		/// <summary>
		/// Called when the activity has detected the user's press of the back
		///  key.
		/// If the save icon is being displayed prompt the user whether or not the changes should be saved
		/// </summary>
		public override void OnBackPressed()
		{
			if ( saveItem.IsVisible == true )
			{
				// Check if user wishes to save the changes
				new AlertDialog.Builder( this )
					.SetMessage( "Changes have been made to this task. Do you want to save these changes?" )
					.SetPositiveButton( "Yes", ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
					{
						Save();
					} )
					.SetNegativeButton( "No", ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
					{
						// Continue with system action
						base.OnBackPressed();
					} )
					.SetNeutralButton( "Return to task details", ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
					{
						// Consume the event
					} )
					.Show(); 
			}
			else
			{
				base.OnBackPressed();
			}
		}

		//
		// Protected methods
		//

		/// <summary>
		/// Called when the activity is first created - before it is displayed
		/// Initialise the visible componnet references
		/// </summary>
		/// <param name="bundle">Bundle.</param>
		protected override void OnCreate( Bundle bundle )
		{
			base.OnCreate( bundle );

			// Get the task identity from the intent and if it is non-zero get the task from the database
			int taskID = Intent.GetIntExtra( "TaskID", 0 );

			if ( taskID > 0 ) 
			{
				task = TaskManager.GetTask( taskID );

				// Check whether or not this is being called from a notification
				if ( Intent.GetIntExtra( "Notification", 0 ) == 1 )
				{
					if ( task.NotificationRequired == true )
					{
						task.NotificationRequired = false;
						task.DueDate = new DateTime( task.DueDate.Year, task.DueDate.Month, task.DueDate.Day, 0, 0, 0 );

						TaskManager.SaveTask( task );
						NotifyChanges();
					}

					// Remove the notification
					( ( NotificationManager )ApplicationContext.GetSystemService( Context.NotificationService ) ).Cancel( taskID );
				}
			}
			
			// Set the layout to be the TaskDetails screen
			SetContentView( Resource.Layout.TaskDetails );

			// Get references to the view components and use them to display the task's contents
			nameTextEdit = FindViewById< EditText >( Resource.Id.detailsName );
			nameTextEdit.Text = task.Name; 

			notesTextEdit = FindViewById< EditText >( Resource.Id.detailsNote );
			notesTextEdit.Text = task.Notes;

			taskDone = FindViewById< CheckBox >( Resource.Id.detailsDone );
			taskDone.Checked = task.Done;

			dueDateDisplay = FindViewById< TextView >( Resource.Id.detailsDueDate );
			displayedDueDate = task.DueDate;
			DisplayDueDate();

			priorityLabel =  FindViewById< TextView >( Resource.Id.detailsPriorityLabel );
			priorityImage = FindViewById< ImageView >( Resource.Id.detailsImagePriority );
			displayedPriority = task.Priority;
			ShowPriorityImage();

			notificationImage = FindViewById< ImageView >( Resource.Id.detailsImageNotification );
			displayedNotification = task.NotificationRequired;
			ShowNotificationImage();

			// Set up handlers for user interaction
			nameTextEdit.AfterTextChanged += ( object sender, Android.Text.AfterTextChangedEventArgs args) => 
			{
				UpdateSaveState();
			};

			notesTextEdit.AfterTextChanged += ( object sender, Android.Text.AfterTextChangedEventArgs args) => 
			{
				UpdateSaveState();
			};

			dueDateDisplay.Click += ( object sender, EventArgs args ) => 
			{ 
				ChangeDate();
			};

			taskDone.CheckedChange += ( object sender, CompoundButton.CheckedChangeEventArgs args ) => 
			{
				UpdateSaveState();
			};

			priorityLabel.Click += ( object sender, EventArgs args ) => 
			{ 
				ChangePriority(); 
			};

			// Click events required for both priority images
			priorityImage.Click += ( object sender, EventArgs args ) => 
			{ 
				ChangePriority(); 
			};

			// Click events required for both notification images
			notificationImage.Click += ( object sender, EventArgs args ) => 
			{ 
				ChangeNotification(); 
			};
		}

		/// <summary>
		/// Called once the activity is made visible.
		/// For new tasks attempt to display the keyboard to enable the user to enter the task name
		/// </summary>
		protected override void OnResume()
		{
			base.OnResume();

			// If this is a new task (i.e. task name and memo both epty then shift the focus to the
			// task name as this is likely what the user wants to do.
			if ( ( nameTextEdit.Text.Length == 0 ) && ( notesTextEdit.Text.Length == 0 ) )
			{
				nameTextEdit.RequestFocus();

				// Display the keyboard after the view has got focus - 200ms delay here
				nameTextEdit.PostDelayed( () =>
				{
					InputMethodManager manager = ( InputMethodManager )GetSystemService( Context.InputMethodService );
					manager.ShowSoftInput( nameTextEdit, ShowFlags.Implicit );
				}, 200 );
			}	
		}

		//
		// Private methods
		//

		/// <summary>
		/// Called whenever any changes are made to the task so that the save icon can be shown or hidden appropriately
		/// </summary>
		private void UpdateSaveState()
		{
			bool taskChanged = false;

			if ( ( nameTextEdit.Text != task.Name ) || ( notesTextEdit.Text != task.Notes ) || ( displayedNotification != task.NotificationRequired ) ||
				( displayedPriority != task.Priority ) || ( task.Done != taskDone.Checked ) || ( task.DueDate != displayedDueDate ) )
			{
				taskChanged = true;
			}

			if ( saveItem != null )
			{
				saveItem.SetVisible( taskChanged );
			}
		}

		private void ChangeDate()
		{
			View calendarLayout = LayoutInflater.Inflate( Resource.Layout.CalendarView, null );

			CalendarView calView = calendarLayout.FindViewById< CalendarView >( Resource.Id.calendarView );
			if ( displayedDueDate == DateTime.MinValue )
			{
				calView.Date = ( long )( DateTime.Now - new DateTime( 1970, 1, 1 ) ).TotalMilliseconds;
			}
			else
			{
				calView.Date = ( long )( displayedDueDate - new DateTime( 1970, 1, 1 ) ).TotalMilliseconds;
			}

			new AlertDialog.Builder( this )
				.SetTitle( "Due date" )
				.SetView( calendarLayout )
				.SetPositiveButton( "Set",  ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
				{
					// If notification was on the previous date then retain the notification hours and minutes
					int hours = displayedDueDate.Hour;
					int mins = displayedDueDate.Minute;

					displayedDueDate = new DateTime( calView.Date * 10000 + new DateTime( 1970, 1, 1 ).Ticks );
					displayedDueDate += new TimeSpan( hours, mins, 0 );
					
					DisplayDueDate();
					UpdateSaveState();
				} )
				.SetNegativeButton( "Cancel",  ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
				{
					// No action
				} )
				.SetNeutralButton( "Clear",  ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
				{
					displayedDueDate = DateTime.MinValue;

					// Clear the notification
					displayedNotification = false;

					DisplayDueDate();
					ShowNotificationImage();
					UpdateSaveState();
				} )
				.Show(); 
		}

		private void ChangePriority()
		{
			displayedPriority = ( displayedPriority + 1 ) % 2;

			ShowPriorityImage();
			UpdateSaveState();
		}

		private void ChangeNotification()
		{
			// Don't show the dialogue it the due date is not set
			if ( displayedDueDate != DateTime.MinValue )
			{
				View notificationLayout = LayoutInflater.Inflate( Resource.Layout.NotificationSelection, null );

				TimePicker notificationTimePicker = notificationLayout.FindViewById< TimePicker >( Resource.Id.NotificationTimePicker );
				notificationTimePicker.SetIs24HourView( Java.Lang.Boolean.True );

				// If the DueDate contains any non-zero hours and minutes fields then initialise the picker to those values, otherwise
				// it will show the current time
				if ( ( displayedDueDate.Hour != 0 ) || ( displayedDueDate.Minute != 0 ) )
				{
					notificationTimePicker.CurrentHour = ( Java.Lang.Integer )displayedDueDate.Hour;
					notificationTimePicker.CurrentMinute = ( Java.Lang.Integer )displayedDueDate.Minute;
				}
				else
				{
					// Make sure that the hour is in the correct format
					notificationTimePicker.CurrentHour = ( Java.Lang.Integer )DateTime.Now.Hour;
				}

				new AlertDialog.Builder( this )
					.SetTitle( "Notification time" )
					.SetView( notificationLayout )
					.SetPositiveButton( "Set",  ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
					{
						notificationTimePicker.ClearFocus();

						// Save the selected hours and minutes in the DueDate field
						displayedDueDate = new DateTime( displayedDueDate.Year, displayedDueDate.Month, displayedDueDate.Day, ( int )notificationTimePicker.CurrentHour,
							( int )notificationTimePicker.CurrentMinute, 0 );

						displayedNotification = ( ( displayedDueDate.Hour != 0 ) || ( displayedDueDate.Minute != 0 ) );

						ShowNotificationImage();
						UpdateSaveState();
					} )
					.SetNegativeButton( "Cancel",  ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
					{
						// No action
					} )
					.SetNeutralButton( "Clear",  ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
					{
						displayedDueDate = new DateTime( displayedDueDate.Year, displayedDueDate.Month, displayedDueDate.Day, 0, 0, 0 );

						displayedNotification = false;

						ShowNotificationImage();
						UpdateSaveState();
					} )
					.Show(); 
			}
		}

		private void ShowPriorityImage()
		{
			if ( displayedPriority == 0 )
			{
				priorityImage.SetImageResource( Resource.Drawable.StarOff );
			}
			else
			{
				priorityImage.SetImageResource( Resource.Drawable.StarOn );
			}
		}

		private void ShowNotificationImage()
		{
			if ( displayedNotification == false )
			{
				notificationImage.SetImageResource( Resource.Drawable.NotificationOff ); 
			}
			else
			{
				notificationImage.SetImageResource( Resource.Drawable.NotificationOn ); 
			}
		}

		private void DisplayDueDate()
		{
			if ( displayedDueDate == DateTime.MinValue )
			{
				dueDateDisplay.Text = "";
			}
			else
			{
				dueDateDisplay.Text = displayedDueDate.ToString( @"dd/MM/yyyy" );
			}
		}

		private void Save()
		{
			// Check for any notification changes that require alarm updates
			if ( displayedNotification != task.NotificationRequired )
			{
				// If the notification has been turned off then cancel the alarm. If it has been turned on then raise the alarm
				if ( displayedNotification == false )
				{
					AlarmInterface.CancelAlarm( task.ID, ApplicationContext );
				}
				else
				{
					AlarmInterface.SetAlarm( task.ID, task.Name, displayedDueDate, ApplicationContext );
				}
			}
			else if ( ( task.DueDate != displayedDueDate ) && ( displayedNotification == true ) )
			{
				AlarmInterface.SetAlarm( task.ID, task.Name, displayedDueDate, ApplicationContext );
			}

			// Update the task with the displayed values
			task.Name = nameTextEdit.Text;
			task.Notes = notesTextEdit.Text;
			task.NotificationRequired = displayedNotification;
			task.Priority = displayedPriority;
			task.Done = taskDone.Checked;
			task.DueDate = displayedDueDate;
			task.ModifiedDate = DateTime.Now;

			TaskManager.SaveTask(task);
			NotifyChanges();
			Finish();
		}

		private void Delete()
		{
			// Confirm deletion with the user
			new AlertDialog.Builder( this )
				.SetMessage( "Do you really want to delete this task?" )
				.SetPositiveButton( "Yes", ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
				{
					TaskManager.DeleteTask( task.ID );
					NotifyChanges();

					Finish();
				} )
				.SetNegativeButton( "No", ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
				{
					// No action
				} )
				.Show(); 
		}

		private void NotifyChanges()
		{
			SendBroadcast( new Intent( AutoNagWidget.UpdatedAction ) );
		}

		//
		// Private data
		//

		private Task task = new Task();
		private EditText notesTextEdit;
		private EditText nameTextEdit;
		private CheckBox taskDone;
		private TextView dueDateDisplay;
		private ImageView priorityImage;
		private ImageView notificationImage;
		private TextView priorityLabel;

		/// <summary>
		/// The priority value being displayed
		/// </summary>
		private int displayedPriority = 0;

		/// <summary>
		/// The displayed notification value.
		/// </summary>
		private bool displayedNotification = false;

		/// <summary>
		/// The displayed due date value
		/// </summary>
		private DateTime displayedDueDate = DateTime.MinValue;

		/// <summary>
		/// The save action bar item
		/// </summary>
		private IMenuItem saveItem = null;
	}
}