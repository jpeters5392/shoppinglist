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
using System.Linq.Expressions;

namespace shoppinglist.Services
{
    public class SqlInitializer
    {
		public MobileServiceClient Client { get; set; } = null;

        public IMobileServiceSyncTable<Category> CategoryTable
        {
            get; private set;
        }

		public IMobileServiceSyncTable<MealItem> MealItemTable
		{
			get; private set;
		}

        public IMobileServiceSyncTable<ShoppingItem> ShoppingItemTable
        {
            get; private set;
        }

        private static TaskCompletionSource<bool> initializerTask = null;

        public SqlInitializer()
        {
        }

		public async Task<bool> Initialize()
		{
            if (initializerTask != null) return await initializerTask.Task;

            if (initializerTask == null)
            {
                initializerTask = new TaskCompletionSource<bool>();
            }

			if (Client?.SyncContext?.IsInitialized ?? false)
                return await initializerTask.Task;

            Client = Locator.Current.GetService<MobileServiceClient>();

			//InitializeDatabase for path
            var path = "syncstore.db";
			path = Path.Combine(MobileServiceClient.DefaultDatabasePath, path);

			//setup our local sqlite store and intialize our table
			var store = new MobileServiceSQLiteStore(path);

			//Define table
			store.DefineTable<Category>();
            store.DefineTable<ShoppingItem>();
            store.DefineTable<MealItem>();

			//Initialize SyncContext
			await Client.SyncContext.InitializeAsync(store);

			//Get our sync table that will call out to azure
			CategoryTable = Client.GetSyncTable<Category>();
            ShoppingItemTable = Client.GetSyncTable<ShoppingItem>();
            MealItemTable = Client.GetSyncTable<MealItem>();

            initializerTask.SetResult(true);
            return initializerTask.Task.Result;
		}
	}
}
