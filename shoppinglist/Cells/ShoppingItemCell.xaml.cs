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

				disposables(this.OneWayBind(
					this.ViewModel,
                    vm => vm.IsCompleted,
                    v => v.CompletionStatus.Source,
                    (isCompleted) => {
                        if (isCompleted) {
                            return ImageSource.FromFile("ic_check_box.png");
                        }

                        return ImageSource.FromFile("ic_check_box_outline.png");
                }));

				disposables(this.BindCommand(
					this.ViewModel,
                    vm => vm.ItemSelected,
                    v => v.CellSelected,
                    nameof(this.CellSelected.Tapped)));
			});
		}
	}
}
