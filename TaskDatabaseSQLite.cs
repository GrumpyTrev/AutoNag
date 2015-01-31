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

			// Create the tables if the database does not exist
			if ( File.Exists( dbPath ) == false ) 
			{
				CreateList( "Items" );
			}
		}

		/// <summary>
		/// Create a table to contain a collection of tasks
		/// </summary>
		/// <returns>Success code.</returns>
		/// <param name="listName">List name.</param>
		public int CreateList( string listName )
		{
			int retCode = 0;

			lock( locker ) 
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				retCode = new SqliteCommand( string.Format( 
					"CREATE TABLE [{0}] ( Identity INTEGER PRIMARY KEY ASC, Name TEXT, Notes TEXT, Done INTEGER, NotificationRequired INTEGER, Priority INTEGER, DueDate TEXT, ModifiedDate TEXT )",
					listName ), connection ).ExecuteNonQuery();

				// There is a bug in Connection.Close such that it generates an error message if a transaction is not active.
				// So just begin a transaction here
				connection.BeginTransaction();
				connection.Close();
			}

			return retCode;
		}

		/// <summary>
		/// Renames the list.
		/// </summary>
		/// <returns>Success code.</returns>
		/// <param name="oldName">Old name.</param>
		/// <param name="newName">New name.</param>
		public int RenameList( string oldName, string newName )
		{
			int retCode = 0;

			lock( locker ) 
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				retCode = new SqliteCommand( string.Format( "ALTER TABLE [{0}] RENAME TO [{1}]", oldName, newName ), connection ).ExecuteNonQuery();

				// There is a bug in Connection.Close such that it generates an error message if a transaction is not active.
				// So just begin a transaction here
				connection.BeginTransaction();
				connection.Close();
			}

			return retCode;
		}

		/// <summary>
		/// Gets the names of the tables in the databases.
		/// </summary>
		/// <returns>The task tables.</returns>
		public IList< string > GetTaskTables()
		{
			List< string > tableList = new List< string >();

			lock( locker ) 
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				SqliteDataReader reader = new SqliteCommand( "SELECT [Name] FROM [sqlite_master] WHERE type = 'table'", connection ).ExecuteReader();

				// Extract the tasks from the reader and add to the list
				while ( reader.Read() == true )
				{
					tableList.Add( ( string )reader[ "Name" ] );
				}

				// There is a bug in Connection.Close such that it generates an error message if a transaction is not active.
				// So just begin a transaction here
				connection.BeginTransaction();
				connection.Close ();
			}

			return tableList;
		}

		/// <summary>
		/// Gets all of the the tasks.
		/// </summary>
		/// <returns>The tasks stored in a IEnumerable<Task></returns>
		/// <param name="sortOrder">Sort order to be applied to the tasks.</param>
		public IList< Task > GetItems( string taskListName, IList< Task.SortOrders > sortOrder )
		{
			List< Task > taskList = new List< Task >();

			lock( locker ) 
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				SqliteCommand command = connection.CreateCommand();

				// Get the order clause and add it to the query if it is defined
				if ( sortOrder != null )
				{
					command.CommandText = string.Format( "SELECT * FROM [{0}] ORDER BY {1}", taskListName, GetSortOrderClause( sortOrder ) );
				}
				else
				{
					command.CommandText = string.Format( "SELECT * FROM [{0}]", taskListName );
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
		public Task GetItem( string taskListName, int id ) 
		{
			Task item = new Task();

			lock( locker )
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				SqliteCommand command = new SqliteCommand( string.Format( "SELECT * FROM [{0}] WHERE [Identity] = ?", taskListName ), connection );
				command.Parameters.Add( new SqliteParameter( DbType.Int32, ( object )id ) );

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
		public int SaveItem( string taskListName, Task item ) 
		{
			int retCode = 0;

			lock( locker )
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				SqliteCommand command = connection.CreateCommand();
				command.Parameters.AddWithValue( null, item.Name );
				command.Parameters.AddWithValue( null, item.Notes );
				command.Parameters.AddWithValue( null, item.Done );
				command.Parameters.AddWithValue( null, item.NotificationRequired );
				command.Parameters.AddWithValue( null, item.Priority );

				// If the DueDate is DateTime.Min then store it as DateTime.Max to preserve date sort order
				DateTime dueDateToStore = item.DueDate;
				if ( item.DueDate == DateTime.MinValue )
				{
					dueDateToStore = DateTime.MaxValue;
				}
				command.Parameters.AddWithValue( null, dueDateToStore.ToString( DueDateFormat ) );

				command.Parameters.AddWithValue( null, item.ModifiedDate.ToString( ModifiedDateFormat ) );

				// Either update an existing row or add a new one
				if ( item.ID != 0 )
				{
					command.CommandText = string.Format( 
						"UPDATE [{0}] SET [Name] = ?, [Notes] = ?, [Done] = ?, [NotificationRequired] = ?, [Priority] = ?, [DueDate] = ?, [ModifiedDate] = ? WHERE [Identity] = ?", 
						taskListName );
					command.Parameters.AddWithValue( null, item.ID );
				}
				else
				{
					command.CommandText = string.Format(
						"INSERT INTO [{0}] ([Name], [Notes], [Done], [NotificationRequired], [Priority], [DueDate], [ModifiedDate] ) VALUES (?, ?, ?, ?, ?, ?, ?)",
						taskListName );
				}
				
				retCode = command.ExecuteNonQuery();

				// If this is a new item then retrieve the new index
				if ( item.ID == 0 )
				{
					command.CommandText = string.Format(
						"SELECT [Identity] FROM [{0}] WHERE [Identity] IN ( SELECT MAX([Identity]) FROM [Items] )", taskListName );

					SqliteDataReader reader = command.ExecuteReader();
					if ( reader.Read() == true )
					{
						item.ID =  Convert.ToInt32( reader[ "Identity" ] );
					}
				}

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
		public int DeleteItem( string taskListName, int id ) 
		{
			int retCode = 0;

			lock( locker ) 
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				SqliteCommand command = new SqliteCommand( string.Format( "DELETE FROM [{0}] WHERE [Identity] = ?", taskListName ), connection );
				command.Parameters.AddWithValue( null, id );

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
		private string GetSortOrderClause( IList< Task.SortOrders > sortOrder )
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