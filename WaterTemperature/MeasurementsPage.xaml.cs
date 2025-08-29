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
                // Only save to database if both temperature and date have valid values
                if (measurement.IsValid)
                {
                    if (measurement.Id == 0)
                    {
                        // New measurement - insert into database
                        await _databaseService.SaveMeasurementAsync(measurement);
                    }
                    else
                    {
                        // Existing measurement - update in database
                        await _databaseService.UpdateMeasurementAsync(measurement);
                    }
                    
                    // Refresh the data to update both grid and chart
                    await _viewModel.RefreshDataAsync();
                }
                else
                {
                    // If measurement becomes invalid, remove from chart data but keep in grid
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        var chartItem = _viewModel.ChartData.FirstOrDefault(c => c.Id == measurement.Id);
                        if (chartItem != null)
                        {
                            _viewModel.ChartData.Remove(chartItem);
                        }
                    });
                }
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
            if (selectedMeasurement.Id > 0)
            {
                // Measurement is in database, use the ViewModel's delete command
                await _viewModel.DeleteMeasurementCommand.ExecuteAsync(selectedMeasurement);
            }
            else
            {
                // Measurement is only in memory (not saved to database), just remove from collection
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _viewModel.Measurements.Remove(selectedMeasurement);
                });
            }
        }
        else
        {
            await Shell.Current.DisplayAlert("No Selection", "Please select a row to delete.", "OK");
        }
    }
}
