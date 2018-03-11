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
using System.Reactive.Disposables;
using System.Reactive;
using System.Diagnostics;

namespace shoppinglist.ViewModels
{
    public class ShoppingCartViewModel : ReactiveObject, ISupportsActivation
    {
        public ReactiveCommand<Unit, (string, string, string, double)> AddShoppingItem { get; }

        public ReactiveCommand<Unit, long> Refresh { get; }

        public ReactiveCommand OpenAddForm { get; }

        public ReactiveCommand CloseAddForm { get; }

		private ObservableAsPropertyHelper<IEnumerable<ShoppingItem>> _rawShoppingItems;
		public IEnumerable<ShoppingItem> RawShoppingItems => _rawShoppingItems.Value;

		private ObservableAsPropertyHelper<ObservableCollection<Category>> _rawCategories;
        public ObservableCollection<Category> RawCategories => _rawCategories.Value;

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

		private bool _shouldShowGrid;
		public bool ShouldShowGrid
		{
			get => _shouldShowGrid;
			set => this.RaiseAndSetIfChanged(ref _shouldShowGrid, value);
		}

        private ObservableAsPropertyHelper<bool> _isLoadingData;
        public bool IsLoadingData => _isLoadingData.Value;

        private Dictionary<string, string> CategoryItemsByKey;

        private Dictionary<string, string> CategoryItemsByName;

        private ShoppingItemService ShoppingItemService { get; set; }

        private CategoryService CategoryService { get; set; }

        private DataCache Cache { get; }

        private readonly ViewModelActivator _viewModelActivator = new ViewModelActivator();

        public ViewModelActivator Activator
        {
            get { return _viewModelActivator; }
        }

        public ShoppingCartViewModel()
		{
			ShoppingItemService = Locator.Current.GetService<ShoppingItemService>();
            CategoryService = Locator.Current.GetService<CategoryService>();
            Cache = Locator.Current.GetService<DataCache>();

            CategoryItemsByKey = new Dictionary<string, string>();
            CategoryItemsByName = new Dictionary<string, string>();

            ShouldShowGrid = false;

            this.WhenActivated(disposables =>
            {
                var lifecycle = new LifecycleLogger(GetType());
                lifecycle.DisposeWith(disposables);

                _rawShoppingItems = this.WhenAnyValue(x => x.Cache.ShoppingItems)
                                        .ToProperty(this, x => x.RawShoppingItems)
                                        .DisposeWith(disposables);

                _rawCategories = this.WhenAnyValue(x => x.Cache.Categories)
                                     .ToProperty(this, x => x.RawCategories)
                                     .DisposeWith(disposables);

                _categories = this.WhenAnyValue(x => x.RawCategories)
                                    .Select(categories =>
                                    {
                                        CategoryItemsByKey.Clear();
                                        CategoryItemsByName.Clear();

                                        if (categories == null) return new ObservableCollection<string>();

                                        return new ObservableCollection<string>(categories.Select(category => {
                                            CategoryItemsByKey.Add(category.Id, category.Name);
                                            CategoryItemsByName.Add(category.Name, category.Id);
                                            return category.Name;
                                        }));
                                    })
                                  .ToProperty(this, x => x.Categories)
                                  .DisposeWith(disposables);

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
                .ToProperty(this, x => x.ShoppingItems)
                .DisposeWith(disposables);

                _isLoadingData = ShoppingItemService.IsSyncing.StartWith(false).ToProperty(this, x => x.IsLoadingData).DisposeWith(disposables);

                Refresh.Select(_ => Unit.Default)
                       .Do(_ => {
                           Debug.WriteLine("Performing Refresh command");
                        })
                       .InvokeCommand(this, x => x.ShoppingItemService.Refresh).DisposeWith(disposables);

                AddShoppingItem.InvokeCommand(this, x => x.ShoppingItemService.AddShoppingItem).DisposeWith(disposables);
            });

            AddShoppingItem = ReactiveCommand.Create<Unit, (string, string, string, double)>((_) =>
            {
                ShouldShowGrid = false;

                string categoryId = null;
                if (NewItemSelectedCategory > -1)
                {
                    categoryId = CategoryItemsByName[Categories.ToList()[NewItemSelectedCategory]];
                }

                var newItem = (categoryId, NewItemName, NewItemDescription, NewQuantity);

                NewItemName = string.Empty;
                NewItemDescription = string.Empty;
                NewItemSelectedCategory = -1;
                NewQuantity = 0;

                return newItem;
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

            Refresh = ReactiveCommand.Create<Unit, long>((_) =>
            {
                return DateTime.Now.Ticks;
            });
		}
	}
}
