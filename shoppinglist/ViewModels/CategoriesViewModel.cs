using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Linq;
using shoppinglist.Models;
using shoppinglist.Services;
using Splat;
using System.Reactive.Linq;
using shoppinglist.Cache;
using System.Reactive;
using System.Reactive.Disposables;

namespace shoppinglist.ViewModels
{
    public class CategoriesViewModel : ReactiveObject, ISupportsActivation
    {
        public ReactiveCommand<Unit, string> AddCategory { get; }

        public ReactiveCommand<Unit, Unit> Refresh { get; }

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

        public ViewModelActivator Activator => new ViewModelActivator();

        public CategoriesViewModel()
        {
            CategoryService = Locator.Current.GetService<CategoryService>();
            Cache = Locator.Current.GetService<DataCache>();

            this.WhenActivated(disposables =>
            {
                _categories = this.WhenAnyValue(x => x.Cache.Categories)
                              .Select(categories => {
                                  var vms = categories.Select(category => new CategoryViewModel(category));
                                  return new ObservableCollection<CategoryViewModel>(vms);
                              })
                                  .ToProperty(this, x => x.Categories)
                                          .DisposeWith(disposables);

                AddCategory.InvokeCommand(this, x => x.CategoryService.AddCategoryItem)
                                          .DisposeWith(disposables);

                Refresh.InvokeCommand(this, x => x.CategoryService.Refresh)
                                          .DisposeWith(disposables);

                _isRefreshing = Observable.CombineLatest(Refresh.IsExecuting, CategoryService.IsSyncing)
                                          .Select(vals => vals.Any(x => x))
                                          .ToProperty(this, x => x.IsRefreshing)
                                          .DisposeWith(disposables);
            });

            AddCategory = ReactiveCommand.Create(() =>
            {
                var newCategoryName = NewCategoryName;
                NewCategoryName = string.Empty;
                return newCategoryName;
            });

            Refresh = ReactiveCommand.Create(() =>
            {
                return Unit.Default;
            });
        }
    }
}
