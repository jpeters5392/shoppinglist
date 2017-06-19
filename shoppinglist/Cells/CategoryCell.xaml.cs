using System;
using System.Collections.Generic;
using ReactiveUI;
using ReactiveUI.XamForms;
using shoppinglist.ViewModels;
using Xamarin.Forms;

namespace shoppinglist.Cells
{
    public partial class CategoryCell : ReactiveViewCell<CategoryViewModel>
    {
        public CategoryCell()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                disposables(this.OneWayBind(
                    this.ViewModel,
                    vm => vm.Name,
                    v => v.CategoryName.Text));
            });
        }
    }
}
