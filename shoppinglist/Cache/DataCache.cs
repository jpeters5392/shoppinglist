using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using ReactiveUI;
using shoppinglist.Models;

namespace shoppinglist.Cache
{
    public class DataCache : ReactiveObject
    {
        private ObservableCollection<ShoppingItem> _shoppingItems;
        public ObservableCollection<ShoppingItem> ShoppingItems
        {
            get => _shoppingItems;
            set => this.RaiseAndSetIfChanged(ref _shoppingItems, value);
        }

		private ObservableCollection<Category> _categories;
		public ObservableCollection<Category> Categories
		{
			get => _categories;
			set => this.RaiseAndSetIfChanged(ref _categories, value);
		}

		private ObservableCollection<MealItem> _mealItems;
		public ObservableCollection<MealItem> MealItems
		{
			get => _mealItems;
			set => this.RaiseAndSetIfChanged(ref _mealItems, value);
		}

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public DataCache()
        {
            ShoppingItems = new ObservableCollection<ShoppingItem>();
            Categories = new ObservableCollection<Category>();
            MealItems = new ObservableCollection<MealItem>();
        }
    }
}
