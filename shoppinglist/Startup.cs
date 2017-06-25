using System;
using Microsoft.WindowsAzure.MobileServices;
using shoppinglist.Cache;
using shoppinglist.Services;
using Splat;
namespace shoppinglist
{
    public class Startup
    {
        public static void Initialize()
        {
            Locator.CurrentMutable.RegisterConstant(new MobileServiceClient("https://shopping-list-jpeters5392.azurewebsites.net"), typeof(MobileServiceClient));
            Locator.CurrentMutable.RegisterConstant(new DataCache(), typeof(DataCache));
            Locator.CurrentMutable.RegisterConstant(new SqlInitializer(), typeof(SqlInitializer));
            Locator.CurrentMutable.RegisterConstant(new CategoryService(), typeof(CategoryService));
            Locator.CurrentMutable.RegisterConstant(new MealItemService(), typeof(MealItemService));
            Locator.CurrentMutable.RegisterConstant(new ShoppingItemService(), typeof(ShoppingItemService));
        }
    }
}
