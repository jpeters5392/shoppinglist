using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using ReactiveUI;
using ReactiveUI.XamForms;
using shoppinglist.ViewModels;
using Xamarin.Forms;

namespace shoppinglist.Cells
{
	public partial class MealItemCell : ReactiveViewCell<MealItemViewModel>
	{
		public MealItemCell()
		{
			InitializeComponent();

			this.WhenActivated(disposables =>
			{
                disposables(this.OneWayBind(
                    this.ViewModel,
                    vm => vm.BreakfastMealItems,
                    v => v.BreakfastItems.Text,
                    (items) => string.Join(", ", items.Select(x => x.Name))));

				disposables(this.OneWayBind(
					this.ViewModel,
					vm => vm.LunchMealItems,
					v => v.LunchItems.Text,
					(items) => string.Join(", ", items.Select(x => x.Name))));

				disposables(this.OneWayBind(
					this.ViewModel,
					vm => vm.DinnerMealItems,
					v => v.DinnerItems.Text,
					(items) => string.Join(", ", items.Select(x => x.Name))));
			});
		}
	}
}
