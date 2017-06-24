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

namespace shoppinglist.Services
{
    public class ShoppingItemService: AzureService<ShoppingItem>
	{
        protected override IMobileServiceSyncTable<ShoppingItem> Table
        {
            get
            {
                return this.SqlInitializer.ShoppingItemTable;
            }
        }

        public ShoppingItemService(): base("allShoppingItems")
        {
            
        }

		protected override ObservableCollection<ShoppingItem> CachedData
		{
            get => Cache.ShoppingItems;
			set
			{
				Cache.ShoppingItems = value;
			}
		}

		public async Task<IEnumerable<ShoppingItem>> GetShoppingItems()
		{
            var items = await GetItems(x => x.CompletedOn == DateTime.MinValue || x.CompletedOn >= DateTime.Now.AddHours(-24));
            CacheData(items.ToList());

			return items.OrderBy(c => c.Name);
		}

        public async Task<ShoppingItem> AddShoppingItem(string categoryId, string name, string description, double quantity)
		{
            var item = new ShoppingItem
			{
				Name = name,
                Description = description,
                CategoryId = categoryId,
                Quantity = quantity
			};

            return await AddItem(item);
		}

		public async Task<ShoppingItem> CompleteItem(string itemId)
		{
			await Initialize();
            await SyncItems();

            var item = await Table.LookupAsync(itemId);
            item.CompletedOn = DateTime.Now;

            await Table.UpdateAsync(item);

			await SyncItems();
            await CacheData();
			return item;
		}

		public async Task<ShoppingItem> UncompleteItem(string itemId)
		{
			await Initialize();
			await SyncItems();

			var item = await Table.LookupAsync(itemId);
            item.CompletedOn = DateTime.MinValue;

			await Table.UpdateAsync(item);

			await SyncItems();
            await CacheData();
			return item;
		}
	}
}
