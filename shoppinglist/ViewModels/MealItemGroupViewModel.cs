using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace shoppinglist.ViewModels
{
	public class MealItemGroupViewModel : List<MealItemViewModel>
	{
		public string Title { get; set; }
		public string ShortName { get; set; } //will be used for jump lists
		public MealItemGroupViewModel(string title, string shortName)
		{
			Title = title;
			ShortName = shortName;
		}
	}
}

