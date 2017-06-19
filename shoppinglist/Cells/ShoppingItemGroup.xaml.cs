using System;
using System.Collections.Generic;
using ReactiveUI;
using ReactiveUI.XamForms;
using shoppinglist.ViewModels;
using Xamarin.Forms;

namespace shoppinglist.Cells
{
    public partial class ShoppingItemGroup : ReactiveViewCell<ShoppingItemGroupViewModel>
    {
        public ShoppingItemGroup()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                disposables(this.OneWayBind(
                    this.ViewModel,
                    vm => vm.Title,
                    v => v.GroupName.Text));
            });
        }
    }
}
