using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Plugin.Connectivity;
using ReactiveUI;
using shoppinglist.Cache;
using Splat;

namespace shoppinglist.Services
{
    public enum SyncStatus
    {
        Skipped,
        Failed,
        Synchronized
    }

    public abstract class AzureService<T> : IDisposable
    {
        public IObservable<bool> IsSyncing { get; }
        protected SqlInitializer SqlInitializer { get; }

        protected ReactiveCommand<Unit, SyncStatus> SyncItems { get; }

        protected DataCache Cache { get; }

        private string AllQueryName { get; }

        protected CompositeDisposable Disposables { get; }

        public AzureService(string allQueryName)
        {
            Cache = Locator.Current.GetService<DataCache>();
            SqlInitializer = Locator.Current.GetService<SqlInitializer>();
            AllQueryName = allQueryName;

            SyncItems = ReactiveCommand.CreateFromTask<Unit, SyncStatus>(async (_) =>
            {
                return await PerformSync();
            });

            IsSyncing = SyncItems.IsExecuting.Publish().RefCount();

            CacheCollection = ReactiveCommand.Create<IList<T>, IList<T>>(data =>
            {
                CachedData = new ObservableCollection<T>(data);
                return data;
            });

            AddItem = ReactiveCommand.CreateFromTask<T, T>(async item =>
            {
                await Initialize();

                await Table.InsertAsync(item);

                return item;
            });

            UpdateItem = ReactiveCommand.CreateFromTask<T, T>(async item =>
            {
                await Initialize();

                await Table.UpdateAsync(item);

                return item;
            });

            DeleteItem = ReactiveCommand.CreateFromTask<T, T>(async item =>
            {
                await Initialize();

                await Table.DeleteAsync(item);

                return item;
            });

            Disposables.Add(Observable.Merge(AddItem, UpdateItem, DeleteItem)
                      .Select(_ => Unit.Default)
                            .InvokeCommand(this, x => x.SyncItems));

            Disposables.Add(SyncItems.Where(x => x == SyncStatus.Synchronized).Select(_ => Unit.Default)
                            .InvokeCommand(this, x => x.CacheData));
        }

        public virtual void Dispose()
        {
            Disposables.Dispose();
        }

        protected Task<bool> Initialize()
        {
            return SqlInitializer.Initialize();
        }

        protected abstract IMobileServiceSyncTable<T> Table { get; }

        protected abstract ObservableCollection<T> CachedData { get; set; }

        protected ReactiveCommand<Unit, IEnumerable<T>> CacheData { get; set; }

        protected ReactiveCommand<IList<T>, IList<T>> CacheCollection { get; }

        public ReactiveCommand<T, T> AddItem { get; protected set; }

        public ReactiveCommand<T, T> UpdateItem { get; protected set; }

        public ReactiveCommand<T, T> DeleteItem { get; protected set; }

        protected async Task<SyncStatus> PerformSync()
        {
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                    return SyncStatus.Skipped;

                await Table.PullAsync(AllQueryName, Table.CreateQuery());

                await SqlInitializer.Client.SyncContext.PushAsync();

                return SyncStatus.Synchronized;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to sync, that is alright as we have offline capabilities: " + ex);
                return SyncStatus.Failed;
            }
        }
    }
}
