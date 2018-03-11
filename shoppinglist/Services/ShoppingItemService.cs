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
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace shoppinglist.Services
{
    public class ShoppingItemService: AzureService<ShoppingItem>, IRefreshableService
	{
        public ReactiveCommand<Unit, long> Refresh { get; }

        public IObservable<IEnumerable<ShoppingItem>> ShoppingItems { get; }

        public ReactiveCommand<(string categoryId, string name, string description, double quantity), ShoppingItem> AddShoppingItem { get; }

        public ReactiveCommand<string, ShoppingItem> CompleteItem { get; }

        public ReactiveCommand<string, ShoppingItem> UncompleteItem { get; }

        protected override IMobileServiceSyncTable<ShoppingItem> Table
        {
            get
            {
                return this.SqlInitializer.ShoppingItemTable;
            }
        }

        public ShoppingItemService(): base("allShoppingItems")
        {
            CacheData = ReactiveCommand.CreateFromTask<Unit, IEnumerable<ShoppingItem>>(async (_) =>
            {
                return await Table.Where(x => x.CompletedOn == DateTime.MinValue || x.CompletedOn >= DateTime.Now.AddHours(-24)).ToListAsync();
            });

            CacheData.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to CacheData: {ex.Message}");
            }).DisposeWith(Disposables);

            InitCommands();

            CacheData.Do(_ => Debug.WriteLine("Caching shopping items"))
                     .InvokeCommand(this, x => x.CacheCollection)
                     .DisposeWith(Disposables);

            ShoppingItems = CacheCollection.Select(items => items.Where(x => x.CompletedOn == DateTime.MinValue || x.CompletedOn >= DateTime.Now.AddHours(-24)).OrderBy(c => c.Name))
                                           .Publish()
                                           .RefCount();

            Refresh = ReactiveCommand.Create<Unit, long>(_ => DateTime.Now.Ticks);

            Refresh.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to Refresh: {ex.Message}");
            }).DisposeWith(Disposables);

            Refresh.Select(_ => Unit.Default)
                   .Do(_ => Debug.WriteLine("Syncing shopping items"))
                   .InvokeCommand(this, x => x.SyncItems)
                   .DisposeWith(Disposables);

            AddShoppingItem = ReactiveCommand.Create<(string categoryId, string name, string description, double quantity), ShoppingItem>((args) =>
            {
                return new ShoppingItem
                {
                    Name = args.name,
                    Description = args.description,
                    CategoryId = args.categoryId,
                    Quantity = args.quantity
                };
            });

            AddShoppingItem.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to AddShoppingItem: {ex.Message}");
            }).DisposeWith(Disposables);

            AddShoppingItem.Do(_ => Debug.WriteLine("Adding shopping item"))
                           .InvokeCommand(this, x => x.AddItem)
                           .DisposeWith(Disposables);

            CompleteItem = ReactiveCommand.CreateFromTask<string, ShoppingItem>(async itemId =>
            {
                await Initialize();
                await PerformSync();

                var item = await Table.LookupAsync(itemId);
                item.CompletedOn = DateTime.Now;

                await Table.UpdateAsync(item);

                return item;
            });

            CompleteItem.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to CompleteItem: {ex.Message}");
            }).DisposeWith(Disposables);

            UncompleteItem = ReactiveCommand.CreateFromTask<string, ShoppingItem>(async itemId =>
            {
                await Initialize();
                await PerformSync();

                var item = await Table.LookupAsync(itemId);
                item.CompletedOn = DateTime.MinValue;

                await Table.UpdateAsync(item);

                return item;
            });

            UncompleteItem.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to UncompleteItem: {ex.Message}");
            }).DisposeWith(Disposables);

            Observable.Merge(CompleteItem, UncompleteItem)
                      .Select(_ => Unit.Default)
                      .Do(_ => Debug.WriteLine("Toggling shopping item"))
                      .InvokeCommand(this, x => x.SyncItems)
                      .DisposeWith(Disposables);
        }

		protected override ObservableCollection<ShoppingItem> CachedData
		{
            get => Cache.ShoppingItems;
			set
			{
				Cache.ShoppingItems = value;
			}
		}
	}
}
