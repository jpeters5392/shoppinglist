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
    public class MealItemService : AzureService<MealItem>, IRefreshableService
	{
		protected override IMobileServiceSyncTable<MealItem> Table
		{
			get
			{
				return this.SqlInitializer.MealItemTable;
			}
		}

		public MealItemService() : base("allMealItems")
		{

		}

		protected override ObservableCollection<MealItem> CachedData
		{
			get => Cache.MealItems;
			set
			{
                Cache.MealItems = value;
			}
		}

		protected override async Task CacheData()
		{
            CacheData(await Table.Where(x => x.Date >= DateTimeOffset.Now.LocalDateTime.Date).ToListAsync());
		}

		public async Task Refresh()
		{
			await GetMealItems();
		}

		public async Task<IEnumerable<MealItem>> GetMealItems()
		{
            var items = await GetItems(x => x.Date >= DateTimeOffset.Now.LocalDateTime.Date);
			CacheData(items.ToList());
			return items.OrderBy(c => c.Name);
		}

		public async Task<MealItem> AddMealItem(string name, DateTimeOffset date, MealType type)
		{
			var item = new MealItem
			{
				Name = name,
                Date = date,
                Type = type
			};

			return await AddItem(item);
		}
	}
}
