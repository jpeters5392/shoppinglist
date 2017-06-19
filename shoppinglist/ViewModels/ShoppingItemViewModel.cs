using System;
using ReactiveUI;
using shoppinglist.Models;

namespace shoppinglist.ViewModels
{
	public class ShoppingItemViewModel : ReactiveObject
	{
		private string _name;
		public string Name
		{
			get => _name;
			set => this.RaiseAndSetIfChanged(ref _name, value);
		}

		private string _description;
		public string Description
		{
			get => _description;
			set => this.RaiseAndSetIfChanged(ref _description, value);
		}

		private double _quantity;
		public double Quantity
		{
			get => _quantity;
			set => this.RaiseAndSetIfChanged(ref _quantity, value);
		}

		public ShoppingItemViewModel(ShoppingItem item)
		{
			Name = item.Name;
            Description = item.Description;
            Quantity = item.Quantity;
		}
	}
}
