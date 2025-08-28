using System.Collections.ObjectModel;
using System.Windows.Input;
using WaterTemperature.Models;
using WaterTemperature.Services;

namespace WaterTemperature.ViewModels
{
    public class MainViewModel : BindableObject
    {
        private readonly DatabaseService _databaseService;
        private double _temperature;
        private DateTime _measurementDate = DateTime.Now;

        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            SaveCommand = new Command(async () => await SaveMeasurement());
            LoadMeasurementsAsync().ConfigureAwait(false);
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
            var measurements = await _databaseService.GetMeasurementsAsync();
            Measurements.Clear();
            foreach (var measurement in measurements)
            {
                Measurements.Add(measurement);
            }
        }
    }
}
