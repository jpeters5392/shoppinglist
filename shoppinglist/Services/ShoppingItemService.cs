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

            Disposables.Add(CacheData.InvokeCommand(this, x => x.CacheCollection));

            ShoppingItems = CacheCollection.Select(items => items.Where(x => x.CompletedOn == DateTime.MinValue || x.CompletedOn >= DateTime.Now.AddHours(-24)).OrderBy(c => c.Name))
                                           .Publish()
                                           .RefCount();

            Disposables.Add(ShoppingItems.InvokeCommand(this, x => x.CacheCollection));

            Refresh = ReactiveCommand.Create<Unit, long>(_ => DateTime.Now.Ticks);

            Disposables.Add(Refresh.Select(_ => Unit.Default).InvokeCommand(this, x => x.SyncItems));

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

            Disposables.Add(AddShoppingItem.InvokeCommand(this, x => x.AddItem));

            CompleteItem = ReactiveCommand.CreateFromTask<string, ShoppingItem>(async itemId =>
            {
                await Initialize();
                await PerformSync();

                var item = await Table.LookupAsync(itemId);
                item.CompletedOn = DateTime.Now;

                await Table.UpdateAsync(item);

                return item;
            });

            UncompleteItem = ReactiveCommand.CreateFromTask<string, ShoppingItem>(async itemId =>
            {
                await Initialize();
                await PerformSync();

                var item = await Table.LookupAsync(itemId);
                item.CompletedOn = DateTime.MinValue;

                await Table.UpdateAsync(item);

                return item;
            });

            Disposables.Add(Observable.Merge(CompleteItem, UncompleteItem)
                      .Select(_ => Unit.Default)
                            .InvokeCommand(this, x => x.SyncItems));
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
