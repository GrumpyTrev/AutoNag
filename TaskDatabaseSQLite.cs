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
			string renameFormat = "ALTER TABLE [{0}] RENAME TO [{1}]";
			string tempTableName = "temp_table";

			// SQLite is case insensitive as far as table names are concerned. The case of a table name is retained when the table is created but thereafter 
			// the case is ignored. When renaming a table it is best to give it a temporary name first and then the final name.
			ExecuteSimpleNonQuery( string.Format( renameFormat, oldName, tempTableName ) );

			return ExecuteSimpleNonQuery( string.Format( renameFormat, tempTableName, newName ) );
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

			// Get all the table names
			ExecuteReaderCommand( "SELECT [Name] FROM [sqlite_master] WHERE type = 'table'", delegate( SqliteDataReader reader )
			{
				// Extract the table names from the reader and add to the list - ignore 'system' table names
				while ( reader.Read() == true )
				{
					string tableName = reader.GetString( 0 );
					if ( ( tableName != ListColourTableName ) && ( tableName != GeneralItemsTableName ) )
					{
						tableList.Add( tableName );
					}
				}
			} );

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

			// Get the tasks from the DB
			ExecuteReaderCommand( queryString.ToString(), delegate( SqliteDataReader reader )
			{
				// Extract the tasks from the reader and add to the list
				while ( reader.Read() == true )
				{
					taskList.Add( FromReader( reader ) );
				}
			} );

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

			// Get the required task and extract it from the reader
			ExecuteReaderCommand( string.Format( "SELECT *, '{0}' AS 'table_name' FROM [{0}] WHERE [Identity] = {1}", taskListName, id ), 
				delegate( SqliteDataReader reader )
			{
				if ( reader.Read() == true )
				{
					item = FromReader( reader );
				}
			} );

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
				SqliteParameterCollection parameters = command.Parameters;

				parameters.AddWithValue( null, item.Name );
				parameters.AddWithValue( null, item.Notes );
				parameters.AddWithValue( null, item.Done );
				parameters.AddWithValue( null, item.NotificationRequired );
				parameters.AddWithValue( null, item.Priority );

				// If the DueDate is DateTime.Min then store it as DateTime.Max to preserve date sort order
				parameters.AddWithValue( null, ( ( item.DueDate == DateTime.MinValue ) ? DateTime.MaxValue : item.DueDate ).ToString( DueDateFormat ) );

				parameters.AddWithValue( null, item.ModifiedDate.ToString( ModifiedDateFormat ) );

				// Either update an existing row or add a new one
				if ( item.ID != 0 )
				{
					command.CommandText = string.Format( 
						"UPDATE [{0}] SET [Name] = ?, [Notes] = ?, [Done] = ?, [NotificationRequired] = ?, [Priority] = ?, [DueDate] = ?, [ModifiedDate] = ? WHERE [Identity] = ?", 
						item.ListName );
					parameters.AddWithValue( null, item.ID );
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

			// Get the contents of the ListColourTableName
			ExecuteReaderCommand( string.Format( "SELECT * FROM [{0}]", ListColourTableName ), delegate( SqliteDataReader reader )
			{
				// Extract the list names and colours from the reader and add to the dictionary
				while ( reader.Read() == true )
				{
					colourList[ reader.GetString( 0 ) ] = ListColourHelper.StringToColourEnum( reader.GetString( 1 ) );
				}
			} );

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

		/// <summary>
		/// Removes an existing the list colour entry
		/// </summary>
		/// <param name="listName">List name.</param>
		public void RemoveListColour( string listName )
		{
			ExecuteSimpleNonQuery( string.Format( "DELETE FROM [{0}] WHERE [ListName] = '{1}'", ListColourTableName, listName ) );
		}

		/// <summary>
		/// Extract the notification tone name and overdue options from the database
		/// </summary>
		/// <returns>The notification tone.</returns>
		public string GetOptions( ref bool highlightOverdueTasks )
		{
			string notificationTone = "";
			bool localhighlightOverdueTasks = false;

			// Create the GeneralItemsTableName if it does not exist
			CreateGeneralTable();

			// Get the contents of the GeneralItemsTableName
			ExecuteReaderCommand( string.Format( "SELECT * FROM [{0}]", GeneralItemsTableName ), delegate( SqliteDataReader reader )
			{
				// Extract the notification tone name from column 0 and the overdue option from column 1
				if ( reader.Read() == true )
				{
					notificationTone = reader.GetString( 0 );

					// Getting a boolean works here even though it is stored as an int in the db.
					localhighlightOverdueTasks = reader.GetBoolean( 1 );
				}
			} );

			highlightOverdueTasks = localhighlightOverdueTasks;
			return notificationTone;
		}

		/// <summary>
		/// Updates the notification tone and overdue options
		/// </summary>
		/// <param name="toneName">Tone name.</param>
		public void SetOptions( string toneName, bool highlightOverdueTasks )
		{
			// Always drop the table and recreate it
			DeleteList( GeneralItemsTableName ); 
			CreateGeneralTable();

			// The highlightOverdueTasks boolean must be explicitly stored as 1/0. When reading back an implicit conversion to a bool is performed
			// by the reader call
			ExecuteSimpleNonQuery( string.Format( "INSERT INTO [{0}] ( [ToneName], [Overdue] ) VALUES ( '{1}', '{2}' )", GeneralItemsTableName, toneName,
				highlightOverdueTasks ? 1 : 0 ) );
		} 

		//
		// Private methods
		//

		/// <summary>
		/// Delegate definition for the Reader extraction method
		/// </summary>
		private delegate void ReaderExtraction( SqliteDataReader reader );

		/// <summary>
		/// Executes the specified command and pass the resultand reader to the delegate for processing.
		/// </summary>
		/// <param name="command">Command.</param>
		/// <param name="extractionDelegate">Extraction delegate.</param>
		private void ExecuteReaderCommand( string command, ReaderExtraction extractionDelegate )
		{
			lock( locker ) 
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				// Extract the command and extract items from the reader
				extractionDelegate( new SqliteCommand( command, connection ).ExecuteReader() );

				// There is a bug in Connection.Close such that it generates an error message if a transaction is not active.
				// So just begin a transaction here
				connection.BeginTransaction();
				connection.Close ();
			}
		}

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
					orderClause = sortOrderBuilder.Insert( 0, " ORDER BY " ).ToString();
				}
			}

			return orderClause;
		}

		/// <summary>
		/// Creates the general table.
		/// </summary>
		private void CreateGeneralTable()
		{
			ExecuteSimpleNonQuery( string.Format( "CREATE TABLE IF NOT EXISTS [{0}] ( ToneName TEXT PRIMARY KEY ASC, Overdue INTEGER )", GeneralItemsTableName ) );
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

		/// <summary>
		/// The name of the general items table
		/// </summary>
		private const string GeneralItemsTableName = "GeneralItems.Table";
	}
}