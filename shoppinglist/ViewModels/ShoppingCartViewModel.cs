using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Linq;
using shoppinglist.Models;
using shoppinglist.Services;
using Splat;
using System.Reactive.Linq;
using System.Collections.Generic;

namespace shoppinglist.ViewModels
{
	public class ShoppingCartViewModel : ReactiveObject
    {
        public ReactiveCommand AddShoppingItem { get; }

		private ObservableAsPropertyHelper<IEnumerable<ShoppingItem>> _rawShoppingItems;
		public IEnumerable<ShoppingItem> RawShoppingItems
		{
			get => _rawShoppingItems.Value;
		}

		private ObservableAsPropertyHelper<ObservableCollection<ShoppingItemGroupViewModel>> _shoppingItems;
		public ObservableCollection<ShoppingItemGroupViewModel> ShoppingItems
		{
			get => _shoppingItems.Value;
		}

        private ObservableAsPropertyHelper<ObservableCollection<string>> _categories;
		public ObservableCollection<string> Categories
		{
			get => _categories.Value;
		}

		private int _newItemSelectedCategory;
		public int NewItemSelectedCategory
		{
			get => _newItemSelectedCategory;
			set => this.RaiseAndSetIfChanged(ref _newItemSelectedCategory, value);
		}

		private double _newQuantity;
		public double NewQuantity
		{
			get => _newQuantity;
			set => this.RaiseAndSetIfChanged(ref _newQuantity, value);
		}

		private string _newItemDescription;
		public string NewItemDescription
		{
			get => _newItemDescription;
			set => this.RaiseAndSetIfChanged(ref _newItemDescription, value);
		}

		private string _newItemName;
		public string NewItemName
		{
			get => _newItemName;
			set => this.RaiseAndSetIfChanged(ref _newItemName, value);
		}

        private Dictionary<string, string> CategoryItems;

        private ShoppingItemService ShoppingItemService { get; set; }

        private CategoryService CategoryService { get; set; }

        public ShoppingCartViewModel()
		{
			ShoppingItemService = Locator.Current.GetService<ShoppingItemService>();
            CategoryService = Locator.Current.GetService<CategoryService>();

            CategoryItems = new Dictionary<string, string>();

            var shoppingItemsObservable = Observable.FromAsync(ShoppingItemService.GetShoppingItems)
                                          .ObserveOn(RxApp.MainThreadScheduler);
            
            _rawShoppingItems = shoppingItemsObservable.ToProperty(this, x => x.RawShoppingItems);
            
			_categories = Observable.FromAsync(CategoryService.GetCategories)
			.ObserveOn(RxApp.MainThreadScheduler)
			.Select(categories =>
			{
                CategoryItems.Clear();
                return new ObservableCollection<string>(categories.Select(category => {
                    CategoryItems.Add(category.Id, category.Name);
                    return category.Name;
                }));
			})
			.ToProperty(this, x => x.Categories);

            _shoppingItems = this.WhenAnyValue(x => x.RawShoppingItems, x => x.Categories)
			.Select(results =>
			{
                if (results == null || results.Item1 == null || results.Item2 == null) return null;
                var items = results.Item1;
				var categories = Categories.ToList();
				return new ObservableCollection<ShoppingItemGroupViewModel>(items.GroupBy(x => x.CategoryId).Select(x =>
				{
                    string categoryName = "";
                    if (CategoryItems.ContainsKey(x.Key))
                    {
                        categoryName = CategoryItems[x.Key];
                    }

					var group = new ShoppingItemGroupViewModel(categoryName, string.Empty);
					group.AddRange(x.Select(item => new ShoppingItemViewModel(item)));
					return group;
				}));
			})
			.ToProperty(this, x => x.ShoppingItems);

            AddShoppingItem = ReactiveCommand.CreateFromTask(async () =>
            {
                string categoryId = null;
                if (NewItemSelectedCategory > -1)
                {
                    categoryId = CategoryItems[Categories.ToList()[NewItemSelectedCategory]];
                }
                var newItem = await ShoppingItemService.AddShoppingItem(
                    categoryId,
                    NewItemName,
                    NewItemDescription,
                    NewQuantity
                );
                var category = ShoppingItems.Where(x => x.Title == categoryId).FirstOrDefault();
                if (category != null)
                {
                    category.Add(new ShoppingItemViewModel(newItem));
                }

                NewItemName = string.Empty;
                NewItemDescription = string.Empty;
                NewItemSelectedCategory = -1;
                NewQuantity = 0;
            });
		}
	}
}
