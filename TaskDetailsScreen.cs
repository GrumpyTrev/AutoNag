// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        User Interface
// Filename:    TaskDetailsScreen.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The TaskDetailsScreen activity allows the user to display and edit a task
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
using Android.Content;
using Android.OS;
using Android.Widget;

using Android.Views;
using Android.Views.InputMethods;

using System;


namespace AutoNag
{
	/// <summary>
	/// The TaskDetailsScreen activity allows the user to display and edit a task
	/// </summary>
	[Activity (Label = "Task Details", WindowSoftInputMode = SoftInput.AdjustResize )]			
	public class TaskDetailsScreen : Activity, NotificationTimeDialogue.NotificationTimeDialogueListener, DueDateDialogue.IDueDateDialogueListener
	{
		//
		// Public methods
		//

		/// <summary>
		/// Initialize the contents of the Activity's standard options menu.
		/// </summary>
		/// <param name="menu">The options menu in which you place your items.</param>
		/// <returns>Base class result</returns>
		public override bool OnCreateOptionsMenu( IMenu menu ) 
		{
			// Inflate the menu items for use in the action bar
			MenuInflater.Inflate( Resource.Menu.DetailsScreenMenu, menu );

			// Hide delete item if new task
			if ( task.ID == 0 )
			{
				menu.FindItem( Resource.Id.deleteTask ).SetVisible( false );
			}

			// Get access to the Save item. This item is shown at start-up and removed at OnResume.
			// This is to resolve a problem on earlier androids whereby the save item is not displayed
			// when required. This should be solved in a better way when a fuller understanding of the 
			// problem is aquired
			saveItem = menu.FindItem( Resource.Id.saveTask );

			return base.OnCreateOptionsMenu( menu );
		}

		/// <summary>
		/// This hook is called whenever an item in the options menu is selected.
		/// </summary>
		/// <param name="item">The menu item that was selected.</param>
		/// <returns>True</returns>
		public override bool OnOptionsItemSelected( IMenuItem item )
		{
			base.OnOptionsItemSelected( item );

			if ( item.ItemId == Resource.Id.saveTask )
			{
				Save();
			}
			else if ( item.ItemId == Resource.Id.deleteTask )
			{
				Delete();
			}
			else if ( item.ItemId == Resource.Id.copyTask ) 
			{
				Copy();
			}
			else if ( item.ItemId == Resource.Id.moveTask ) 
			{
				Move();
			}

			return true;
		}

		/// <summary>
		/// Called when the activity has detected the user's press of the back key.
		/// If the save icon is being displayed prompt the user whether or not the changes should be saved
		/// </summary>
		public override void OnBackPressed()
		{
			if ( saveItem.IsVisible == true )
			{
				// Check if user wishes to save the changes
				ShowDialogue( new AlertDialog.Builder( this )
					.SetMessage( "Changes have been made to this task. Do you want to save these changes?" )
					.SetPositiveButton( "Yes", ( buttonSender, buttonEvents ) => { Save(); } )
					.SetNegativeButton( "No", ( buttonSender, buttonEvents ) => { base.OnBackPressed(); } )
					.SetNeutralButton( "Return to task details", ( buttonSender, buttonEvents ) => {} )
					.Create() ); 
			}
			else
			{
				base.OnBackPressed();
			}
		}

		/// <summary>
		/// Called when the user has set a notification time
		/// </summary>
		/// <param name="hours">Hours.</param>
		/// <param name="minutes">Minutes.</param>
		public void OnNotificationSet( int hours, int minutes )
		{
			// Save the selected hours and minutes in the DueDate field
			displayedDueDate = new DateTime( displayedDueDate.Year, displayedDueDate.Month, displayedDueDate.Day, hours, minutes, 0 );

			// Set the notification flag and update its assocoiated image
			displayedNotification = ( ( displayedDueDate.Hour != 0 ) || ( displayedDueDate.Minute != 0 ) );
			ShowNotificationImage();

			// Check if the save icon should be displayed
			UpdateSaveState();
		}

		/// <summary>
		/// Called when the user has cleared the notification time
		/// </summary>
		public void OnNotificationCleared()
		{
			// Remove the hours and minutes from the due date
			OnNotificationSet( 0, 0 );
		}

		/// <summary>
		/// Called when the user has set the due date
		/// </summary>
		/// <param name="dueDate">New due date.</param>
		public void OnDueDateSet( DateTime dueDate )
		{
			// If a notification was on the previous date then retain the notification hours and minutes
			displayedDueDate = dueDate + new TimeSpan( displayedDueDate.Hour, displayedDueDate.Minute, 0 );

            DisplayDueDate();

			// Check if the save icon should be displayed
			UpdateSaveState();
		}

		/// <summary>
		/// Called when the user has cleared the due date
		/// </summary>
		public void OnDueDateCleared()
		{
			displayedDueDate = DateTime.MinValue;

			// Clear the notification
			displayedNotification = false;

			DisplayDueDate();
			ShowNotificationImage();

			// Check if the save icon should be displayed
			UpdateSaveState();
		}

		//
		// Protected methods
		//

		/// <summary>
		/// Called when the activity is first created - before it is displayed
		/// Initialise the visible component references
		/// </summary>
		/// <param name="bundle">Bundle.</param>
		protected override void OnCreate( Bundle bundle )
		{
			base.OnCreate( bundle );

			// Load the task associated with the intent
			CarryOutIntentActions();

			// Set the layout to be the TaskDetails screen
			SetContentView( Resource.Layout.TaskDetails );

			// Get references to the view components that are used elsewhere
			nameTextEdit = FindViewById< EditText >( Resource.Id.detailsName );
			notesTextEdit = FindViewById< EditText >( Resource.Id.detailsNote );
			taskDone = FindViewById< CheckBox >( Resource.Id.detailsDone );
			dueDateDisplay = FindViewById< TextView >( Resource.Id.detailsDueDate );
			priorityImage = FindViewById< ImageView >( Resource.Id.detailsImagePriority );
			notificationImage = FindViewById< ImageView >( Resource.Id.detailsImageNotification );

			// Load the state of the activity from provided bundle
			LoadState( bundle );

			// Display the state
			DisplayDueDate();
			ShowPriorityImage();
			ShowNotificationImage();

			// Set up handlers for user interaction
			nameTextEdit.AfterTextChanged += ( sender, args) => { UpdateSaveState(); };
			notesTextEdit.AfterTextChanged += ( sender, args) => { UpdateSaveState(); };
			dueDateDisplay.Click += ( sender, args ) => { ChangeDate(); };
			taskDone.CheckedChange += ( sender, args ) => { UpdateSaveState(); };
			FindViewById< TextView >( Resource.Id.detailsPriorityLabel ).Click += ( sender, args ) => { ChangePriority(); };
			priorityImage.Click += ( sender, args ) => { ChangePriority(); };
			notificationImage.Click += ( sender, args ) => { ChangeNotification(); };
		}

		/// <summary>
		/// Called once the activity is made visible.
		/// For new tasks attempt to display the keyboard to enable the user to enter the task name
		/// </summary>
		protected override void OnResume()
		{
			base.OnResume();

			// If this is a new task (i.e. task name and memo both empty then shift the focus to the
			// task name as this is likely what the user wants to do.
			if ( ( nameTextEdit.Text.Length == 0 ) && ( notesTextEdit.Text.Length == 0 ) )
			{
				nameTextEdit.RequestFocus();

				// Display the keyboard after the view has got focus - 200ms delay here
				nameTextEdit.PostDelayed( () =>
				{
					( ( InputMethodManager )GetSystemService( Context.InputMethodService ) ).ShowSoftInput( nameTextEdit, ShowFlags.Implicit );
				}, 200 );
			}

			// To get around a problem with earlier androids the save item is displayed at activity start and then removed after a delay
			nameTextEdit.PostDelayed( () =>
			{
				UpdateSaveState();
			}, 50 );
		}

		/// <summary>
		/// Raises the save instance state event.
		/// The activity will automatically save the content of the views, so just save any state items that are not help in View objects 
		/// </summary>
		/// <param name="outState">Bundle in which to place your saved state.</param>
		protected override void OnSaveInstanceState( Bundle outState )
		{
			base.OnSaveInstanceState( outState );

			outState.PutString( DueDateName, displayedDueDate.ToString( DueDateFormat ) );
			outState.PutInt( PriorityName, displayedPriority );
			outState.PutBoolean( NotificationName, displayedNotification );
		}

		/// <summary>
		/// Called as part of the activity lifecycle when an activity is going into the background, but has not (yet) been killed.
		/// Dismiss any dialogues being shown
		/// </summary>
		protected override void OnPause()
		{
			base.OnPause();

			if ( dialogueBeingShown != null )
			{
				if ( dialogueBeingShown.IsShowing == true )
				{
					dialogueBeingShown.Dismiss();
				}

				dialogueBeingShown = null;
			}
		}

		//
		// Private methods
		//

		/// <summary>
		/// The supplied Intent contains the identity of the task to load and whether or not this activity has been started in response
		/// to a notification
		/// </summary>
		private void CarryOutIntentActions()
		{
			WidgetIntent wrappedIntent = new WidgetIntent( Intent );

			taskListName = wrappedIntent.TaskListNameProperty;

			// Get the task identity and if it is non-zero load it.
			int taskID = wrappedIntent.TaskIdentityProperty;

			if ( taskID > 0 ) 
			{
				task = TaskRepository.GetTask( taskListName, taskID );

				// If this activity has been started from a notification then clear the notification flag.
				if ( wrappedIntent.NotificationProperty == true )
				{
					// Make sure that a notification was due
					if ( task.NotificationRequired == true )
					{
						task.NotificationRequired = false;
						task.DueDate = new DateTime( task.DueDate.Year, task.DueDate.Month, task.DueDate.Day, 0, 0, 0 );

						// Save these updated task fields before displaying anything to the user
						TaskRepository.SaveTask( task );

						// Tell everyone about it
						NotifyChanges();
					}

					// Remove the notification
					( ( NotificationManager )ApplicationContext.GetSystemService( Context.NotificationService ) ).Cancel( WidgetIntent.GetRequestCode( taskID, taskListName ) );
				}
			}
			else
			{
				// Make sure that the empty task is initialised with the correct list name
				task.ListName = taskListName;
			}
		}

		/// <summary>
		/// Loads the state from the supplied bundle
		/// </summary>
		/// <param name="stateBundle">State bundle.</param>
		private void LoadState( Bundle stateBundle )
		{
			// If the Bundle is null, i.e. this is the first time the activity has been displayed, then load state from the task 
			if ( stateBundle == null )
			{
				notesTextEdit.Text = task.Notes;
				nameTextEdit.Text = task.Name; 
				taskDone.Checked = task.Done;
				displayedDueDate = task.DueDate;
				displayedPriority = task.Priority;
				displayedNotification = task.NotificationRequired;
			}
			else
			{
				// Load the non-view state from the bundle
				displayedDueDate = DateTime.ParseExact( stateBundle.GetString( DueDateName ), DueDateFormat, System.Globalization.CultureInfo.InvariantCulture );
				displayedPriority = stateBundle.GetInt( PriorityName );
				displayedNotification = stateBundle.GetBoolean( NotificationName );
			}
		}
			
		/// <summary>
		/// Called whenever any changes are made to the task so that the save icon can be shown or hidden appropriately
		/// </summary>
		private void UpdateSaveState()
		{
			taskChanged = false;

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

		/// <summary>
		/// Called when the due date has been clicked.
		/// Allow the user to set or clear the due date
		/// </summary>
		private void ChangeDate()
		{
			DueDateDialogue.CreateInstance( displayedDueDate ).Show( FragmentManager.BeginTransaction(), "DueDateDialogue" );
		}

		/// <summary>
		/// Called to toggle the priority of this task
		/// </summary>
		private void ChangePriority()
		{
			// Toggle the priority and display the associated image
			displayedPriority = ( displayedPriority + 1 ) % 2;
			ShowPriorityImage();

			// Check if the save icon should be displayed
			UpdateSaveState();
		}

		/// <summary>
		/// Called when the notification image has been clicked.
		/// Allow the user to set or clear the notification time
		/// </summary>
		private void ChangeNotification()
		{
			// Don't show the dialogue it the due date is not set
			if ( displayedDueDate != DateTime.MinValue )
			{
				NotificationTimeDialogue.CreateInstance( displayedDueDate ).Show( FragmentManager.BeginTransaction(), "NotificationTimeDialogue" );
			}
		}

		/// <summary>
		/// Shows the priority image.
		/// </summary>
		private void ShowPriorityImage()
		{
			priorityImage.SetImageResource( ( displayedPriority == 0 ) ? Resource.Drawable.StarOff :  Resource.Drawable.StarOn );
		}

		/// <summary>
		/// Shows the notification image.
		/// </summary>
		private void ShowNotificationImage()
		{
			notificationImage.SetImageResource( ( displayedNotification == false ) ? Resource.Drawable.NotificationOff : Resource.Drawable.NotificationOn ); 
		}

		/// <summary>
		/// Displays the due date.
		/// </summary>
		private void DisplayDueDate()
		{
			dueDateDisplay.Text = ( displayedDueDate == DateTime.MinValue ) ? "" : displayedDueDate.ToString( DisplayDueDateFormat );
		}

		/// <summary>
		/// Called when the user has selected the save option (or when the activity has been closed with unsaved items)
		/// </summary>
		private void Save()
		{
			// If this is a newly created task from a widget displaying the combined list then we need to ask the user
			// which list to use.
			if ( ( task.ID == 0 ) && ( taskListName == SettingsActivity.CombinedListName ) )
			{
				ShowDialogue( new ListPreference( this, "Select list for new task", OnSave )
					.CreateDialogue( Android.Resource.Style.ThemeDeviceDefaultDialogMinWidth ) );
			}
			else
			{
				// Call the common event handler
				OnSave( taskListName );
			}
		}

		/// <summary>
		/// Update the task with the state held by this activity and save it.
		/// Check for any changes that may need notifying to the AlarmInterface
		/// </summary>
		private void OnSave( string saveListName )
		{
			// Make sure the list name is set in the task (in case its a new task) and is used for all notifications
			task.ListName = saveListName;
			taskListName = saveListName;

			// Check for any notification changes that require alarm updates.
			// Do this before updating the task otherwise the changes will be lost
			bool setAlarm = false;
			bool cancelAlarm = false;

			if ( displayedNotification != task.NotificationRequired )
			{
				// If the notification has been turned off then cancel the alarm. If it has been turned on then raise the alarm
				if ( displayedNotification == false )
				{
					cancelAlarm = true;
				}
				else
				{
					setAlarm = true;
				}
			}
			// If the due date has changed and a notification is required then raise the alarm
			else if ( ( task.DueDate != displayedDueDate ) && ( displayedNotification == true ) )
			{
				setAlarm = true;
			}

			// Update the task with the displayed values
			task.Name = nameTextEdit.Text;
			task.Notes = notesTextEdit.Text;
			task.NotificationRequired = displayedNotification;
			task.Priority = displayedPriority;
			task.Done = taskDone.Checked;
			task.DueDate = displayedDueDate;
			task.ModifiedDate = DateTime.Now;

			// Save the task
			TaskRepository.SaveTask( task );

			// Tell everyone about it
			NotifyChanges();

			if ( cancelAlarm == true )
			{
				AlarmInterface.CancelAlarm( taskListName, task.ID, ApplicationContext );
			}
			else if ( setAlarm == true )
			{
				AlarmInterface.SetAlarm( taskListName, task.ID, task.Name, task.DueDate, ApplicationContext );
			}

			Finish();
		}

		/// <summary>
		/// Called when the delete action bar option has been selected.
		/// Confirm this with the user
		/// </summary>
		private void Delete()
		{
			// Confirm deletion with the user
			ShowDialogue( new AlertDialog.Builder( this )
				.SetMessage( "Do you really want to delete this task?" )
				.SetPositiveButton( "Yes", ( buttonSender, buttonEvents ) =>
				{
					// Delete the task
					TaskRepository.DeleteTask( taskListName, task.ID );

					// Cancel any alarms associated with this task
					AlarmInterface.CancelAlarm( taskListName, task.ID, ApplicationContext );

					// Tell everyone about it
					NotifyChanges();

					Finish();
				} )
				.SetNegativeButton( "No", ( buttonSender, buttonEvents ) => {} )
				.Create() ); 
		}

		/// <summary>
		/// Called when the copy action bar option has been selected.
		/// Ask the use to choose a new list
		/// </summary>
		private void Copy()
		{
			ShowDialogue( new ListPreference( this, string.Format( "Copy task [{0}] from list [{1}] to list", nameTextEdit.Text, taskListName ), OnCopyList )
				.CreateDialogue( Android.Resource.Style.ThemeDeviceDefaultDialogMinWidth ) );
		}

		/// <summary>
		/// Called when the move action bar option has been selected.
		/// Ask the use to choose a new list
		/// </summary>
		private void Move()
		{
			ShowDialogue( new ListPreference( this, string.Format( "Move task [{0}] from list [{1}] to list", nameTextEdit.Text, taskListName ), OnMoveList )
				.CreateDialogue( Android.Resource.Style.ThemeDeviceDefaultDialogMinWidth ) );
		}

		/// <summary>
		/// The user has selected a task to copy to another list
		/// </summary>
		private void OnCopyList( string newTaskListName )
		{
			OnCopyOrDelete( newTaskListName, true );
		}

		/// <summary>
		/// The user has selected a task to move to another list
		/// </summary>
		private void OnMoveList( string newTaskListName )
		{
			OnCopyOrDelete( newTaskListName, false );
		}

		/// <summary>
		/// Raises the copy or delete event.
		/// </summary>
		/// <param name="copyOnly">If set to <c>true</c> copy only.</param>
		private void OnCopyOrDelete( string newTaskListName, bool copyOnly )
		{
			dialogueBeingShown.Dismiss();
			dialogueBeingShown = null;

			// Create a new task and copy all the current details to it.
			// Take the details from the current screen controls
			Task newTask = new Task();

			// Update the task with the displayed values
			newTask.Name = nameTextEdit.Text;
			newTask.Notes = notesTextEdit.Text;
			newTask.NotificationRequired = displayedNotification;
			newTask.Priority = displayedPriority;
			newTask.Done = taskDone.Checked;
			newTask.DueDate = displayedDueDate;

			// If the task has been changed then set the modified date to now.  Otherwise copy it from the existing task
			newTask.ModifiedDate = ( taskChanged == true ) ? DateTime.Now : task.ModifiedDate;

			newTask.ListName = newTaskListName;

			// Save the new task
			if ( TaskRepository.SaveTask( newTask ) == true )
			{
				if ( copyOnly == false )
				{
					// Carry out deletion specific actions
					// Delete the original
					TaskRepository.DeleteTask( taskListName, task.ID );

					// Tell everyone about the deletion
					SendBroadcast( new WidgetIntent( AutoNagWidget.UpdatedAction ).SetTaskListName( taskListName ) );

					// Cancel any alarms associated with the deleted task
					AlarmInterface.CancelAlarm( taskListName, task.ID, ApplicationContext );
				}

				Toast.MakeText( this, string.Format( "Task [{0}] {3} from list [{1}] to list [{2}]", newTask.Name, taskListName, newTaskListName,
					copyOnly == true ? "copied" : "moved" ), ToastLength.Short ).Show();
			}

			// Tell everyone about the new task
			SendBroadcast( new WidgetIntent( AutoNagWidget.UpdatedAction ).SetTaskListName( newTaskListName ) );

			if ( newTask.NotificationRequired == true )
			{
				AlarmInterface.SetAlarm( newTaskListName, newTask.ID, newTask.Name, newTask.DueDate, ApplicationContext );
			}
		}

		/// <summary>
		/// Broadcasts and intent to notify interested receivers there has been a change.  
		/// Note that the actual changed item it not indicated.
		/// </summary>
		private void NotifyChanges()
		{
			SendBroadcast( new WidgetIntent( AutoNagWidget.UpdatedAction ).SetTaskListName( taskListName ) );
		}

		/// <summary>
		/// Shows the dialogue and records the dialogue being shown in order to dismiss it.
		/// </summary>
		/// <param name="dialogueToShow">Dialogue to show.</param>
		private void ShowDialogue( Dialog dialogueToShow )
		{
			dialogueBeingShown = dialogueToShow;
			dialogueToShow.Show();
		}

		//
		// Private data
		//

		/// <summary>
		/// The task being shown/edited
		/// </summary>
		private Task task = new Task();

		/// <summary>
		/// The name of the task list.
		/// </summary>
		private string taskListName = "";

		/// <summary>
		/// The views holding the displayed state of the task
		/// </summary>
		private EditText notesTextEdit;
		private EditText nameTextEdit;
		private CheckBox taskDone;
		private TextView dueDateDisplay;
		private ImageView priorityImage;
		private ImageView notificationImage;

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

		/// <summary>
		/// Keep track of any dialogues being shown so that they can be dismissed on configuration change
		/// </summary>
		private Dialog dialogueBeingShown = null;

		/// <summary>
		/// Keep track of whether or not the task items have changed
		/// </summary>
		private bool taskChanged = false;

		/// <summary>
		/// The due date format in the bundle
		/// </summary>
		private const string DueDateFormat = "yyyyMMddHHmm";

		/// <summary>
		/// The display due date format.
		/// </summary>
		private const string DisplayDueDateFormat = @"dd/MM/yyyy";

		/// <summary>
		/// The names of Bundled states.
		/// </summary>
		private const string DueDateName = "displayedDueDate";
		private const string PriorityName = "displayedPriority";
		private const string NotificationName = "displayedNotification";
	}
}