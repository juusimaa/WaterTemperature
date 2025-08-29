using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using SkiaSharp;
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
        [property: EditorBrowsable(EditorBrowsableState.Never)]
        private Chart? chart;

        public ObservableCollection<WaterTemperatureMeasurement> Measurements { get; } = [];

        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            // Initialize with a default chart showing "No data"
            InitializeEmptyChart();
            
            // Load data asynchronously
            Task.Run(LoadMeasurementsAsync);
        }

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
                
                // Update the measurements collection
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Measurements.Clear();
                    foreach (var measurement in measurements)
                    {
                        Measurements.Add(measurement);
                    }
                });

                if (measurements.Count == 0)
                {
                    MainThread.BeginInvokeOnMainThread(() => InitializeEmptyChart());
                    return;
                }

                var entries = measurements
                    .OrderBy(m => m.MeasurementDate)
                    .Select(m => new ChartEntry((float)m.Temperature)
                    {
                        Label = m.MeasurementDate.ToString("d"),
                        ValueLabel = $"{m.Temperature:F1}°C",
                        Color = SKColor.Parse("#2196F3")
                    })
                    .ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Chart = new LineChart
                    {
                        Entries = entries,
                        LabelTextSize = 30,
                        LabelOrientation = Orientation.Horizontal,
                        ValueLabelOrientation = Orientation.Horizontal,
                        LineSize = 8,
                        PointSize = 18,
                        BackgroundColor = SKColors.Transparent,
                        IsAnimated = true
                    };
                });
            }
            catch (Exception)
            {
                MainThread.BeginInvokeOnMainThread(() => InitializeEmptyChart());
            }
        }

        private async Task DeleteMeasurement(WaterTemperatureMeasurement measurement)
        {
            if (measurement != null)
            {
                await _databaseService.DeleteMeasurementAsync(measurement);
                await LoadMeasurementsAsync();
            }
        }

        private void InitializeEmptyChart()
        {
            var emptyEntries = new List<ChartEntry>
            {
                new(0)
                {
                    Label = "No data",
                    ValueLabel = "No data",
                    Color = SKColor.Parse("#2196F3")
                }
            };

            Chart = new LineChart
            {
                Entries = emptyEntries,
                LabelTextSize = 30,
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,
                LineSize = 8,
                PointSize = 18,
                BackgroundColor = SKColors.Transparent,
                IsAnimated = false
            };
        }
    }
}
