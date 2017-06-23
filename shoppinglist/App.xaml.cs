using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using shoppinglist.Models;
using shoppinglist.Services;
using Splat;
using Xamarin.Forms;

namespace shoppinglist
{
    public partial class App : Application
    {
        public static IObservable<IEnumerable<Category>> CategoryRefresh { get; private set; }
        public static IObservable<IEnumerable<ShoppingItem>> ShoppingListRefresh { get; private set; }

        public App()
        {
            InitializeComponent();

            Startup.Initialize();

			var categoryService = Locator.Current.GetService<CategoryService>();
			var itemsService = Locator.Current.GetService<ShoppingItemService>();
            CategoryRefresh = Observable.FromAsync(categoryService.GetCategories);
            ShoppingListRefresh = Observable.FromAsync(itemsService.GetShoppingItems);

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            var categoryService = Locator.Current.GetService<CategoryService>();
            var itemsService = Locator.Current.GetService<ShoppingItemService>();
            CategoryRefresh.Subscribe();
            ShoppingListRefresh.Subscribe();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
			// Handle when your app resumes
			var categoryService = Locator.Current.GetService<CategoryService>();
			var itemsService = Locator.Current.GetService<ShoppingItemService>();
			CategoryRefresh.Subscribe();
			ShoppingListRefresh.Subscribe();
        }
    }
}
