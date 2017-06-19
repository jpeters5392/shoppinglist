using System;
using shoppinglist.ViewModels;
using Xamarin.Forms;

namespace shoppinglist
{
    public class MainPage : TabbedPage
    {
        public MainPage()
        {
			var categories = new CategoriesPage();
			categories.Icon = "ic_toc.png";
			categories.Title = "Categories";
            var categoriesViewModel = new CategoriesViewModel();
            categories.ViewModel = categoriesViewModel;

            var shoppingCart = new ShoppingCart();
			shoppingCart.Icon = "ic_shopping_cart.png";
			shoppingCart.Title = "Shopping List";
            var shoppingCartViewModel = new ShoppingCartViewModel();
            shoppingCart.ViewModel = shoppingCartViewModel;

			Children.Add(shoppingCart);
            Children.Add(categories);
        }
    }
}

