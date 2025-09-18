using Bucket.Core.Helpers;

namespace Bucket.App.Views
{
    public sealed partial class AboutUsSettingPage : Page
    {
        public AboutUsSettingViewModel ViewModel { get; }

        public AboutUsSettingPage()
        {
            ViewModel = App.GetService<AboutUsSettingViewModel>();
            this.InitializeComponent();
        }

        public string AppVersion => VersionHelper.GetAppVersion();
    }

}
