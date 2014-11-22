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
				AlertDialog.Builder confirmationDialogue = new AlertDialog.Builder( this );
				confirmationDialogue.SetMessage( "Changes have been made to this task. Do you want to save these changes?" );

				confirmationDialogue.SetPositiveButton( "Yes",  ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
				{
					Save();
				} );

				confirmationDialogue.SetNegativeButton( "No",  ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
				{
					// Continue with system action
					base.OnBackPressed();
				} );

				confirmationDialogue.SetNeutralButton( "Return to task details",  ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
				{
					// Consume the event
				} );

				confirmationDialogue.Show(); 
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
			AlertDialog.Builder createdDialogue = new AlertDialog.Builder( this );
			createdDialogue.SetTitle( "Due date" );

			View calendarLayout = LayoutInflater.Inflate( Resource.Layout.CalendarView, null );
			createdDialogue.SetView( calendarLayout );

			CalendarView calView = calendarLayout.FindViewById< CalendarView >( Resource.Id.calendarView );
			if ( displayedDueDate == DateTime.MinValue )
			{
				calView.Date = ( long )( DateTime.Now - new DateTime( 1970, 1, 1 ) ).TotalMilliseconds;
			}
			else
			{
				calView.Date = ( long )( displayedDueDate - new DateTime( 1970, 1, 1 ) ).TotalMilliseconds;
			}

			createdDialogue.SetPositiveButton( "Set",  ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
			{
				displayedDueDate = new DateTime( calView.Date * 10000 + new DateTime( 1970, 1, 1 ).Ticks );
				DisplayDueDate();
				UpdateSaveState();
			} );

			createdDialogue.SetNegativeButton( "Cancel",  ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
			{
				// No action
			} );

			createdDialogue.SetNeutralButton( "Clear",  ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
			{
				displayedDueDate = DateTime.MinValue;
				DisplayDueDate();
				UpdateSaveState();
			} );

			createdDialogue.Show(); 
		}

		private void ChangePriority()
		{
			displayedPriority = ( displayedPriority + 1 ) % 2;

			ShowPriorityImage();
			UpdateSaveState();
		}

		private void ChangeNotification()
		{
			displayedNotification = !displayedNotification;

			ShowNotificationImage();
			UpdateSaveState();
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
			AlertDialog.Builder confirmationDialogue = new AlertDialog.Builder( this );
			confirmationDialogue.SetMessage( "Do you really want to delete this task?" );

			confirmationDialogue.SetPositiveButton( "Yes",  ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
			{
				TaskManager.DeleteTask(task.ID);
				NotifyChanges();

				Finish();
			} );

			confirmationDialogue.SetNegativeButton( "No",  ( object buttonSender, DialogClickEventArgs buttonEvents ) =>
			{
				// No action
			} );

			confirmationDialogue.Show(); 
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