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
        public IObservable<bool> IsSyncing { get; set; }
        protected SqlInitializer SqlInitializer { get; }

        protected ReactiveCommand<Unit, SyncStatus> SyncItems { get; set; }

        protected DataCache Cache { get; }

        private string AllQueryName { get; }

        protected CompositeDisposable Disposables { get; } = new CompositeDisposable();

        protected abstract IMobileServiceSyncTable<T> Table { get; }

        protected abstract ObservableCollection<T> CachedData { get; set; }

        protected ReactiveCommand<Unit, IEnumerable<T>> CacheData { get; set; }

        protected ReactiveCommand<IList<T>, IList<T>> CacheCollection { get; set; }

        public ReactiveCommand<T, T> AddItem { get; protected set; }

        public ReactiveCommand<T, T> UpdateItem { get; protected set; }

        public ReactiveCommand<T, T> DeleteItem { get; protected set; }

        public AzureService(string allQueryName)
        {
            Cache = Locator.Current.GetService<DataCache>();
            SqlInitializer = Locator.Current.GetService<SqlInitializer>();
            AllQueryName = allQueryName;
        }

        protected void InitCommands()
        {
            SyncItems = ReactiveCommand.CreateFromTask<Unit, SyncStatus>(async (_) =>
            {
                return await PerformSync();
            });

            SyncItems.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to SyncItems: {ex.Message}");
            }).DisposeWith(Disposables);

            IsSyncing = SyncItems.IsExecuting.StartWith(false).Publish().RefCount();

            CacheCollection = ReactiveCommand.Create<IList<T>, IList<T>>(data =>
            {
                CachedData = new ObservableCollection<T>(data);
                return data;
            });

            CacheCollection.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to CacheCollection: {ex.Message}");
            }).DisposeWith(Disposables);

            AddItem = ReactiveCommand.CreateFromTask<T, T>(async item =>
            {
                await Initialize();

                await Table.InsertAsync(item);

                return item;
            });

            AddItem.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to AddItem: {ex.Message}");
            }).DisposeWith(Disposables);

            UpdateItem = ReactiveCommand.CreateFromTask<T, T>(async item =>
            {
                await Initialize();

                await Table.UpdateAsync(item);

                return item;
            });

            UpdateItem.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to UpdateItem: {ex.Message}");
            }).DisposeWith(Disposables);

            DeleteItem = ReactiveCommand.CreateFromTask<T, T>(async item =>
            {
                await Initialize();

                await Table.DeleteAsync(item);

                return item;
            });

            DeleteItem.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to DeleteItem: {ex.Message}");
            }).DisposeWith(Disposables);

            Observable.Merge(AddItem, UpdateItem, DeleteItem)
                      .Select(_ => Unit.Default)
                      .Do(_ => Debug.WriteLine("Syncing after changing items"))
                      .InvokeCommand(this, x => x.SyncItems)
                      .DisposeWith(Disposables);

            SyncItems.Where(x => x == SyncStatus.Synchronized)
                     .Select(_ => Unit.Default)
                     .Do(_ => Debug.WriteLine("Caching sync'd items"))
                     .InvokeCommand(this, x => x.CacheData)
                     .DisposeWith(Disposables);
        }

        public virtual void Dispose()
        {
            Debug.WriteLine("Disposing");
            Disposables.Dispose();
        }

        protected Task<bool> Initialize()
        {
            return SqlInitializer.Initialize();
        }

        protected async Task<SyncStatus> PerformSync()
        {
            try
            {
                if (Table == null)
                {
                    await Initialize();    
                }

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
