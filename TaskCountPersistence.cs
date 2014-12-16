// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        Persistence
// Filename:    TaskCountPersistence.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The TaskCountPersistence class controls the persistence of the task count for the widgets.
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
using System;
using Android.Content;

namespace AutoNag
{
	/// <summary>
	/// The TaskCountPersistence class controls the persistence of the task count for the widgets.
	/// </summary>
	public class TaskCountPersistence
	{
		//
		// Public methods
		//

		/// <summary>
		/// Gets the task count from the persistent store.
		/// </summary>
		/// <returns>The sort order.</returns>
		/// <param name="persistenceContext">Persistence context.</param>
		/// <param name="widgetId">Widget identifier.</param>
		public static int GetTaskCount( Context persistenceContext, int widgetId )
		{
			return persistenceContext.GetSharedPreferences( PreferenceFileName, FileCreationMode.Private )
				.GetInt( string.Format( NumberOfTasksFormat, widgetId ), 0 );
		}

		/// <summary>
		/// Persist the task count
		/// </summary>
		/// <param name="persistenceContext">Persistence context.</param>
		/// <param name="widgetId">Widget identifier.</param>
		/// <param name="taskCount">Task count</param>
		public static void SetTaskCount( Context persistenceContext, int widgetId, int taskCount )
		{
			persistenceContext.GetSharedPreferences( PreferenceFileName, FileCreationMode.Private )
				.Edit()
				.PutInt( string.Format( NumberOfTasksFormat, widgetId ), taskCount )
				.Commit();
		}

		//
		// Private methods
		//

		/// <summary>
		/// Private constructor
		/// </summary>
		private TaskCountPersistence()
		{
		}

		//
		// Private data
		//

		/// <summary>
		/// The name of the preference file.
		/// </summary>
		private const string PreferenceFileName = "TaskCountPersistence";

		/// <summary>
		/// Format for the task count
		/// </summary>
		private const string NumberOfTasksFormat = "{0}TaskCount";
	}
}

