using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Linq;
using shoppinglist.Models;
using shoppinglist.Services;
using Splat;
using System.Reactive.Linq;

namespace shoppinglist.ViewModels
{
    public class CategoriesViewModel : ReactiveObject
    {
        public ReactiveCommand AddCategory { get; }

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

        public CategoriesViewModel()
        {
            CategoryService = Locator.Current.GetService<CategoryService>();

            _categories = Observable.FromAsync(CategoryService.GetCategories)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(categories => {
                var vms = categories.Select(category => new CategoryViewModel(category));
                return new ObservableCollection<CategoryViewModel>(vms);
            })
            .ToProperty(this, x => x.Categories);

            AddCategory = ReactiveCommand.CreateFromTask(async () =>
            {
                var newCategory = await CategoryService.AddCategory(NewCategoryName);
                Categories.Add(new CategoryViewModel(newCategory));

                NewCategoryName = string.Empty;
            });
        }
    }
}
