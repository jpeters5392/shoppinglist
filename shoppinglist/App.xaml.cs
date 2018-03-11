using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using shoppinglist.Models;
using shoppinglist.Services;
using Splat;
using Xamarin.Forms;

namespace shoppinglist
{
    public partial class App : Application
    {
        private IDataRefresher DataRefresher { get; }

        public static App Instance { get; private set; }

        public App()
        {
            InitializeComponent();

            Instance = this;

            Startup.Initialize();

            DataRefresher = Locator.Current.GetService<IDataRefresher>();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            Observable.Return(Unit.Default).InvokeCommand(this, x => x.DataRefresher.RefreshAll);
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
			// Handle when your app resumes
            Observable.Return(Unit.Default).InvokeCommand(this, x => x.DataRefresher.RefreshAll);
        }
    }
}
