using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Splat;

namespace shoppinglist.Services
{
    public class DataRefresher : IDataRefresher
    {
        private IList<IRefreshableService> Services { get; }

        public DataRefresher()
        {
            Services = new List<IRefreshableService>
            { 
                Locator.Current.GetService<CategoryService>(),
                Locator.Current.GetService<ShoppingItemService>(),
                Locator.Current.GetService<MealItemService>()
            };
        }

        public async Task RefreshAll()
        {
            await Task.WhenAll(Services.Select(x => x.Refresh()));
        }
    }
}
