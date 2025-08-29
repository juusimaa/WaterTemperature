using WaterTemperature.ViewModels;

namespace WaterTemperature;

public partial class GraphPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public GraphPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Refresh chart data when page appears
        _viewModel.SyncChartData();
    }

    // Method to refresh chart programmatically
    public async Task RefreshChartAsync()
    {
        await _viewModel.RefreshDataAsync();
    }

    // Quick sync method
    public void SyncChart()
    {
        _viewModel.SyncChartData();
    }
}
