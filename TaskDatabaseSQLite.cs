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
		/// </summary>
		/// <param name="dbPath">Db path.</param>
		public TaskDatabaseSQLite( string dbPath ) 
		{
			// Save the full connection string
			connectionString = "Data Source=" + dbPath;
		}

		/// <summary>
		/// Create a table to contain a collection of tasks
		/// </summary>
		/// <returns>Success code.</returns>
		/// <param name="listName">List name.</param>
		public int CreateList( string listName )
		{
			return ExecuteSimpleNonQuery( string.Format( "CREATE TABLE [{0}] ( Identity INTEGER PRIMARY KEY ASC, Name TEXT, Notes TEXT, Done INTEGER," +
				" NotificationRequired INTEGER, Priority INTEGER, DueDate TEXT, ModifiedDate TEXT )", listName ) );
		}

		/// <summary>
		/// Renames the list.
		/// </summary>
		/// <returns>Success code.</returns>
		/// <param name="oldName">Old name.</param>
		/// <param name="newName">New name.</param>
		public int RenameList( string oldName, string newName )
		{
			return ExecuteSimpleNonQuery( string.Format( "ALTER TABLE [{0}] RENAME TO [{1}]", oldName, newName ) );
		}

		/// <summary>
		/// Deletes the list.
		/// </summary>
		/// <returns>Success code.</returns>
		/// <param name="listName">List name.</param>
		public int DeleteList( string listName )
		{
			return ExecuteSimpleNonQuery( string.Format( "DROP TABLE [{0}]", listName ) );
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

				// Extract the table names from the reader and add to the list
				while ( reader.Read() == true )
				{
					string tableName = reader.GetString( 0 );
					if ( tableName != ListColourTableName )
					{
						tableList.Add( tableName );
					}
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
		public IList< Task > GetItems( IList< string > taskListNames, IList< Task.SortOrders > sortOrder )
		{
			List< Task > taskList = new List< Task >();

			lock( locker ) 
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				// Need to join together the results of the selects and then order it
				StringBuilder queryString = new StringBuilder();
				foreach ( string taskListName in taskListNames )
				{
					queryString.AppendFormat( ( queryString.Length == 0 ) ? 
						"SELECT *, '{0}' AS 'table_name' FROM [{0}]" : 
						" UNION ALL SELECT *, '{0}' AS 'table_name' FROM [{0}]", taskListName );
				}

				// Finally add the sort clause
				queryString.Append( GetSortOrderClause( sortOrder ) );

				// Execute the query
				SqliteDataReader reader = new SqliteCommand( queryString.ToString(), connection ).ExecuteReader();

				// Extract the tasks from the reader and add to the list
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

				SqliteDataReader reader = new SqliteCommand( string.Format( "SELECT *, '{0}' AS 'table_name' FROM [{0}] WHERE [Identity] = {1}", taskListName, id ), 
					connection ).ExecuteReader();

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
		public int SaveItem( Task item ) 
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
				DateTime dueDateToStore = ( item.DueDate == DateTime.MinValue ) ? DateTime.MaxValue : item.DueDate;
				command.Parameters.AddWithValue( null, dueDateToStore.ToString( DueDateFormat ) );

				command.Parameters.AddWithValue( null, item.ModifiedDate.ToString( ModifiedDateFormat ) );

				// Either update an existing row or add a new one
				if ( item.ID != 0 )
				{
					command.CommandText = string.Format( 
						"UPDATE [{0}] SET [Name] = ?, [Notes] = ?, [Done] = ?, [NotificationRequired] = ?, [Priority] = ?, [DueDate] = ?, [ModifiedDate] = ? WHERE [Identity] = ?", 
						item.ListName );
					command.Parameters.AddWithValue( null, item.ID );
				}
				else
				{
					command.CommandText = string.Format(
						"INSERT INTO [{0}] ([Name], [Notes], [Done], [NotificationRequired], [Priority], [DueDate], [ModifiedDate] ) VALUES (?, ?, ?, ?, ?, ?, ?)",
						item.ListName );
				}
				
				retCode = command.ExecuteNonQuery();

				// If this is a new item then retrieve the new index
				if ( item.ID == 0 )
				{
					command.CommandText = string.Format( "SELECT [Identity] FROM [{0}] WHERE [Identity] IN ( SELECT MAX([Identity]) FROM [{0}] )", item.ListName );

					SqliteDataReader reader = command.ExecuteReader();
					if ( reader.Read() == true )
					{
						item.ID =  reader.GetInt32( 0 );
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
			return ExecuteSimpleNonQuery( string.Format( "DELETE FROM [{0}] WHERE [Identity] = {1}", taskListName, id ) );
		}

		/// <summary>
		/// Gets all of the the list colour entries
		/// </summary>
		/// <returns>The tasks stored in a IEnumerable<Task></returns>
		/// <param name="sortOrder">Sort order to be applied to the tasks.</param>
		public IDictionary< string, ListColourEnum > GetListColours()
		{
			Dictionary< string, ListColourEnum >colourList = new Dictionary< string, ListColourEnum >();

			// Create the ListColourTableName if it does not exist
			ExecuteSimpleNonQuery( string.Format( "CREATE TABLE IF NOT EXISTS [{0}] ( ListName TEXT PRIMARY KEY ASC, Colour TEXT )", ListColourTableName ) );

			lock( locker ) 
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				// Get the contents of the ListColourTableName
				SqliteDataReader reader = new SqliteCommand( string.Format( "SELECT * FROM [{0}]", ListColourTableName ), connection ).ExecuteReader();

				// Extract the list names and colours from the reader and add to the dictionary
				while ( reader.Read() == true )
				{
					colourList[ reader.GetString( 0 ) ] = ListColourHelper.StringToColourEnum( reader.GetString( 1 ) );
				}

				// There is a bug in Connection.Close such that it generates an error message if a transaction is not active.
				// So just begin a transaction here
				connection.BeginTransaction();
				connection.Close ();
			}

			return colourList;
		}

		/// <summary>
		/// Adds the list colour entry to the ListColourTableName
		/// </summary>
		/// <param name="listName">List name.</param>
		/// <param name="colour">Colour.</param>
		public void AddListColour( string listName, ListColourEnum colour )
		{
			ExecuteSimpleNonQuery( string.Format( "INSERT INTO [{0}] ( [ListName], [Colour] ) VALUES ( '{1}', '{2}' )", ListColourTableName, listName, colour ) );
		}

		/// <summary>
		/// Updates an existing the list colour entry
		/// </summary>
		/// <param name="listName">List name.</param>
		/// <param name="colour">Colour.</param>
		public void UpdateListColour( string listName, ListColourEnum colour )
		{
			ExecuteSimpleNonQuery( string.Format( "UPDATE [{0}] SET [Colour] = '{1}' WHERE [ListName] = '{2}'", ListColourTableName, colour, listName ) );
		}

		//
		// Private methods
		//

		/// <summary>
		/// Executes the simple non query.
		/// </summary>
		/// <returns>The simple non query.</returns>
		/// <param name="commandText">Command text.</param>
		private int ExecuteSimpleNonQuery( string commandText )
		{
			int retCode = 0;

			lock( locker ) 
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				retCode = new SqliteCommand( commandText, connection ).ExecuteNonQuery();

				// There is a bug in Connection.Close such that it generates an error message if a transaction is not active.
				// So just begin a transaction here
				connection.BeginTransaction();
				connection.Close();
			}

			return retCode;
		}

		/// <summary>
		/// Extract a Task object from a SqliteDataReader
		/// </summary>
		/// <returns>The reader.</returns>
		/// <param name="reader">Reader.</param>
		private Task FromReader( SqliteDataReader reader )
		{
			Task toTask = new Task();

			toTask.ID = reader.GetInt32( 0 );
			toTask.Name = reader.GetString( 1 );
			toTask.Notes = reader.GetString( 2 );
			toTask.Done = reader.GetBoolean( 3 );
			toTask.NotificationRequired = reader.GetBoolean( 4 );
			toTask.Priority	= reader.GetInt32( 5 );

			// Allow the DueTime to be in either yyyyMMddHHmm or for compatibility yyyyMMdd
			DateTime dueDateReadIn;
			string dateTimeString = reader.GetString( 6 );

			if ( ( DateTime.TryParseExact( dateTimeString, DueDateFormat, System.Globalization.CultureInfo.InvariantCulture, 
					System.Globalization.DateTimeStyles.None, out dueDateReadIn ) == false ) &&
				( DateTime.TryParseExact( dateTimeString, CompatibleDueDateFormat, System.Globalization.CultureInfo.InvariantCulture, 
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

			if ( DateTime.TryParseExact( reader.GetString( 7 ), ModifiedDateFormat, System.Globalization.CultureInfo.InvariantCulture, 
				System.Globalization.DateTimeStyles.None, out modifiedDateReadIn ) == false )
			{
				modifiedDateReadIn = DateTime.MinValue;
			}

			toTask.ModifiedDate = modifiedDateReadIn;

			toTask.ListName = reader.GetString( 8 );

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

				if ( sortOrderBuilder.Length > 0 )
				{
					orderClause = string.Format( " ORDER BY {0}", sortOrderBuilder.ToString() );
				}
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

		/// <summary>
		/// The name of the list colour table.
		/// </summary>
		private const string ListColourTableName = "ListColour.Table";
	}
}