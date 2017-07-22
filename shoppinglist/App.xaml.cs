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
        private IDataRefresher DataRefresher { get; }

        public App()
        {
            InitializeComponent();

            Startup.Initialize();

            DataRefresher = Locator.Current.GetService<IDataRefresher>();

            MainPage = new MainPage();
        }

        protected override async void OnStart()
        {
            // Handle when your app starts
            try
            {
                await DataRefresher.RefreshAll();
            }
            catch(Exception)
            {
                
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override async void OnResume()
        {
			// Handle when your app resumes
			try
			{
				await DataRefresher.RefreshAll();
			}
			catch (Exception)
			{

			}
        }
    }
}
