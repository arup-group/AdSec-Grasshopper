using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace AdSecGH.Components.Ribbon
{
	/// <summary>
	/// Class containing the ribbon tab display name
	/// 
	/// Call this class from all components in plugin for naming consistency
	/// </summary>
	public class CategoryName
	{
		public static string Name()
		{
			return "AdSec";
		}
	}

	/// <summary>
	/// Class containing ribbon category names
	/// 
	/// Call this class from all components to pick which category component should sit in
	/// 
	/// Sorting of categories in the ribbon is controlled with a number of spaces in front of the name
	/// to avoid naming each category with a number in front. Spaces will automatically be removed when displayed
	/// </summary>
	internal class SubCategoryName
	{
		public static string Cat0()
		{
			return new string(' ', 8) + "File";
		}

		public static string Cat1()
		{
			return new string(' ', 7) + "Material";
		}
		public static string Cat2()
		{
			return new string(' ', 6) + "Profile";
		}
		public static string Cat3()
		{
			return new string(' ', 5) + "Rebar";
		}
		public static string Cat4()
		{
			return new string(' ', 4) + "Section";
		}

		public static string Cat5()
		{
			return new string(' ', 3) + "Loads";
		}

		public static string Cat6()
		{
			return new string(' ', 2) + "Solution";
		}

		public static string Cat7()
		{
			return new string(' ', 1) + "Results";
		}

		public static string Cat8()
		{
			return "test";
		}

		public static string Cat9()
		{
			return "Params";
		}
	}
}
