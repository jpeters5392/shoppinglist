using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Splat;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Disposables;

namespace shoppinglist.Services
{
    public class DataRefresher : IDataRefresher
    {
        private CategoryService CategoryService { get; }
        private ShoppingItemService ShoppingItemService { get; }
        private MealItemService MealItemService { get; }

        public ReactiveCommand<Unit, long> RefreshAll { get; }

        private CompositeDisposable Disposables { get; }

        public DataRefresher()
        {
            CategoryService = Locator.Current.GetService<CategoryService>();
            ShoppingItemService = Locator.Current.GetService<ShoppingItemService>();
            MealItemService = Locator.Current.GetService<MealItemService>();

            RefreshAll = ReactiveCommand.Create<Unit, long>((_) =>
            {
                return DateTime.Now.Ticks;
            });

            Disposables.Add(RefreshAll.Select(_ => Unit.Default).InvokeCommand(this, x => x.CategoryService.Refresh));
            Disposables.Add(RefreshAll.Select(_ => Unit.Default).InvokeCommand(this, x => x.ShoppingItemService.Refresh));
            Disposables.Add(RefreshAll.Select(_ => Unit.Default).InvokeCommand(this, x => x.MealItemService.Refresh));
        }

        public void Dispose()
        {
            Disposables.Dispose();
        }
    }
}
