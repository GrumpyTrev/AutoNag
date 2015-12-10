// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        User Interface
// Filename:    DueTimeDialogue.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The DueDateDialogue class allows the user to set the due date for a task.
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
using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace AutoNag
{
	/// <summary>
	/// The DueDateDialogue class allows the user to set the due date for a task.
	/// </summary>
	public class DueDateDialogue : DialogFragment
	{
		/// <summary>
		/// Interface to pass back dialogue results
		/// </summary>
		public interface DueDateDialogueListener
		{
			/// <summary>
			/// Raises the due date set event.
			/// </summary>
			/// <param name="dueDate">New due date.</param>
			void OnDueDateSet( DateTime dueDate );

			/// <summary>
			/// Raises the due date cleared event.
			/// </summary>
			void OnDueDateCleared();
		}

		//
		// Public methods
		//

		/// <summary>
		/// Creates a DueDateDialogue passing the initial date as Bundle arguments
		/// </summary>
		/// <returns>The instance.</returns>
		/// <param name="initialDate">Initial date.</param>
		public static DueDateDialogue CreateInstance( DateTime initialDate )
		{
			// If the date has not been set then use the current date
			Bundle dialogueArgs = new Bundle();
			dialogueArgs.PutLong( DueDateLabel, ( long )( ( ( initialDate == DateTime.MinValue ) ? DateTime.Today : initialDate.Date ) - Seventies ).TotalMilliseconds );

			DueDateDialogue dateDialogue = new DueDateDialogue();
			dateDialogue.Arguments = dialogueArgs;

			return dateDialogue;
		}


		/// <summary>
		/// Called when a fragment is first attached to its activity.
		/// </summary>
		/// <param name="activity">To be added.</param>
		public override void OnAttach( Activity activity )
		{
			base.OnAttach( activity );

			// Save the NotificationTimeDialogueListener interface
			listener = ( DueDateDialogueListener )activity;
		}

		/// <summary>
		/// Called to have the fragment instantiate its user interface view.
		/// </summary>
		/// <param name="inflater">The LayoutInflater object that can be used to inflate
		///  any views in the fragment,</param>
		/// <param name="container">If non-null, this is the parent view that the fragment's
		///  UI should be attached to. The fragment should not add the view itself,
		///  but this can be used to generate the LayoutParams of the view.</param>
		/// <param name="savedInstanceState">If non-null, this fragment is being re-constructed
		///  from a previous saved state as given here.</param>
		/// <returns>The view.</returns>
		public override View OnCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
		{
			base.OnCreateView( inflater, container, savedInstanceState );

			View layoutView = inflater.Inflate( Resource.Layout.DueDateSelection, container );

			// Access the CalendarView
			calendarControl = layoutView.FindViewById< CalendarView >( Resource.Id.calendarView );

			// Either get the date from the arguments originally used to start the fragment of the saved date
			calendarControl.Date = ( savedInstanceState == null ) ? Arguments.GetLong( DueDateLabel ) : savedInstanceState.GetLong( DueDateLabel );

			// Set the title
			Dialog.SetTitle( "Due date" );

			// Setup the click handlers for the 3 buttons
			layoutView.FindViewById< Button >( Resource.Id.dueDateCancelButton ).Click += ( buttonSender, buttonEvents ) => 
			{
				Dismiss();
			};
			layoutView.FindViewById< Button >( Resource.Id.dueDateSetButton ).Click += ( buttonSender, buttonEvents ) =>
			{
				// Pass back the notification. Use a GregorianCalendar in order to keep track of summer time
				GregorianCalendar setDate = new GregorianCalendar();
				setDate.TimeInMillis = calendarControl.Date;
				listener.OnDueDateSet( new DateTime( setDate.Get( CalendarField.Year ), setDate.Get( CalendarField.Month ) + 1, setDate.Get( CalendarField.DayOfMonth ) ) );
				Dismiss();
			};
			layoutView.FindViewById< Button >( Resource.Id.dueDateClearButton ).Click += ( buttonSender, buttonEvents ) =>
			{
				// Pass back the notification
				listener.OnDueDateCleared();
				Dismiss();
			};

			return layoutView;
		}

		/// <summary>
		/// Called to ask the fragment to save its current dynamic state, so it can later be reconstructed in a new instance of its process is
		/// restarted.
		/// The date held by the calendar view has to be saved because it is not (for some reason) saved automatically
		/// </summary>
		/// <param name="outState">Bundle in which to place your saved state.</param>
		public override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);

			outState.PutLong( DueDateLabel, calendarControl.Date );
		}

		//
		// Private data
		//

		/// <summary>
		/// The DueDateDialogueListener instance
		/// </summary>
		private DueDateDialogueListener listener = null;

		/// <summary>
		/// Keys for storing arguments
		/// </summary>
		private const string DueDateLabel = "DueDate";

		/// <summary>
		/// The calendar control.
		/// </summary>
		private CalendarView calendarControl = null;

		/// <summary>
		/// Offset for interfacing to the Calander control
		/// </summary>
		private static DateTime Seventies = new DateTime( 1970, 1, 1 );
	}
}

