// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        TaskManagement
// Filename:    TaskDatabaseSQLite.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The TaskDatabaseSQLite class uses an SQLite database to perform the required create, read, update and delete operations.
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
using System.Collections.Generic;
using System.Text;

using Mono.Data.Sqlite;
using System.IO;
using System.Data;

using Android.Util;

namespace AutoNag
{
	/// <summary>
	/// The TaskDatabaseSQLite class uses an SQLite database to perform the required create, read, update and delete operations.
	/// </summary>
	public class TaskDatabaseSQLite 
	{
		//
		// Public methods
		//

		/// <summary>
		/// Initializes a new instance of the TaskDatabaseSQLite class"/> TaskDatabase. 
		/// If the database doesn't exist, it will create the database and all the tables.
		/// </summary>
		/// <param name="dbPath">Db path.</param>
		public TaskDatabaseSQLite( string dbPath ) 
		{
			// Save the full connection string
			connectionString = "Data Source=" + dbPath;

			lock( locker ) 
			{
				// Create the tables if the database does not exist
				if ( File.Exists( dbPath ) == false ) 
				{
					SqliteConnection connection = new SqliteConnection( connectionString );
					connection.Open ();

					SqliteCommand command = connection.CreateCommand();
					command.CommandText = 
						"CREATE TABLE [Items] ( Identity INTEGER PRIMARY KEY ASC, Name TEXT, Notes TEXT, Done INTEGER, NotificationRequired INTEGER, Priority INTEGER, DueDate TEXT, ModifiedDate TEXT );";
					command.ExecuteNonQuery();

					// There is a bug in Connection.Close such that it generates an error message if a transaction is not active.
					// So just begin a transaction here
					connection.BeginTransaction();
					connection.Close ();
				}
			}
		}

		/// <summary>
		/// Gets all of the the tasks.
		/// </summary>
		/// <returns>The tasks stored in a IEnumerable<Task></returns>
		/// <param name="sortOrder">Sort order to be applied to the tasks.</param>
		public IEnumerable<Task> GetItems( List< Task.SortOrders > sortOrder )
		{
			List< Task > taskList = new List< Task >();

			lock( locker ) 
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				SqliteCommand command = connection.CreateCommand();
				command.CommandText = "SELECT [Identity], [Name], [Notes], [Done], [NotificationRequired], [Priority], [DueDate], [ModifiedDate] from [Items]";

				// Get the order clause and add it to the query if it is defined
				string orderClause = GetSortOrderClause( sortOrder );
				if ( orderClause.Length > 0 )
				{
					command.CommandText = command.CommandText + " ORDER BY " + orderClause;
				}
					
				SqliteDataReader reader = command.ExecuteReader();

				// Extract the tasks from the reader and add to teh lsit
				while ( reader.Read() == true )
				{
					taskList.Add( FromReader( reader ) );
				}
					
				// There is a bug in Connection.Close such that it generates an error message if a transaction is not active.
				// So just begin a transaction here
				connection.BeginTransaction();
				connection.Close ();
			}

			return taskList;
		}

		/// <summary>
		/// Gets the task associated with the specified Id
		/// </summary>
		/// <returns>The task.</returns>
		/// <param name="id">Identifier.</param>
		public Task GetItem( int id ) 
		{
			Task item = new Task();

			lock( locker )
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				SqliteCommand command = connection.CreateCommand();
				command.CommandText = "SELECT [Identity], [Name], [Notes], [Done], [NotificationRequired], [Priority], [DueDate], [ModifiedDate] from [Items] WHERE [Identity] = ?";
				command.Parameters.Add( new SqliteParameter( DbType.Int32 ){ Value = id } );

				SqliteDataReader reader = command.ExecuteReader();
				if ( reader.Read() == true )
				{
					item = FromReader( reader );
				}

				// There is a bug in Connection.Close such that it generates an error message if a transaction is not active.
				// So just begin a transaction here
				connection.BeginTransaction();
				connection.Close ();
			}

			return item;
		}

		/// <summary>
		/// Saves the task.
		/// </summary>
		/// <returns>The number of items saved</returns>
		/// <param name="item">Item.</param>
		public int SaveItem (Task item) 
		{
			int retCode = 0;

			lock( locker )
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				SqliteCommand command = connection.CreateCommand();
				command.Parameters.Add( new SqliteParameter( DbType.String ) { Value = item.Name } );
				command.Parameters.Add( new SqliteParameter( DbType.String ) { Value = item.Notes } );
				command.Parameters.Add( new SqliteParameter( DbType.Int32 ) { Value = item.Done } );
				command.Parameters.Add( new SqliteParameter( DbType.Int32 ) { Value = item.NotificationRequired } );
				command.Parameters.Add( new SqliteParameter( DbType.Int32 ) { Value = item.Priority } );

				// If the DueDate is DateTime.Min then store it as DateTime.Max to preserve date sort order
				DateTime dueDateToStore = item.DueDate;
				if ( item.DueDate == DateTime.MinValue )
				{
					dueDateToStore = DateTime.MaxValue;
				}
				command.Parameters.Add( new SqliteParameter( DbType.String ) { Value = dueDateToStore.ToString( DueDateFormat ) } );

				command.Parameters.Add( new SqliteParameter( DbType.String ) { Value = item.ModifiedDate.ToString( ModifiedDateFormat ) } );

				// Either update an existing row or add a new one
				if ( item.ID != 0 )
				{
					command.CommandText = "UPDATE [Items] SET [Name] = ?, [Notes] = ?, [Done] = ?, [NotificationRequired] = ?, [Priority] = ?, [DueDate] = ?, [ModifiedDate] = ? WHERE [Identity] = ?;";
					command.Parameters.Add (new SqliteParameter (DbType.Int32) { Value = item.ID });
				}
				else
				{
					command.CommandText = "INSERT INTO [Items] ([Name], [Notes], [Done], [NotificationRequired], [Priority], [DueDate], [ModifiedDate] ) VALUES (?, ?, ?, ?, ?, ?, ?)";
				}
				
				retCode = command.ExecuteNonQuery();

				// There is a bug in Connection.Close such that it generates an error message if a transaction is not active.
				// So just begin a transaction here
				connection.BeginTransaction();
				connection.Close ();
			}

			return retCode;
		}

		/// <summary>
		/// Deletes the task.
		/// </summary>
		/// <returns>The number of items deleted</returns>
		/// <param name="id">Identifier.</param>
		public int DeleteItem( int id ) 
		{
			int retCode = 0;

			lock( locker ) 
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				SqliteCommand command = connection.CreateCommand();
				command.CommandText = "DELETE FROM [Items] WHERE [Identity] = ?;";
				command.Parameters.Add( new SqliteParameter(DbType.Int32) { Value = id } );

				retCode = command.ExecuteNonQuery();

				// There is a bug in Connection.Close such that it generates an error message if a transaction is not active.
				// So just begin a transaction here
				connection.BeginTransaction();
				connection.Close ();
			}

			return retCode;
		}

		//
		// Private methods
		//

		/// <summary>
		/// Extract a Task object from a SqliteDataReader
		/// </summary>
		/// <returns>The reader.</returns>
		/// <param name="reader">Reader.</param>
		private Task FromReader( SqliteDataReader reader )
		{
			Task toTask = new Task();

			toTask.ID =  Convert.ToInt32( reader[ "Identity" ] );
			toTask.Name = reader ["Name"].ToString();
			toTask.Notes = reader ["Notes"].ToString();
			toTask.Done = Convert.ToBoolean( reader ["Done"] );
			toTask.NotificationRequired = Convert.ToBoolean( reader ["NotificationRequired"] );
			toTask.Priority	= Convert.ToInt32( reader[ "Priority" ] );

			// Allow the DueTime to be in either yyyyMMddHHmm or for compatibility yyyyMMdd
			DateTime dueDateReadIn;

			if ( ( DateTime.TryParseExact( reader[ "DueDate" ].ToString(), DueDateFormat, System.Globalization.CultureInfo.InvariantCulture, 
				System.Globalization.DateTimeStyles.None, out dueDateReadIn ) == false ) &&
				( DateTime.TryParseExact( reader[ "DueDate" ].ToString(), CompatibleDueDateFormat, System.Globalization.CultureInfo.InvariantCulture, 
					System.Globalization.DateTimeStyles.None, out dueDateReadIn ) == false ) )
			{
				dueDateReadIn = DateTime.MinValue;
			}
			else
			{
				// In the database an undefined DueDate is set to the MaxValue, but in the rest of the system this is defined as MinValue so change it here
				if ( dueDateReadIn.Date == DateTime.MaxValue.Date )
				{
					dueDateReadIn = DateTime.MinValue;
				}
			}

			toTask.DueDate = dueDateReadIn;

			// The format of the ModifiedDate is fixed
			DateTime modifiedDateReadIn;

			if ( DateTime.TryParseExact( reader[ "ModifiedDate" ].ToString(), ModifiedDateFormat, System.Globalization.CultureInfo.InvariantCulture, 
				System.Globalization.DateTimeStyles.None, out modifiedDateReadIn ) == false )
			{
				modifiedDateReadIn = DateTime.MinValue;
			}

			toTask.ModifiedDate = modifiedDateReadIn;

			return toTask;
		}

		/// <summary>
		/// Generate a sort order clause for incluson in the SELECT commmand from list of SortOrder items
		/// </summary>
		/// <returns>The sort order clause.</returns>
		/// <param name="sortOrder">Sort order.</param>
		private string GetSortOrderClause( List< Task.SortOrders > sortOrder )
		{
			string orderClause = "";

			if ( sortOrder != null )
			{
				StringBuilder sortOrderBuilder = new StringBuilder();

				foreach ( Task.SortOrders currentSortOrder in sortOrder )
				{
					if ( sortOrderBuilder.Length > 0 )
					{
						sortOrderBuilder.Append( ", " );
					}

					if ( currentSortOrder == Task.SortOrders.Done )
					{
						sortOrderBuilder.Append( "[Done] ASC" );
					}
					else if ( currentSortOrder == Task.SortOrders.Priority )
					{
						sortOrderBuilder.Append( "[Priority] DESC" );
					}
					else
					{
						sortOrderBuilder.Append( "[DueDate] ASC" );
					}
				}

				orderClause = sortOrderBuilder.ToString();
			}

			return orderClause;
		}

		//
		// Private data
		//

		/// <summary>
		/// Static lock object to protect database access
		/// </summary>
		private static object locker = new object ();

		/// <summary>
		/// The connection string.
		/// </summary>
		private string connectionString = "";

		/// <summary>
		/// Format strings for dates held in the database
		/// </summary>
		private const string DueDateFormat = "yyyyMMddHHmm";
		private const string CompatibleDueDateFormat = "yyyyMMdd";
		private const string ModifiedDateFormat = "yyyyMMddHHmmss";
	}
}