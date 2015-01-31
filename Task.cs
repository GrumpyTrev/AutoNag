// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        TaskManagement
// Filename:    Task.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The Task class is a wrapper around the task details written to and retrieved from storage
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

namespace AutoNag 
{
	/// <summary>
	/// The Task class is a wrapper around the task details written to and retrieved from storage
	/// </summary>
	public class Task
	{
		/// <summary>
		/// The available task sort orders
		/// </summary>
		public enum SortOrders
		{
			Priority,
			DueDate,
			Done
		}

		//
		// Public methods
		//

		/// <summary>
		/// Default constructor
		/// </summary>
		public Task ()
		{
		}

		/// <summary>
		/// The unique identity for the task
		/// </summary>
		/// <value>The ID</value>
        public int ID
		{ 
			get; 
			set; 
		}

		/// <summary>
		/// Gets or sets the task name.
		/// </summary>
		/// <value>The name.</value>
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

		/// <summary>
		/// The content of the task
		/// </summary>
		/// <value>The notes.</value>
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

		/// <summary>
		/// Gets or sets a value indicating whether this task is done.
		/// </summary>
		/// <value><c>true</c> if done; otherwise, <c>false</c>.</value>
		public bool Done 
		{ 
			get; 
			set; 
		}

		/// <summary>
		/// Gets or sets the priority.
		/// </summary>
		/// <value>The priority.</value>
		public int Priority
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the due date.
		/// </summary>
		/// <value>The due date.</value>
		public DateTime DueDate
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the modified date.
		/// </summary>
		/// <value>The modified date.</value>
		public DateTime ModifiedDate
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="AutoNag.Task"/> notification required.
		/// </summary>
		/// <value><c>true</c> if notification required; otherwise, <c>false</c>.</value>
		public bool NotificationRequired
		{
			get;
			set;
		}

		//
		// Private data
		//

		/// <summary>
		/// The name of the task. Explictly defined so that the get property always returns a non-null
		/// </summary>
		private string taskName = "";

		/// <summary>
		/// The task content. Explictly defined so that the get property always returns a non-null
		/// </summary>
		private string taskMemo = "";
	}
}