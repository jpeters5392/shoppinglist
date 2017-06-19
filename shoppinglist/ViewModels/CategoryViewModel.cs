using System;
using ReactiveUI;
using shoppinglist.Models;

namespace shoppinglist.ViewModels
{
    public class CategoryViewModel : ReactiveObject
    {
		private string _name;
		public string Name
		{
			get => _name;
			set => this.RaiseAndSetIfChanged(ref _name, value);
		}

        public CategoryViewModel(Category category)
        {
            Name = category.Name;
        }
    }
}
