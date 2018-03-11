using System;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using System.Diagnostics;
using Xamarin.Forms;
using System.IO;
using Plugin.Connectivity;
using Splat;
using shoppinglist.Models;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace shoppinglist.Services
{
    public class CategoryService : AzureService<Category>, IRefreshableService
	{
        public ReactiveCommand<Unit, long> Refresh { get; }
        public IObservable<IEnumerable<Category>> CategoryItems { get; }
        public ReactiveCommand<string, Category> AddCategoryItem { get; }

		protected override IMobileServiceSyncTable<Category> Table
		{
			get
			{
                return this.SqlInitializer.CategoryTable;
			}
		}

		public CategoryService() : base("allCategories")
		{
            CacheData = ReactiveCommand.CreateFromTask<Unit, IEnumerable<Category>>(async (_) =>
            {
                return await Table.ToListAsync();
            });

            CacheData.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to CacheData: {ex.Message}");
            }).DisposeWith(Disposables);

            InitCommands();

            CacheData.Do(_ => Debug.WriteLine("Caching category items"))
                     .InvokeCommand(this, x => x.CacheCollection)
                     .DisposeWith(Disposables);

            CategoryItems = CacheCollection.Select(items => items.Where(x => !string.IsNullOrWhiteSpace(x.Name)).OrderBy(x => x.Name))
                                           .Do(_ => Debug.WriteLine("Updated collection of categories"))
                                           .Publish()
                                           .RefCount();

            Refresh = ReactiveCommand.Create<Unit, long>(_ => DateTime.Now.Ticks);

            Refresh.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to Refresh: {ex.Message}");
            }).DisposeWith(Disposables);

            Refresh.Select(_ => Unit.Default)
                   .Do(_ => Debug.WriteLine("Syncing category items"))
                   .InvokeCommand(this, x => x.SyncItems).DisposeWith(Disposables);

            AddCategoryItem = ReactiveCommand.Create<string, Category>(name =>
            {
                return new Category
                {
                    Name = name
                };
            });

            AddCategoryItem.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to AddCategoryItem: {ex.Message}");
            }).DisposeWith(Disposables);

            AddCategoryItem.Do(_ => Debug.WriteLine("Adding category item to database"))
                           .InvokeCommand(this, x => x.AddItem)
                           .DisposeWith(Disposables);
		}

        protected override ObservableCollection<Category> CachedData
        {
            get => Cache.Categories;
            set 
            {
                Cache.Categories = value;
            }
        }
	}
}
