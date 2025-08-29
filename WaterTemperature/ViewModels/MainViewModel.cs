using System.Collections.ObjectModel;
using System.Windows.Input;
using Microcharts;
using SkiaSharp;
using WaterTemperature.Models;
using WaterTemperature.Services;

namespace WaterTemperature.ViewModels
{
    public class MainViewModel : BindableObject
    {
        private readonly DatabaseService _databaseService;
        private double _temperature;
        private DateTime _measurementDate = DateTime.Now;
        private Chart _chart;

        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            SaveCommand = new Command(async () => await SaveMeasurement());
            DeleteCommand = new Command<WaterTemperatureMeasurement>(async (measurement) => await DeleteMeasurement(measurement));

            // Initialize with a default chart showing "No data"
            InitializeEmptyChart();
            
            // Load data asynchronously
            Task.Run(async () => await LoadMeasurementsAsync());
        }

        public double Temperature
        {
            get => _temperature;
            set
            {
                if (_temperature != value)
                {
                    _temperature = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime MeasurementDate
        {
            get => _measurementDate;
            set
            {
                if (_measurementDate != value)
                {
                    _measurementDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<WaterTemperatureMeasurement> Measurements { get; } = new();

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }

        public Chart Chart
        {
            get => _chart;
            private set
            {
                if (_chart != value)
                {
                    _chart = value;
                    OnPropertyChanged();
                }
            }
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

                if (!measurements.Any())
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
                new ChartEntry(0)
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
