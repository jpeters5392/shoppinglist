using System;
using System.Collections.Generic;
using ReactiveUI.XamForms;
using ReactiveUI;
using shoppinglist.ViewModels;

namespace shoppinglist
{
    public partial class CategoriesPage : ReactiveContentPage<CategoriesViewModel>
    {
        public CategoriesPage()
        {
            InitializeComponent();

            this.WhenActivated(disposables => {
                disposables(this.OneWayBind(
                    this.ViewModel,
                    vm => vm.Categories,
                    v => v.Categories.ItemsSource));

                disposables(this.Bind(
                    this.ViewModel,
                    vm => vm.NewCategoryName,
                    v => v.NewCategory.Text));

                disposables(this.BindCommand(
                    this.ViewModel,
                    vm => vm.AddCategory,
                    v => v.AddCategory,
                    nameof(this.AddCategory.Clicked)));

				disposables(this.OneWayBind(
					this.ViewModel,
					vm => vm.Refresh,
                    v => v.Categories.RefreshCommand));

				disposables(this.OneWayBind(
					this.ViewModel,
					vm => vm.IsRefreshing,
					v => v.Categories.IsRefreshing));
            });

        }
    }
}
