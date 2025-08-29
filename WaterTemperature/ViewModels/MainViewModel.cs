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

        // Chart data should only include valid measurements (with both temperature and date)
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
            // Only save if we have valid temperature and date values
            if (Temperature > 0 && MeasurementDate != default)
            {
                var measurement = new WaterTemperatureMeasurement
                {
                    Temperature = Temperature,
                    MeasurementDate = MeasurementDate
                };

                await _databaseService.SaveMeasurementAsync(measurement);
                await LoadMeasurementsAsync();
                
                // Reset form values
                Temperature = 0;
                MeasurementDate = DateTime.Now;
                MeasurementTime = DateTime.Now.TimeOfDay;
            }
        }

        [RelayCommand]
        private async Task AddNewMeasurement()
        {
            // Create a new measurement with null values (no database save until valid data is entered)
            var newMeasurement = new WaterTemperatureMeasurement
            {
                Temperature = null, // No default value
                MeasurementDate = null // No default value
            };

            // Add to collection for editing without saving to database
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Measurements.Add(newMeasurement);
            });
        }

        private async Task LoadMeasurementsAsync()
        {
            try
            {
                var measurements = await _databaseService.GetMeasurementsAsync();
                
                // Update the collections on main thread
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // Keep track of any invalid measurements (with null values) that are being edited
                    var invalidMeasurements = Measurements.Where(m => !m.IsValid).ToList();
                    
                    Measurements.Clear();
                    ChartData.Clear();
                    
                    // Add valid measurements from database to both collections
                    foreach (var measurement in measurements)
                    {
                        Measurements.Add(measurement);
                        // Only add to chart if valid (has both temperature and date)
                        if (measurement.IsValid)
                        {
                            ChartData.Add(measurement);
                        }
                    }
                    
                    // Add back any invalid measurements to the grid (but not to chart)
                    foreach (var invalidMeasurement in invalidMeasurements)
                    {
                        Measurements.Add(invalidMeasurement);
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

        // Public method to refresh data from external calls (like DataGrid updates)
        public async Task RefreshDataAsync()
        {
            await LoadMeasurementsAsync();
        }

        // Method to manually sync chart data with current measurements
        public void SyncChartData()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ChartData.Clear();
                foreach (var measurement in Measurements.Where(m => m.IsValid))
                {
                    ChartData.Add(measurement);
                }
            });
        }

        [RelayCommand]
        private async Task DeleteMeasurement(WaterTemperatureMeasurement measurement)
        {
            if (measurement != null)
            {
                // Show confirmation dialog
                bool confirm = await Shell.Current.DisplayAlert(
                    "Delete Measurement",
                    measurement.IsValid 
                        ? $"Are you sure you want to delete the measurement of {measurement.Temperature}°C from {measurement.MeasurementDate:g}?"
                        : "Are you sure you want to delete this incomplete measurement?",
                    "Delete",
                    "Cancel");

                if (confirm)
                {
                    // Only try to delete from database if it has an ID (was saved)
                    if (measurement.Id > 0)
                    {
                        await _databaseService.DeleteMeasurementAsync(measurement);
                        await LoadMeasurementsAsync();
                    }
                    else
                    {
                        // Just remove from memory if not saved to database
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            Measurements.Remove(measurement);
                        });
                    }
                }
            }
        }
    }
}
