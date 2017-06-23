using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Linq;
using shoppinglist.Models;
using shoppinglist.Services;
using Splat;
using System.Reactive.Linq;
using shoppinglist.Cache;

namespace shoppinglist.ViewModels
{
    public class CategoriesViewModel : ReactiveObject
    {
        public ReactiveCommand AddCategory { get; }

        public ReactiveCommand Refresh { get; }

        private string _newCategoryName;
        public string NewCategoryName
        {
            get => _newCategoryName;
            set => this.RaiseAndSetIfChanged(ref _newCategoryName, value);
        }

        private ObservableAsPropertyHelper<ObservableCollection<CategoryViewModel>> _categories;
        public ObservableCollection<CategoryViewModel> Categories
        {
            get => _categories.Value;
        }

        private CategoryService CategoryService { get; set; }
        private DataCache Cache { get; }

		private ObservableAsPropertyHelper<bool> _isRefreshing;
		public bool IsRefreshing => _isRefreshing.Value;

        public CategoriesViewModel()
        {
            CategoryService = Locator.Current.GetService<CategoryService>();
            Cache = Locator.Current.GetService<DataCache>();

            _categories = this.WhenAnyValue(x => x.Cache.Categories)
                              .Select(categories => {
                var vms = categories.Select(category => new CategoryViewModel(category));
                return new ObservableCollection<CategoryViewModel>(vms);
            })
            .ToProperty(this, x => x.Categories);

            AddCategory = ReactiveCommand.CreateFromTask(async () =>
            {
                var newCategory = await CategoryService.AddCategory(NewCategoryName);

                NewCategoryName = string.Empty;
            });

            Refresh = ReactiveCommand.CreateFromTask(async () =>
			{
                await CategoryService.GetCategories();
			});

            _isRefreshing = Refresh.IsExecuting.ToProperty(this, x => x.IsRefreshing);
        }
    }
}
