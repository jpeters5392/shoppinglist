using System;
using System.Collections.Generic;
using ReactiveUI;
using ReactiveUI.XamForms;
using shoppinglist.ViewModels;
using Xamarin.Forms;

namespace shoppinglist.Cells
{
	public partial class ShoppingItemCell : ReactiveViewCell<ShoppingItemViewModel>
	{
		public ShoppingItemCell()
		{
			InitializeComponent();

			this.WhenActivated(disposables =>
			{
				disposables(this.OneWayBind(
					this.ViewModel,
					vm => vm.Name,
					v => v.ItemName.Text));

				disposables(this.OneWayBind(
					this.ViewModel,
                    vm => vm.Description,
                    v => v.ItemDescription.Text));

				disposables(this.OneWayBind(
					this.ViewModel,
					vm => vm.Quantity,
                    v => v.Quantity.Text));
			});
		}
	}
}
