using DnsClient.Internal;
using MongoDB.Bson;
using MongoDB.Driver;
using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;

namespace EnergyDataIngestor
{
    public class Worker(ILogger<Worker> logger) : BackgroundService
    {

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // connect to MongoDB client
            var mongoClient = new MongoClient("mongodb://localhost:27017/?directConnection=true");
            var database = mongoClient.GetDatabase("energy_telemetry_data");
            var collection = database.GetCollection<EnergyTelemetryDocument>("energy_telemetry");

            // compound indexing on charger id and timestamp
            var indexModel = new CreateIndexModel<EnergyTelemetryDocument>(
                Builders<EnergyTelemetryDocument>.IndexKeys
                    .Ascending(x => x.ChargerId)
                    .Descending(x => x.Timestamp),
                new CreateIndexOptions { Name = "station_time_idx" }
            );
            await collection.Indexes.CreateOneAsync(indexModel, cancellationToken: stoppingToken);
            logger.LogInformation("Index created on StationId + Timestamp.");

            // Connect to MQTT broker
            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();

            // async event handler that fires every time a message arrives
            mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                // decode raw bytes back to UTF8 string
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                logger.LogInformation("Received: {payload}", payload);

                try
                {
                    // Deserialize the JSON
                    var incoming = JsonSerializer.Deserialize<EnergyTelemetry>(payload,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (incoming == null) return;

                    // Map to  MongoDB document model
                    var doc = new EnergyTelemetryDocument
                    {
                        ChargerId = incoming.ChargerId,
                        Timestamp = incoming.Timestamp,
                        PowerKw = incoming.PowerKw,
                        CurrentA = incoming.CurrentA,
                        VoltageV = incoming.VoltageV,
                        State = incoming.State
                    };

                    await collection.InsertOneAsync(doc);
                    logger.LogInformation("Saved to MongoDB: {stationId} at {time}", doc.ChargerId, doc.Timestamp);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process message");
                }
            };

            // MQTT connection setup
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 1883)
                .WithClientId("energy-ingestor")
                .Build();

            await mqttClient.ConnectAsync(options, stoppingToken);

            // Subscribe to ALL stations using the '+' wildcard
            await mqttClient.SubscribeAsync("stations/+/telemetry");
            logger.LogInformation("Subscribed to stations/+/telemetry");

            // Keep running until the service is stopped
            await Task.Delay(Timeout.Infinite, stoppingToken);


        }
    }
}
