namespace App11.Views;

public sealed partial class HomeLandingPage : Page
{
    public HomeLandingPage()
    {
        this.InitializeComponent();
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = Cmb.SelectedItem as ComboBoxItem;
        switch (item.Tag.ToString())
        {
            case "English":
                Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
                App.Current.NavService.Frame.Navigate(typeof(HomeLandingPage));
                App.Current.NavService.Frame.BackStack.Remove(App.Current.NavService.Frame.BackStack.LastOrDefault());

                DataSource.Instance.Groups.Clear();
                App.Current.NavService.Reset();
                MainWindow.Instance.ReInitialize();
                break;
            case "Persian":
                Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "fa-IR";
                App.Current.NavService.Frame.Navigate(typeof(HomeLandingPage));
                App.Current.NavService.Frame.BackStack.Remove(App.Current.NavService.Frame.BackStack.LastOrDefault());

                DataSource.Instance.Groups.Clear();
                App.Current.NavService.Reset();
                MainWindow.Instance.ReInitialize();
                break;
        }
    }
}

