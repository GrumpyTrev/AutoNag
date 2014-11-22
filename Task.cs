using System;

namespace AutoNag 
{
	/// <summary>
	/// Task business object
	/// </summary>
	public class Task
	{
		public Task ()
		{
		}

        public int ID { get; set; }

		public string Name
		{ 
			get
			{
				return taskName;
			}

			set
			{
				taskName = value;
			}
		}

		public string Notes
		{ 
			get
			{
				return taskMemo;
			}

			set
			{
				taskMemo = value;
			}
		}

		public bool Done { get; set; }	
		public int Priority
		{
			get;
			set;
		}

		public DateTime DueDate
		{
			get;
			set;
		}

		public DateTime ModifiedDate
		{
			get;
			set;
		}

		public bool NotificationRequired
		{
			get;
			set;
		}

		//
		// Private data
		//

		private string taskName = "";
		private string taskMemo = "";
	}
}