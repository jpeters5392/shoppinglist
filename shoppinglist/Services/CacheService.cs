using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using shoppinglist.Models;
using Splat;

namespace shoppinglist.Services
{
    public class CacheService: ReactiveObject
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

        public ReactiveCommand SyncShoppingItems { get; }
        public ReactiveCommand SyncCategories { get; }

        private ShoppingItemService ShoppingItemService { get; }
        private CategoryService CategoryService { get; }

        public CacheService()
        {
			ShoppingItemService = Locator.Current.GetService<ShoppingItemService>();
			CategoryService = Locator.Current.GetService<CategoryService>();

            SyncShoppingItems = ReactiveCommand.CreateFromTask(async () =>
            {
                var shoppingItems = await ShoppingItemService.GetShoppingItems();
                ShoppingItems = new ObservableCollection<ShoppingItem>(shoppingItems);
            });

			SyncCategories = ReactiveCommand.CreateFromTask(async () =>
			{
                var categories = await CategoryService.GetCategories();
				Categories = new ObservableCollection<Category>(categories);
			});
        }
    }
}
