// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        TaskManagement
// Filename:    TaskRepository.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The TaskRepository class is an wrapper around the TaskDatabaseSQLite class that actually performs all of the database operations.
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

using System.Collections.Generic;
using System.IO;

namespace AutoNag 
{
	/// <summary>
	/// The TaskRepository class is an wrapper around the TaskDatabaseSQLite class that actually performs all of the database operations.
	/// </summary>
	public class TaskRepository 
	{
		//
		// Public methods
		//

		/// <summary>
		/// Gets the task associated with the specified Id
		/// </summary>
		/// <returns>The task.</returns>
		/// <param name="id">Identifier.</param>
		public static Task GetTask( int id )
		{
			return InstanceProperty.sqliteDatabase.GetItem( id );
		}

		/// <summary>
		/// Gets the last task inserted in the table
		/// </summary>
		/// <returns>The task.</returns>
		/// <param name="id">Identifier.</param>
		public static Task GetLastTask() 
		{
			return InstanceProperty.sqliteDatabase.GetLastItem();
		}

		/// <summary>
		/// Gets all of the the tasks.
		/// </summary>
		/// <returns>The tasks stored in a IEnumerable<Task></returns>
		/// <param name="sortOrder">Sort order to be applied to the tasks.</param>
		public static IList< Task > GetTasks( IList< Task.SortOrders > sortOrder )
		{
			return InstanceProperty.sqliteDatabase.GetItems( sortOrder );
		}

		/// <summary>
		/// Saves the task.
		/// </summary>
		/// <returns>Whether or not the item was saved</returns>
		/// <param name="item">Item.</param>
		public static bool SaveTask( Task item )
		{
			return ( InstanceProperty.sqliteDatabase.SaveItem( item ) == SQLiteSingleItemUpdated );
		}

		/// <summary>
		/// Deletes the task.
		/// </summary>
		/// <returns><c>true</c>, if task was deleted, <c>false</c> otherwise.</returns>
		/// <param name="id">Identifier.</param>
		public static bool DeleteTask( int id )
		{
			return ( InstanceProperty.sqliteDatabase.DeleteItem( id ) == SQLiteSingleItemUpdated );
		}

		//
		// Private methods
		// 

		/// <summary>
		/// Private constructor
		/// </summary>
		private TaskRepository()
		{
			// Instantiate the database	interface
			sqliteDatabase = new TaskDatabaseSQLite( Path.Combine( Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "TaskDatabase.db3" ) );
		}

		/// <summary>
		/// Gets the single instance of this class, creating it if necessary.
		/// </summary>
		/// <value>The instance property.</value>
		private static TaskRepository InstanceProperty
		{
			get
			{
				if ( instance == null )
				{
					instance = new TaskRepository(); 
				}

				return instance;
			}
		}

		//
		// Private data
		//

		/// <summary>
		/// The single instance of the TaskRepository
		/// </summary>
		private static TaskRepository instance = null;

		/// <summary>
		/// The TaskDatabase object used to access the database
		/// </summary>
		private TaskDatabaseSQLite sqliteDatabase = null;

		/// <summary>
		/// Success code for SQLite commands that only affect a single row
		/// </summary>
		private const int SQLiteSingleItemUpdated = 1;
	}	
}

