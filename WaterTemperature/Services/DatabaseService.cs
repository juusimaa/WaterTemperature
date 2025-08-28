using SQLite;
using WaterTemperature.Models;

namespace WaterTemperature.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<WaterTemperatureMeasurement>().Wait();
        }

        public async Task<List<WaterTemperatureMeasurement>> GetMeasurementsAsync()
        {
            return await _database.Table<WaterTemperatureMeasurement>().OrderByDescending(x => x.MeasurementDate).ToListAsync();
        }

        public async Task<int> SaveMeasurementAsync(WaterTemperatureMeasurement measurement)
        {
            return await _database.InsertAsync(measurement);
        }
    }
}
