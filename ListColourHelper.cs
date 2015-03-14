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
	/// The background drawable and colour resources
	/// </summary>
	public class ListColour
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AutoNag.ListColour"/> class.
		/// </summary>
		/// <param name="drawableId">Drawable identifier.</param>
		/// <param name="colourId">Colour identifier.</param>
		public ListColour( int drawableId, int colourId )
		{
			drawableResourceId = drawableId;
			colourResourceId = colourId;
		}

		/// <summary>
		/// Gets the drawable property.
		/// </summary>
		/// <value>The drawable property.</value>
		public int DrawableProperty
		{
			get
			{
				return drawableResourceId;
			}
		}

		/// <summary>
		/// Gets the colour property.
		/// </summary>
		/// <value>The colour property.</value>
		public int ColourProperty
		{
			get
			{
				return colourResourceId;
			}
		}

		//
		// Private data
		//

		/// <summary>
		/// The drawable resource identifier.
		/// </summary>
		private readonly int drawableResourceId = 0;

		/// <summary>
		/// The colour resource identifier.
		/// </summary>
		private readonly int colourResourceId = 0;
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
			return colourCollection[ ListColourPersistence.GetListColour( listName ) ].ColourProperty;
		}

		/// <summary>
		/// Gets the drawable resource.
		/// </summary>
		/// <returns>The drawable resource.</returns>
		/// <param name="colour">Colour.</param>
		public static int GetDrawableResource( ListColourEnum colour )
		{
			return colourCollection[ colour ].DrawableProperty;
		}

		/// <summary>
		/// Gets the drawable resource.
		/// </summary>
		/// <returns>The drawable resource.</returns>
		/// <param name="listName">List name.</param>
		public static int GetDrawableResource( string listName )
		{
			return colourCollection[ ListColourPersistence.GetListColour( listName ) ].DrawableProperty;
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
			colourCollection[ ListColourEnum.Yellow ] = new ListColour( Resource.Drawable.NotDoneBackgroundYellow, Resource.Color.itemYellowBackground );
			colourCollection[ ListColourEnum.Blue ] = new ListColour( Resource.Drawable.NotDoneBackgroundBlue, Resource.Color.itemBlueBackground );
			colourCollection[ ListColourEnum.Green ] = new ListColour( Resource.Drawable.NotDoneBackgroundGreen, Resource.Color.itemGreenBackground );
			colourCollection[ ListColourEnum.Pink ] = new ListColour( Resource.Drawable.NotDoneBackgroundPink, Resource.Color.itemPinkBackground );
			colourCollection[ ListColourEnum.Beige ] = new ListColour( Resource.Drawable.NotDoneBackgroundBeige, Resource.Color.itemBeigeBackground );
		}

		//
		// Private data
		//

		/// <summary>
		/// The colour collection.
		/// </summary>
		private static Dictionary< ListColourEnum, ListColour > colourCollection = new Dictionary< ListColourEnum, ListColour >();

		/// <summary>
		/// The default list colour.
		/// </summary>
		private static ListColour defaultListColour = new ListColour( Resource.Drawable.NotDoneBackgroundYellow, Resource.Color.itemYellowBackground );

		/// <summary>
		/// The default list colour enum.
		/// </summary>
		private static ListColourEnum defaultListColourEnum = ListColourEnum.Yellow;
	}
}


