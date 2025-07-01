using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Bucket.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using Bucket.Models;
using WinUI.TableView;

namespace Bucket.Views;

/// <summary>
/// Microsoft Update Catalog page for searching and downloading Windows updates
/// </summary>
public sealed partial class MicrosoftUpdateCatalogPage : Page
{
    /// <summary>
    /// Gets the view model for this page
    /// </summary>
    public MicrosoftUpdateCatalogViewModel ViewModel { get; }

    /// <summary>
    /// Initializes a new instance of the MicrosoftUpdateCatalogPage class
    /// </summary>
    public MicrosoftUpdateCatalogPage()
    {
        ViewModel = App.GetService<MicrosoftUpdateCatalogViewModel>();
        InitializeComponent();
        
        // Set up TableView selection changed handler
        ResultsTableView.SelectionChanged += OnTableViewSelectionChanged;
        
        Logger.Information("MicrosoftUpdateCatalogPage initialized");
    }

    /// <summary>
    /// Called when the page is navigated to
    /// </summary>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Logger.Information("Navigated to MicrosoftUpdateCatalogPage");
    }

    /// <summary>
    /// Called when the page is navigated away from
    /// </summary>
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        Logger.Information("Navigated from MicrosoftUpdateCatalogPage");
    }

    /// <summary>
    /// Handles TableView selection changes to update the ViewModel's selected items
    /// </summary>
    private void OnTableViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is TableView tableView)
        {
            // Clear the ViewModel's selected updates collection
            ViewModel.SelectedUpdates.Clear();

            // Add all selected items to the ViewModel's collection
            foreach (var item in tableView.SelectedItems)
            {
                if (item is MSCatalogUpdate update)
                {
                    update.IsSelected = true;
                    if (!ViewModel.SelectedUpdates.Contains(update))
                    {
                        ViewModel.SelectedUpdates.Add(update);
                    }
                }
            }

            // Update IsSelected for unselected items
            foreach (var update in ViewModel.Updates)
            {
                if (!tableView.SelectedItems.Contains(update))
                {
                    update.IsSelected = false;
                }
            }
        }
    }
}