﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Plugin.Connectivity;
using Splat;

namespace shoppinglist.Services
{
    public abstract class AzureService<T>
    {
        protected SqlInitializer SqlInitializer { get; }

        private string AllQueryName { get; }

        public AzureService(string allQueryName)
        {
            SqlInitializer = Locator.Current.GetService<SqlInitializer>();
            AllQueryName = allQueryName;
        }

        protected abstract IMobileServiceSyncTable<T> Table { get; }

        protected async Task<bool> Initialize()
        {
            return await SqlInitializer.Initialize();
        }

		protected async Task SyncItems()
		{
			try
			{
				if (!CrossConnectivity.Current.IsConnected)
					return;

				await Table.PullAsync(AllQueryName, Table.CreateQuery());

				await SqlInitializer.Client.SyncContext.PushAsync();
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to sync, that is alright as we have offline capabilities: " + ex);
			}
		}

		public virtual async Task<IEnumerable<T>> GetItems(Expression<Func<T, bool>> clause)
		{
			//Initialize & Sync
			await Initialize();
			await SyncItems();

			return await Table.Where(clause).ToEnumerableAsync();
		}

		public virtual async Task<T> AddItem(T item)
		{
			await Initialize();

			await Table.InsertAsync(item);

			await SyncItems();
			return item;
		}

		public virtual async Task<T> UpdateItem(T item)
		{
			await Initialize();

			await Table.UpdateAsync(item);

			await SyncItems();
			return item;
		}
    }
}