using System;
using System.Collections.Generic;

using Mono.Data.Sqlite;
using System.IO;
using System.Data;

namespace AutoNag
{
	/// <summary>
	/// TaskDatabase uses ADO.NET to create the [Items] table and create,read,update,delete data
	/// </summary>
	public class TaskDatabase 
	{
		static object locker = new object ();

		public string connectionString;

		/// <summary>
		/// Initializes a new instance of the <see cref="Tasky.DL.TaskDatabase"/> TaskDatabase. 
		/// if the database doesn't exist, it will create the database and all the tables.
		/// </summary>
		public TaskDatabase( string dbPath ) 
		{
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

		/// <summary>Convert from DataReader to Task object</summary>
		Task FromReader( SqliteDataReader reader )
		{
			Task toTask = new Task();
			toTask.ID =  Convert.ToInt32( reader[ "Identity" ] );
			toTask.Name = reader ["Name"].ToString();
			toTask.Notes = reader ["Notes"].ToString();
			toTask.Done = Convert.ToBoolean( reader ["Done"] );
			toTask.NotificationRequired = Convert.ToBoolean( reader ["NotificationRequired"] );
			toTask.Priority	= Convert.ToInt32( reader[ "Priority" ] );

			try
			{
				toTask.DueDate = DateTime.ParseExact( reader[ "DueDate" ].ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture );
			}
			catch ( Exception )
			{
				toTask.DueDate = DateTime.MinValue;
			}

			try
			{
				toTask.ModifiedDate = DateTime.ParseExact( reader[ "ModifiedDate" ].ToString(), "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture );
			}
			catch ( Exception )
			{
				toTask.ModifiedDate = DateTime.MinValue;
			}

			return toTask;
		}

		public IEnumerable<Task> GetItems()
		{
			List< Task > taskList = new List< Task >();

			lock( locker ) 
			{
				SqliteConnection connection = new SqliteConnection( connectionString );
				connection.Open();

				SqliteCommand command = connection.CreateCommand();
				command.CommandText = "SELECT [Identity], [Name], [Notes], [Done], [NotificationRequired], [Priority], [DueDate], [ModifiedDate] from [Items]";

				SqliteDataReader reader = command.ExecuteReader();
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
				command.Parameters.Add( new SqliteParameter( DbType.String ) { Value = item.DueDate.ToString( "yyyyMMdd" ) } );
				command.Parameters.Add( new SqliteParameter( DbType.String ) { Value = item.ModifiedDate.ToString( "yyyyMMddHHmmss" ) } );

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
	}
}