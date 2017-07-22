using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using shoppinglist.Cache;
using shoppinglist.Models;
using shoppinglist.Services;
using Splat;

namespace shoppinglist.ViewModels
{
    public class MealPlannerViewModel : ReactiveObject
    {
        public ReactiveCommand Refresh { get; }

        public ReactiveCommand OpenAddMealItemForm { get; }
        public ReactiveCommand CloseAddMealItemForm { get; }
        public ReactiveCommand AddMealItem { get; }

        private DateTimeOffset _todayDate;

		private readonly ObservableAsPropertyHelper<ObservableCollection<MealItemGroupViewModel>> _mealItemGroups;
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

		private bool _isLoadingData;
		public bool IsLoadingData
		{
			get => _isLoadingData;
			set => this.RaiseAndSetIfChanged(ref _isLoadingData, value);
		}

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
            }).ToProperty(this, x => x.MealItemGroups);

			Refresh = ReactiveCommand.CreateFromTask(async () =>
			{
                await MealItemService.GetMealItems();
			});

			_isRefreshing = Observable.CombineLatest(Refresh.IsExecuting, MealItemService.IsSyncing)
									  .Select(vals => vals.Any(x => x))
									  .ToProperty(this, x => x.IsRefreshing);

			OpenAddMealItemForm = ReactiveCommand.Create(() =>
			{
				ShouldShowGrid = true;
			});

			CloseAddMealItemForm = ReactiveCommand.Create(() =>
			{
				ShouldShowGrid = false;
			});

			AddMealItem = ReactiveCommand.CreateFromTask(async () =>
			{
				ShouldShowGrid = false;

                var mealItemType = (MealType)Enum.Parse(typeof(MealType), MealTypes[NewItemSelectedMealType]);

                var newItem = await MealItemService.AddMealItem(
					NewItemName,
					NewItemDate,
					mealItemType
				);

				NewItemName = string.Empty;
                NewItemDate = DateTime.Now;
				NewItemSelectedMealType = 2;
			});

			AddMealItem.IsExecuting.Subscribe(isExecuting =>
			{
				IsLoadingData = isExecuting;
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
