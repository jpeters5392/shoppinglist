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
    public class MealItemService : AzureService<MealItem>, IRefreshableService
	{
        public ReactiveCommand<Unit, long> Refresh { get; }
        public IObservable<IEnumerable<MealItem>> MealItems { get; }
        public ReactiveCommand<(string name, DateTimeOffset date, MealType type), MealItem> AddMealItem { get; }

		protected override IMobileServiceSyncTable<MealItem> Table
		{
			get
			{
				return this.SqlInitializer.MealItemTable;
			}
		}

		public MealItemService() : base("allMealItems")
		{
            CacheData = ReactiveCommand.CreateFromTask<Unit, IEnumerable<MealItem>>(async (_) =>
            {
                return await Table.Where(x => x.Date >= DateTimeOffset.Now.LocalDateTime.Date).ToListAsync();
            });

            CacheData.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to CacheData: {ex.Message}");
            }).DisposeWith(Disposables);

            InitCommands();

            CacheData.Do(_ => Debug.WriteLine("Caching meal items"))
                     .InvokeCommand(this, x => x.CacheCollection)
                     .DisposeWith(Disposables);

            MealItems = CacheCollection.Select(items => items.Where(x => x.Date >= DateTimeOffset.Now.LocalDateTime.Date).OrderBy(x => x.Name))
                                           .Publish()
                                           .RefCount();

            Refresh = ReactiveCommand.Create<Unit, long>(_ => DateTime.Now.Ticks);

            Refresh.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to Refresh: {ex.Message}");
            }).DisposeWith(Disposables);

            Refresh.Select(_ => Unit.Default)
                   .Do(_ => Debug.WriteLine("Syncing meal items"))
                   .InvokeCommand(this, x => x.SyncItems)
                   .DisposeWith(Disposables);

            AddMealItem = ReactiveCommand.Create<(string name, DateTimeOffset date, MealType type), MealItem>((args) =>
            {
                return new MealItem
                {
                    Name = args.name,
                    Date = args.date,
                    Type = args.type
                };
            });

            AddMealItem.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Failed to AddMealItem: {ex.Message}");
            }).DisposeWith(Disposables);

            AddMealItem.Do(_ => Debug.WriteLine("Adding meal item"))
                       .InvokeCommand(this, x => x.AddItem)
                       .DisposeWith(Disposables);
		}

		protected override ObservableCollection<MealItem> CachedData
		{
			get => Cache.MealItems;
			set
			{
                Cache.MealItems = value;
			}
		}
	}
}
