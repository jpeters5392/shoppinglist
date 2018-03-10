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

            Disposables.Add(CacheData.InvokeCommand(this, x => x.CacheCollection));

            CategoryItems = CacheCollection.Select(items => items.Where(x => true).OrderBy(x => x.Name))
                                           .Publish()
                                           .RefCount();

            Disposables.Add(CategoryItems.InvokeCommand(this, x => x.CacheCollection));

            Refresh = ReactiveCommand.Create<Unit, long>(_ => DateTime.Now.Ticks);

            Disposables.Add(Refresh.Select(_ => Unit.Default).InvokeCommand(this, x => x.SyncItems));

            AddCategoryItem = ReactiveCommand.Create<string, Category>(name =>
            {
                return new Category
                {
                    Name = name
                };
            });

            Disposables.Add(AddCategoryItem.InvokeCommand(this, x => x.AddItem));

            Disposables.Add(CategoryItems.InvokeCommand(this, x => x.CacheCollection));
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
