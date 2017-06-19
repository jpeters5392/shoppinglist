using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace shoppinglist.ViewModels
{
    public class ShoppingItemGroupViewModel : List<ShoppingItemViewModel>
    {
		public string Title { get; set; }
		public string ShortName { get; set; } //will be used for jump lists
		public ShoppingItemGroupViewModel(string title, string shortName)
		{
			Title = title;
			ShortName = shortName;
		}
    }
}

