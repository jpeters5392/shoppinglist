﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using shoppinglist.Cache;
using shoppinglist.Models;
using shoppinglist.Services;
using Splat;
using System.Reactive.Disposables;
using System.Reactive;

namespace shoppinglist.ViewModels
{
    public class MealPlannerViewModel : ReactiveObject, ISupportsActivation
    {
        public ReactiveCommand<Unit, long> Refresh { get; }

        public ReactiveCommand OpenAddMealItemForm { get; }
        public ReactiveCommand CloseAddMealItemForm { get; }
        public ReactiveCommand<Unit, (string, DateTimeOffset, MealType)> AddMealItem { get; }

        private DateTimeOffset _todayDate;

		private ObservableAsPropertyHelper<ObservableCollection<MealItemGroupViewModel>> _mealItemGroups;
		public ObservableCollection<MealItemGroupViewModel> MealItemGroups => _mealItemGroups.Value;

        private MealItemService MealItemService { get; }
		private DataCache Cache { get; }

		private ObservableAsPropertyHelper<bool> _isRefreshing;
		public bool IsRefreshing => _isRefreshing.Value;

		private ObservableCollection<string> _mealTypes;
		public ObservableCollection<string> MealTypes
		{
			get => _mealTypes;
		}

		private bool _shouldShowGrid;
		public bool ShouldShowGrid
		{
			get => _shouldShowGrid;
			set => this.RaiseAndSetIfChanged(ref _shouldShowGrid, value);
		}

        private ObservableAsPropertyHelper<bool> _isLoadingData;
        public bool IsLoadingData => _isLoadingData.Value;

		private int _newItemSelectedMealType;
		public int NewItemSelectedMealType
		{
			get => _newItemSelectedMealType;
			set => this.RaiseAndSetIfChanged(ref _newItemSelectedMealType, value);
		}

		private string _newItemName;
		public string NewItemName
		{
			get => _newItemName;
			set => this.RaiseAndSetIfChanged(ref _newItemName, value);
		}

		private DateTime _newItemDate;
		public DateTime NewItemDate
		{
			get => _newItemDate;
			set => this.RaiseAndSetIfChanged(ref _newItemDate, value);
		}

        private readonly ViewModelActivator _viewModelActivator = new ViewModelActivator();

        public ViewModelActivator Activator
        {
            get { return _viewModelActivator; }
        }

        public MealPlannerViewModel()
        {
            MealItemService = Locator.Current.GetService<MealItemService>();
            Cache = Locator.Current.GetService<DataCache>();

            _todayDate = DateTimeOffset.Now;
            ShouldShowGrid = false;
            _mealTypes = new ObservableCollection<string>(new List<string> { "Breakfast", "Lunch", "Dinner" });

            // default to dinner
            NewItemSelectedMealType = 2;
            NewItemDate = DateTime.Now;

            this.WhenActivated(disposables => {
                _mealItemGroups = this.WhenAnyValue(x => x.Cache.MealItems)
                                  .Select(mealItems =>
                                  {
                                      var todayItemGroup = BuildMealItemGroup("Today", mealItems.Where(x => x.Date.Date == DateTime.Now.Date));
                                      var todayPlusOneItemGroup = BuildMealItemGroup(FormatDate(_todayDate.AddDays(1).ToLocalTime()), mealItems.Where(x => x.Date.Date == DateTime.Now.AddDays(1).Date));
                                      var todayPlusTwoItemGroup = BuildMealItemGroup(FormatDate(_todayDate.AddDays(2).ToLocalTime()), mealItems.Where(x => x.Date.Date == DateTime.Now.AddDays(2).Date));
                                      var todayPlusThreeItemGroup = BuildMealItemGroup(FormatDate(_todayDate.AddDays(3).ToLocalTime()), mealItems.Where(x => x.Date.Date == DateTime.Now.AddDays(3).Date));
                                      var todayPlusFourItemGroup = BuildMealItemGroup(FormatDate(_todayDate.AddDays(4).ToLocalTime()), mealItems.Where(x => x.Date.Date == DateTime.Now.AddDays(4).Date));
                                      var todayPlusFiveItemGroup = BuildMealItemGroup(FormatDate(_todayDate.AddDays(5).ToLocalTime()), mealItems.Where(x => x.Date.Date == DateTime.Now.AddDays(5).Date));
                                      var todayPlusSixItemGroup = BuildMealItemGroup(FormatDate(_todayDate.AddDays(6).ToLocalTime()), mealItems.Where(x => x.Date.Date == DateTime.Now.AddDays(6).Date));

                                      var groups = new List<MealItemGroupViewModel>();
                                      groups.Add(todayItemGroup);
                                      groups.Add(todayPlusOneItemGroup);
                                      groups.Add(todayPlusTwoItemGroup);
                                      groups.Add(todayPlusThreeItemGroup);
                                      groups.Add(todayPlusFourItemGroup);
                                      groups.Add(todayPlusFiveItemGroup);
                                      groups.Add(todayPlusSixItemGroup);

                                      return new ObservableCollection<MealItemGroupViewModel>(groups);
                                  })
                                      .ToProperty(this, x => x.MealItemGroups)
                                      .DisposeWith(disposables);

                Refresh.Select(_ => Unit.Default).InvokeCommand(this, x => x.MealItemService.Refresh).DisposeWith(disposables);

                _isRefreshing = Observable.CombineLatest(Refresh.IsExecuting, MealItemService.IsSyncing)
                                      .Select(vals => vals.Any(x => x))
                                      .ToProperty(this, x => x.IsRefreshing)
                                          .DisposeWith(disposables);

                _isLoadingData = MealItemService.IsSyncing.ToProperty(this, x => x.IsLoadingData).DisposeWith(disposables);

                AddMealItem.Where(x => !string.IsNullOrWhiteSpace(x.Item1))
                           .Select(x => x)
                           .InvokeCommand(this, x => x.MealItemService.AddMealItem)
                           .DisposeWith(disposables);

                AddMealItem.Where(x => string.IsNullOrWhiteSpace(x.Item1))
                           .Subscribe(async _ => {
                               await App.Instance.MainPage.DisplayAlert("Error", "You must enter a name", "OK");
                           })
                           .DisposeWith(disposables);
            });

			Refresh = ReactiveCommand.Create<Unit, long>((_) =>
			{
                return DateTime.Now.Ticks;
			});

			OpenAddMealItemForm = ReactiveCommand.Create(() =>
			{
				ShouldShowGrid = true;
			});

			CloseAddMealItemForm = ReactiveCommand.Create(() =>
			{
				ShouldShowGrid = false;
			});

			AddMealItem = ReactiveCommand.Create<Unit, (string, DateTimeOffset, MealType)>((_) =>
			{
				ShouldShowGrid = false;

                var mealItemType = (MealType)Enum.Parse(typeof(MealType), MealTypes[NewItemSelectedMealType]);

                var newMealItem = (NewItemName, new DateTimeOffset(NewItemDate), mealItemType);

                NewItemName = string.Empty;
                NewItemDate = DateTime.Now;
                NewItemSelectedMealType = 2;

                return newMealItem;
			});
        }

        private MealItemGroupViewModel BuildMealItemGroup(string title, IEnumerable<MealItem> items)
        {
            var group = new MealItemGroupViewModel(title, title);
            var itemViewModel = new MealItemViewModel();
            itemViewModel.BreakfastMealItems = new ObservableCollection<MealItem>(items.Where(x => x.Type == MealType.Breakfast));
            itemViewModel.DinnerMealItems = new ObservableCollection<MealItem>(items.Where(x => x.Type == MealType.Dinner));
            itemViewModel.LunchMealItems = new ObservableCollection<MealItem>(items.Where(x => x.Type == MealType.Lunch));
            group.Add(itemViewModel);
            return group;
        }

        private string FormatDate(DateTimeOffset date)
        {
            return date.ToString("MM/dd/yyyy");
        }
    }
}
