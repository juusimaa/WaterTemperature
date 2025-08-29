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
            var result = await _database.InsertAsync(measurement);
            // The measurement object's Id property is automatically updated by SQLite
            return result;
        }

        public async Task<WaterTemperatureMeasurement?> GetMeasurementByIdAsync(int id)
        {
            return await _database.Table<WaterTemperatureMeasurement>().Where(m => m.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> UpdateMeasurementAsync(WaterTemperatureMeasurement measurement)
        {
            return await _database.UpdateAsync(measurement);
        }

        public async Task<int> DeleteMeasurementAsync(WaterTemperatureMeasurement measurement)
        {
            return await _database.DeleteAsync(measurement);
        }
    }
}
