using Microsoft.UI.Xaml.Media;

namespace Bucket.Views;

public sealed partial class ThemeSettingPage : Page
{
    public ThemeSettingPage()
    {
        this.InitializeComponent();
    }

    private void OnColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        TintBox.Fill = new SolidColorBrush(args.NewColor);
        App.Current.ThemeService.SetBackdropTintColor(args.NewColor);
    }

    private void OnColorPaletteItemClick(object sender, ItemClickEventArgs e)
    {
        var color = e.ClickedItem as ColorPaletteItem;
        if (color != null)
        {
            // Check if this is a "reset" color (typically black or transparent)
            if (color.ColorName?.ToLower() == "black" || color.Color == Windows.UI.Color.FromArgb(255, 0, 0, 0))
            {
                App.Current.ThemeService.ResetBackdropProperties();
            }
            else
            {
                App.Current.ThemeService.SetBackdropTintColor(color.Color);
            }
            TintBox.Fill = new SolidColorBrush(color.Color);
        }
    }
}
