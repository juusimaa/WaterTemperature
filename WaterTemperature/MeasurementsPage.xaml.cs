using WaterTemperature.ViewModels;
using Syncfusion.Maui.DataGrid;
using WaterTemperature.Models;
using WaterTemperature.Services;

namespace WaterTemperature;

public partial class MeasurementsPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private readonly DatabaseService _databaseService;

    public MeasurementsPage(MainViewModel viewModel, DatabaseService databaseService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _databaseService = databaseService;
        BindingContext = viewModel;
    }

    private async void OnDataGridCellValueChanged(object sender, DataGridCellValueChangedEventArgs e)
    {
        if (e.RowData is WaterTemperatureMeasurement measurement)
        {
            try
            {
                // Save the updated measurement to the database
                await _databaseService.UpdateMeasurementAsync(measurement);
                
                // Refresh the chart data by reloading all measurements
                await _viewModel.RefreshDataAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save changes: {ex.Message}", "OK");
            }
        }
    }

    private async void OnDeleteSelectedClicked(object sender, EventArgs e)
    {
        if (dataGrid.SelectedRow is WaterTemperatureMeasurement selectedMeasurement)
        {
            await _viewModel.DeleteMeasurementCommand.ExecuteAsync(selectedMeasurement);
        }
        else
        {
            await Shell.Current.DisplayAlert("No Selection", "Please select a row to delete.", "OK");
        }
    }
}
