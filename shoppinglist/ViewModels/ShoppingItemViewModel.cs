using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using shoppinglist.Models;
using shoppinglist.Services;
using Splat;

namespace shoppinglist.ViewModels
{
    public class ShoppingItemViewModel : ReactiveObject, ISupportsActivation
	{
        public ReactiveCommand<Unit, string> ItemSelected { get; }
        public ReactiveCommand<Unit, string> ItemDeleted { get; }

		private string _name;
		public string Name
		{
			get => _name;
			set => this.RaiseAndSetIfChanged(ref _name, value);
		}

		private string _description;
		public string Description
		{
			get => _description;
			set => this.RaiseAndSetIfChanged(ref _description, value);
		}

		private string _id;
		public string Id
		{
			get => _id;
			set => this.RaiseAndSetIfChanged(ref _id, value);
		}

		private double _quantity;
		public double Quantity
		{
			get => _quantity;
			set => this.RaiseAndSetIfChanged(ref _quantity, value);
		}

		private bool _isCompleted;
		public bool IsCompleted
		{
			get => _isCompleted;
			set => this.RaiseAndSetIfChanged(ref _isCompleted, value);
		}

        private ShoppingItemService Service { get; }

        public ViewModelActivator Activator => new ViewModelActivator();

		public ShoppingItemViewModel(ShoppingItem item)
		{
			Name = item.Name;
            Description = item.Description;
            Quantity = item.Quantity;
            IsCompleted = item.CompletedOn != DateTime.MinValue;
            Id = item.Id;

            Service = Locator.Current.GetService<ShoppingItemService>();

            ItemSelected = ReactiveCommand.Create(() =>
            {
                return Id;
            });

			ItemDeleted = ReactiveCommand.Create(() =>
			{
                return item.Id;
			});

            this.WhenActivated(disposables => {
                ItemSelected.Where(_ => IsCompleted).InvokeCommand(this, x => x.Service.UncompleteItem).DisposeWith(disposables);
                ItemSelected.Where(_ => !IsCompleted).InvokeCommand(this, x => x.Service.CompleteItem).DisposeWith(disposables);

                ItemDeleted.InvokeCommand(this, x => x.Service.DeleteItem).DisposeWith(disposables);
            });
		}
	}
}
