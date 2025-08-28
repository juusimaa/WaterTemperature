using SQLite;

namespace WaterTemperature.Models
{
    public class WaterTemperatureMeasurement
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public double Temperature { get; set; }
        
        public DateTime MeasurementDate { get; set; }
    }
}
