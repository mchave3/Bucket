using Bucket.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Bucket.Views;

public sealed partial class WindowsImagePage : Page
{
    public WindowsImageViewModel ViewModel
    {
        get;
    }

    public WindowsImagePage()
    {
        ViewModel = App.GetService<WindowsImageViewModel>();
        InitializeComponent();
    }
}
