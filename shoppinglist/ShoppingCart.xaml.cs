using System;
using System.Collections.Generic;
using ReactiveUI.XamForms;
using ReactiveUI;
using shoppinglist.ViewModels;
using Xamarin.Forms;

namespace shoppinglist
{
    public partial class ShoppingCart : ReactiveContentPage<ShoppingCartViewModel>
    {
        public ShoppingCart()
        {
            InitializeComponent();

			this.WhenActivated(disposables =>
			{
				disposables(this.OneWayBind(
					this.ViewModel,
                    vm => vm.ShoppingItems,
					v => v.ShoppingItems.ItemsSource));

                disposables(this.OneWayBind(
                    this.ViewModel,
                    vm => vm.Categories,
                    v => v.CategoryType.ItemsSource));

				disposables(this.Bind(
					this.ViewModel,
                    vm => vm.NewItemName,
                    v => v.NewItem.Text));

				disposables(this.Bind(
					this.ViewModel,
                    vm => vm.NewItemDescription,
                    v => v.NewDescription.Text));

				disposables(this.Bind(
					this.ViewModel,
                    vm => vm.NewQuantity,
                    v => v.NewQuantity.Text));

				disposables(this.Bind(
					this.ViewModel,
                    vm => vm.NewItemSelectedCategory,
                    v => v.CategoryType.SelectedIndex));

				disposables(this.Bind(
                    this.ViewModel,
					vm => vm.IsLoadingData,
                    v => v.ProgressIndicator.IsVisible));

				disposables(this.Bind(
                    this.ViewModel,
                    vm => vm.ShouldShowGrid,
                    v => v.AddNewItem.IsVisible));

				disposables(this.Bind(
					this.ViewModel,
                    vm => vm.IsLoadingData,
                    v => v.ProgressIndicator.IsRunning));

				disposables(this.BindCommand(
					this.ViewModel,
                    vm => vm.AddShoppingItem,
                    v => v.AddItem,
					nameof(this.AddItem.Clicked)));

				disposables(this.BindCommand(
					this.ViewModel,
                    vm => vm.OpenAddForm,
                    v => v.FAB,
					nameof(this.FAB.Clicked)));

				disposables(this.BindCommand(
					this.ViewModel,
                    vm => vm.CloseAddForm,
					v => v.CloseForm,
                    nameof(this.CloseForm.Tapped)));
			});

            this.ShoppingItems.ItemSelected += (sender, e) =>
            {
                ((ListView)sender).SelectedItem = null;
            };
        }
    }
}
