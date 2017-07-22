using System;
[assembly: Xamarin.Forms.ExportRenderer(typeof(shoppinglist.Controls.Fab), typeof(shoppinglist.Droid.Renderers.FabRenderer))]
namespace shoppinglist.Droid.Renderers
{
    public class FabRenderer : Xamarin.Forms.Platform.Android.ButtonRenderer
    {
    }
}
