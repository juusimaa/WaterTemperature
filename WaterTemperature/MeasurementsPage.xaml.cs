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
        
        // Add long press gesture for deletion
        var longPressGesture = new TapGestureRecognizer
        {
            NumberOfTapsRequired = 1
        };
        longPressGesture.Tapped += OnDataGridLongPressed;
        dataGrid.GestureRecognizers.Add(longPressGesture);
    }

    private async void OnDataGridCellValueChanged(object sender, DataGridCellValueChangedEventArgs e)
    {
        if (e.RowData is WaterTemperatureMeasurement measurement)
        {
            // Save the updated measurement to the database
            await _databaseService.UpdateMeasurementAsync(measurement);
        }
    }

    private async void OnDataGridLongPressed(object? sender, TappedEventArgs e)
    {
        // Get the selected measurement
        if (dataGrid.SelectedRow is WaterTemperatureMeasurement selectedMeasurement)
        {
            await _viewModel.DeleteMeasurementCommand.ExecuteAsync(selectedMeasurement);
        }
    }
}
