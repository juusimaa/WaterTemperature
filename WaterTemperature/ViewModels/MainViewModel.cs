using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WaterTemperature.Models;
using WaterTemperature.Services;

namespace WaterTemperature.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private double temperature;

        [ObservableProperty]
        private DateTime measurementDate = DateTime.Now;

        [ObservableProperty]
        private TimeSpan measurementTime;

        public ObservableCollection<WaterTemperatureMeasurement> Measurements { get; } = [];

        // Chart data for Syncfusion
        public ObservableCollection<WaterTemperatureMeasurement> ChartData { get; } = [];

        partial void OnMeasurementTimeChanged(TimeSpan value)
        {
            // When time changes, update the date part but avoid circular updates
            var newDateTime = MeasurementDate.Date.Add(value);
            if (newDateTime != MeasurementDate)
            {
                SetProperty(ref measurementDate, newDateTime, nameof(MeasurementDate));
            }
        }

        partial void OnMeasurementDateChanged(DateTime value)
        {
            // When date changes, update the time to match
            var timeOfDay = value.TimeOfDay;
            if (timeOfDay != MeasurementTime)
            {
                SetProperty(ref measurementTime, timeOfDay, nameof(MeasurementTime));
            }
        }

        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            // Initialize measurement time
            MeasurementTime = DateTime.Now.TimeOfDay;
            
            // Load data asynchronously
            Task.Run(LoadMeasurementsAsync);
        }

        [RelayCommand]
        private async Task SaveMeasurement()
        {
            var measurement = new WaterTemperatureMeasurement
            {
                Temperature = Temperature,
                MeasurementDate = MeasurementDate
            };

            await _databaseService.SaveMeasurementAsync(measurement);
            await LoadMeasurementsAsync();
        }

        private async Task LoadMeasurementsAsync()
        {
            try
            {
                var measurements = await _databaseService.GetMeasurementsAsync();
                
                // Update both collections on main thread
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Measurements.Clear();
                    ChartData.Clear();
                    
                    foreach (var measurement in measurements)
                    {
                        Measurements.Add(measurement);
                        ChartData.Add(measurement);
                    }
                });
            }
            catch (Exception)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Measurements.Clear();
                    ChartData.Clear();
                });
            }
        }

        [RelayCommand]
        private async Task DeleteMeasurement(WaterTemperatureMeasurement measurement)
        {
            if (measurement != null)
            {
                await _databaseService.DeleteMeasurementAsync(measurement);
                await LoadMeasurementsAsync();
            }
        }
    }
}
