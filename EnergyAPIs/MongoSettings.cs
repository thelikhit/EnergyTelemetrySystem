namespace EnergyAPIs
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017/?directConnection=true";
        public string DatabaseName { get; set; } = "energy_telemetry_data";
    }
}
