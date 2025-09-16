using Bucket.App.ViewModels;

namespace Bucket.App.Tests.ViewModels;

public class MainViewModelTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        var viewModel = new MainViewModel();

        Assert.NotNull(viewModel);
    }

    [Fact]
    public void MainViewModel_ShouldInheritFromObservableObject()
    {
        var viewModel = new MainViewModel();

        Assert.IsAssignableFrom<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>(viewModel);
    }
}