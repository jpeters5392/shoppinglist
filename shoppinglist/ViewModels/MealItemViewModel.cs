using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using shoppinglist.Models;

namespace shoppinglist.ViewModels
{
    public class MealItemViewModel : ReactiveObject
    {
		private ObservableCollection<MealItem> _breakfastMealItems;
        public ObservableCollection<MealItem> BreakfastMealItems 
        {
            get => _breakfastMealItems;
            set => this.RaiseAndSetIfChanged(ref _breakfastMealItems, value);
        }

		private ObservableCollection<MealItem> _lunchMealItems;
		public ObservableCollection<MealItem> LunchMealItems
		{
			get => _lunchMealItems;
			set => this.RaiseAndSetIfChanged(ref _lunchMealItems, value);
		}

		private ObservableCollection<MealItem> _dinnerMealItems;
		public ObservableCollection<MealItem> DinnerMealItems
		{
			get => _dinnerMealItems;
			set => this.RaiseAndSetIfChanged(ref _dinnerMealItems, value);
		}

        public MealItemViewModel()
        {
        }
    }
}
