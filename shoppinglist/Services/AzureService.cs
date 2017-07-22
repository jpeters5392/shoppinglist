using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Plugin.Connectivity;
using shoppinglist.Cache;
using Splat;

namespace shoppinglist.Services
{
    public abstract class AzureService<T>
    {
        private Subject<bool> _isSyncing;
        public IObservable<bool> IsSyncing { get; }
        protected SqlInitializer SqlInitializer { get; }

        protected DataCache Cache { get; }

        private string AllQueryName { get; }

        public AzureService(string allQueryName)
        {
            Cache = Locator.Current.GetService<DataCache>();
            SqlInitializer = Locator.Current.GetService<SqlInitializer>();
            AllQueryName = allQueryName;
            _isSyncing = new Subject<bool>();
            IsSyncing = _isSyncing.StartWith(false);
        }

        protected abstract IMobileServiceSyncTable<T> Table { get; }

        protected abstract ObservableCollection<T> CachedData { get; set; }

        protected async Task<bool> Initialize()
        {
            return await SqlInitializer.Initialize();
        }

        protected abstract Task CacheData();

		protected void CacheData(IList<T> items)
		{
			CachedData = new ObservableCollection<T>(items);
		}

		protected async Task SyncItems()
		{
			try
			{
                _isSyncing.OnNext(true);
				if (!CrossConnectivity.Current.IsConnected)
					return;

				await Table.PullAsync(AllQueryName, Table.CreateQuery());

				await SqlInitializer.Client.SyncContext.PushAsync();
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to sync, that is alright as we have offline capabilities: " + ex);
			}
            finally
            {
                _isSyncing.OnNext(false);
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
            await CacheData();
			return item;
		}

		public virtual async Task<T> UpdateItem(T item)
		{
			await Initialize();

			await Table.UpdateAsync(item);

			await SyncItems();
            await CacheData();
			return item;
		}

		public virtual async Task DeleteItem(T item)
		{
			await Initialize();

            await Table.DeleteAsync(item);

			await SyncItems();
			await CacheData();
			return;
		}
    }
}
