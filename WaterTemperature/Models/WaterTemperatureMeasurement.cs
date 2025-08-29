using SQLite;
using System.ComponentModel;

namespace WaterTemperature.Models
{
    public class WaterTemperatureMeasurement : INotifyPropertyChanged
    {
        private double? _temperature;
        private DateTime? _measurementDate;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public double? Temperature 
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
        
        public DateTime? MeasurementDate 
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

        // Helper property to check if the measurement has valid data
        public bool IsValid => Temperature.HasValue && MeasurementDate.HasValue;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
