// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        TaskManagement
// Filename:    TaskManager.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The TaskManager class is an abstraction on the data access layer.
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

namespace AutoNag
{
	/// <summary>
	/// The TaskManager class is an abstraction on the data access layer
	/// </summary>
	public abstract class TaskManager 
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
			return TaskRepository.GetTask( id );
		}

		/// <summary>
		/// Gets all of the the tasks.
		/// </summary>
		/// <returns>The tasks stored in a IList<Task></returns>
		/// <param name="sortOrder">Sort order to be applied to the tasks.</param>
		public static IList< Task > GetTasks( IList< Task.SortOrders > sortOrder )
		{
			return TaskRepository.GetTasks( sortOrder );
		}

		/// <summary>
		/// Saves the task.
		/// </summary>
		/// <returns>Whether or not the item was saved</returns>
		/// <param name="item">Item.</param>
		public static bool SaveTask( Task item )
		{
			return TaskRepository.SaveTask( item );
		}

		/// <summary>
		/// Deletes the task.
		/// </summary>
		/// <returns><c>true</c>, if task was deleted, <c>false</c> otherwise.</returns>
		/// <param name="id">Identifier.</param>
		public static bool DeleteTask( int id )
		{
			return TaskRepository.DeleteTask( id );
		}

		//
		// Private methods
		// 

		/// <summary>
		/// Private constructor
		/// </summary>
		private TaskManager ()
		{
		}
	}
}