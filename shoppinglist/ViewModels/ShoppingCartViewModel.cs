using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Linq;
using shoppinglist.Models;
using shoppinglist.Services;
using Splat;
using System.Reactive.Linq;
using System.Collections.Generic;
using shoppinglist.Cache;

namespace shoppinglist.ViewModels
{
	public class ShoppingCartViewModel : ReactiveObject
    {
        public ReactiveCommand AddShoppingItem { get; }

        public ReactiveCommand Refresh { get; }

        public ReactiveCommand OpenAddForm { get; }

        public ReactiveCommand CloseAddForm { get; }

		private readonly ObservableAsPropertyHelper<IEnumerable<ShoppingItem>> _rawShoppingItems;
		public IEnumerable<ShoppingItem> RawShoppingItems => _rawShoppingItems.Value;

		private readonly ObservableAsPropertyHelper<ObservableCollection<Category>> _rawCategories;
        public ObservableCollection<Category> RawCategories => _rawCategories.Value;

		private readonly ObservableAsPropertyHelper<ObservableCollection<ShoppingItemGroupViewModel>> _shoppingItems;
		public ObservableCollection<ShoppingItemGroupViewModel> ShoppingItems
		{
			get => _shoppingItems.Value;
		}

        private readonly ObservableAsPropertyHelper<ObservableCollection<string>> _categories;
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

		private bool _shouldShowGrid;
		public bool ShouldShowGrid
		{
			get => _shouldShowGrid;
			set => this.RaiseAndSetIfChanged(ref _shouldShowGrid, value);
		}

		private bool _isLoadingData;
		public bool IsLoadingData
		{
			get => _isLoadingData;
			set => this.RaiseAndSetIfChanged(ref _isLoadingData, value);
		}

        private ObservableAsPropertyHelper<bool> _isRefreshing;
        public bool IsRefreshing => _isRefreshing.Value;

        private Dictionary<string, string> CategoryItemsByKey;

        private Dictionary<string, string> CategoryItemsByName;

        private ShoppingItemService ShoppingItemService { get; set; }

        private CategoryService CategoryService { get; set; }

        private DataCache Cache { get; }

        public ShoppingCartViewModel()
		{
			ShoppingItemService = Locator.Current.GetService<ShoppingItemService>();
            CategoryService = Locator.Current.GetService<CategoryService>();
            Cache = Locator.Current.GetService<DataCache>();

            CategoryItemsByKey = new Dictionary<string, string>();
            CategoryItemsByName = new Dictionary<string, string>();

            ShouldShowGrid = false;

            _rawShoppingItems = this.WhenAnyValue(x => x.Cache.ShoppingItems)
                .ToProperty(this, x => x.RawShoppingItems);

            _rawCategories = this.WhenAnyValue(x => x.Cache.Categories)
                              .ToProperty(this, x => x.RawCategories);
            
            _categories = this.WhenAnyValue(x => x.RawCategories)
			.Select(categories =>
			{
                CategoryItemsByKey.Clear();
                CategoryItemsByName.Clear();
                return new ObservableCollection<string>(categories.Select(category => {
                    CategoryItemsByKey.Add(category.Id, category.Name);
                    CategoryItemsByName.Add(category.Name, category.Id);
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
                    if (x.Key != null && CategoryItemsByKey.ContainsKey(x.Key))
                    {
                        categoryName = CategoryItemsByKey[x.Key];
                    }
                    else
                    {
                        categoryName = "None";
                    }

					var group = new ShoppingItemGroupViewModel(categoryName, string.Empty);
					group.AddRange(x.Select(item => new ShoppingItemViewModel(item)));
					return group;
				}));
			})
			.ToProperty(this, x => x.ShoppingItems);

            AddShoppingItem = ReactiveCommand.CreateFromTask(async () =>
            {
                ShouldShowGrid = false;

                string categoryId = null;
                if (NewItemSelectedCategory > -1)
                {
                    categoryId = CategoryItemsByName[Categories.ToList()[NewItemSelectedCategory]];
                }

                var newItem = await ShoppingItemService.AddShoppingItem(
                    categoryId,
                    NewItemName,
                    NewItemDescription,
                    NewQuantity
                );

                NewItemName = string.Empty;
                NewItemDescription = string.Empty;
                NewItemSelectedCategory = -1;
                NewQuantity = 0;
            });

            AddShoppingItem.IsExecuting.Subscribe(isExecuting => {
                IsLoadingData = isExecuting;
            });

            OpenAddForm = ReactiveCommand.Create(() =>
            {
                ShouldShowGrid = true;
            });

			CloseAddForm = ReactiveCommand.Create(() =>
			{
				NewItemName = string.Empty;
				NewItemDescription = string.Empty;
				NewItemSelectedCategory = -1;
				NewQuantity = 0;
                ShouldShowGrid = false;
			});

            Refresh = ReactiveCommand.CreateFromTask(async () =>
            {
                await ShoppingItemService.GetShoppingItems();
            });

            _isRefreshing = Refresh.IsExecuting.ToProperty(this, x => x.IsRefreshing);
		}
	}
}
