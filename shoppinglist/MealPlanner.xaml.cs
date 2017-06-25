using System;
using System.Collections.Generic;
using ReactiveUI;
using ReactiveUI.XamForms;
using shoppinglist.ViewModels;
using Xamarin.Forms;

namespace shoppinglist
{
    public partial class MealPlanner : ReactiveContentPage<MealPlannerViewModel>
    {
        public MealPlanner()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
				disposables(this.OneWayBind(
					this.ViewModel,
					vm => vm.Refresh,
					v => v.MealItems.RefreshCommand));

				disposables(this.OneWayBind(
					this.ViewModel,
					vm => vm.IsRefreshing,
					v => v.MealItems.IsRefreshing));

				disposables(this.OneWayBind(
					this.ViewModel,
                    vm => vm.MealItemGroups,
                    v => v.MealItems.ItemsSource));

				disposables(this.BindCommand(
					this.ViewModel,
					vm => vm.OpenAddMealItemForm,
					v => v.FAB,
					nameof(this.FAB.Clicked)));

				disposables(this.WhenAnyValue(x => x.ViewModel.ShouldShowGrid)
					.Subscribe(shouldShowGrid =>
					{
						if (shouldShowGrid)
						{
							var bounds = new Rectangle(0, 1, this.ParentLayout.Width, this.ParentLayout.Height);
							this.AddNewMealItem.LayoutTo(bounds);
						}
						else
						{
							var bounds = new Rectangle(0, 1, this.ParentLayout.Width, 0);
							this.AddNewMealItem.LayoutTo(bounds);
						}
					}));

				disposables(this.BindCommand(
					this.ViewModel,
					vm => vm.CloseAddMealItemForm,
					v => v.CloseForm,
					nameof(this.CloseForm.Tapped)));

				disposables(this.OneWayBind(
					this.ViewModel,
					vm => vm.MealTypes,
					v => v.MealTypes.ItemsSource));

				disposables(this.Bind(
					this.ViewModel,
                    vm => vm.NewItemName,
                    v => v.NewItem.Text));

				disposables(this.Bind(
					this.ViewModel,
					vm => vm.NewItemDate,
                    v => v.NewDate.Date));

				disposables(this.Bind(
					this.ViewModel,
                    vm => vm.NewItemSelectedMealType,
                    v => v.MealTypes.SelectedIndex));

				disposables(this.BindCommand(
					this.ViewModel,
					vm => vm.AddMealItem,
					v => v.AddItem,
					nameof(this.AddItem.Clicked)));

				disposables(this.Bind(
					this.ViewModel,
					vm => vm.IsLoadingData,
					v => v.ProgressIndicator.IsRunning));
            });

			this.MealItems.ItemSelected += (sender, e) =>
			{
				((ListView)sender).SelectedItem = null;
			};
        }
    }
}
