using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: Xamarin.Forms.ExportRenderer(typeof(shoppinglist.Controls.Fab), typeof(shoppinglist.Droid.Renderers.FabRenderer))]
namespace shoppinglist.Droid.Renderers
{
    public class FabRenderer : Xamarin.Forms.Platform.Android.ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null && e.NewElement != null)
            {
                this.Control.Elevation = 4;
            }
        }
    }
}
