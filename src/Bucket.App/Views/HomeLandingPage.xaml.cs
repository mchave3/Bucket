using System.Reflection;
using Bucket.Core.Helpers;

namespace Bucket.App.Views
{
    public sealed partial class HomeLandingPage : Page
    {
        public HomeLandingPage()
        {
            this.InitializeComponent();
        }

        public string AppVersion => VersionHelper.GetAppVersion();
    }

}
