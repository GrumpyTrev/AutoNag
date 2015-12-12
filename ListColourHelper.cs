// 
// File Details 
// -------------- 
//
// Project:     AutoNag
// Task:        User Interface
// Filename:    ListColourHelper.cs
// Created by:  T. Simmonds
//
//
// File Description
// ------------------
//
// Purpose:      The ListColourHelper class encapsulates some miscellaneous task list colour functions.
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
// (c) Copyright 2015 Trevor Simmonds.
// This software is protected by copyright, the design of any 
// article recorded in the software is protected by design 
// right and the information contained in the software is 
// confidential. This software may not be copied, any design 
// may not be reproduced and the information contained in the 
// software may not be used or disclosed except with the
// prior written permission of and in a manner permitted by
// the proprietors Trevor Simmonds (c) 2015
//
//    Copyright Holders:
//       Trevor Simmonds,
//       t.simmonds@virgin.net
//
using System;
using Android.Content;
using System.Collections;
using System.Collections.Generic;

namespace AutoNag
{
	/// <summary>
	/// The possible list colours.
	/// </summary>
	public enum ListColourEnum
	{
		Yellow,
		Green,
		Blue,
		Pink,
		Beige
	}

	/// <summary>
	/// The ListColourHelper class encapsulates some miscellaneous task list colour functions.
	/// </summary>
	public class ListColourHelper
	{
		//
		// Public methods
		//

		/// <summary>
		/// Gets the colour resource.
		/// </summary>
		/// <returns>The colour resource.</returns>
		/// <param name="listName">List name.</param>
		public static int GetColourResource( string listName )
		{
			return colourCollection[ ListColourPersistence.GetListColour( listName ) ];
		}

		/// <summary>
		/// Gets the colour resource.
		/// </summary>
		/// <returns>The colour resource.</returns>
		/// <param name="listName">List name.</param>
		public static int GetColourResource( ListColourEnum colour )
		{
			return colourCollection[ colour ];
		}

		/// <summary>
		/// Gets the default colour enum property.
		/// </summary>
		/// <value>The default colour enum property.</value>
		public static ListColourEnum DefaultColourEnumProperty
		{
			get
			{
				return defaultListColourEnum;
			}
		}

		/// <summary>
		/// Gets the no task list colour property.
		/// </summary>
		/// <value>The no task list colour property.</value>
		public static int NoTaskListColourProperty
		{
			get
			{
				return 0;
			}
		}

		/// <summary>
		/// Strings to colour enum.
		/// </summary>
		/// <returns>The to colour enum.</returns>
		/// <param name="colourName">Colour name.</param>
		public static ListColourEnum StringToColourEnum( string colourName )
		{
			ListColourEnum result = defaultListColourEnum;
			Enum.TryParse< ListColourEnum >( colourName, out result );
			return result;
		}

		/// <summary>
		/// Gets the overdue colour resource property.
		/// </summary>
		/// <value>The overdue colour resource property.</value>
		public static int OverdueColourResourceProperty
		{
			get
			{
				return Resource.Color.itemOverdueBackground;
			}
		}

		//
		// Private methods
		//

		/// <summary>
		/// Private constructor
		/// </summary>
		private ListColourHelper()
		{
		}

		/// <summary>
		/// Initializes the <see cref="AutoNag.ListColourHelper"/> class.
		/// </summary>
		static ListColourHelper()
		{
			colourCollection[ ListColourEnum.Yellow ] = Resource.Color.itemYellowBackground;
			colourCollection[ ListColourEnum.Blue ] = Resource.Color.itemBlueBackground;
			colourCollection[ ListColourEnum.Green ] = Resource.Color.itemGreenBackground;
			colourCollection[ ListColourEnum.Pink ] = Resource.Color.itemPinkBackground;
			colourCollection[ ListColourEnum.Beige ] = Resource.Color.itemBeigeBackground;
		}

		//
		// Private data
		//

		/// <summary>
		/// The colour collection.
		/// </summary>
		private static Dictionary< ListColourEnum, int > colourCollection = new Dictionary< ListColourEnum, int >();

		/// <summary>
		/// The default list colour enum.
		/// </summary>
		private static ListColourEnum defaultListColourEnum = ListColourEnum.Yellow;
	}
}


