using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;

// create MQTT client factory
var mqttFactory = new MqttFactory();

// create MQTT client instance
var mqttClient = mqttFactory.CreateMqttClient();

// create MQTT client options (MQTT client configuration settings)
var mqttClientOptions = new MqttClientOptionsBuilder()
    .WithTcpServer("localhost", 1883)
    .WithClientId("charging-station")
    .Build();

// open TCP connection to broker
await mqttClient.ConnectAsync(mqttClientOptions);
Console.WriteLine("Charging Station connected to broker.");

// a charger in a charging station
var chargerId = "CHARGER-01";
var random = new Random();
var state = "Charging";

while (true)
{
    // simulate telemetry reading
    var energyData = new
    {
        chargerId = chargerId,
        timestamp = DateTime.UtcNow,
        powerKw = Math.Round(random.NextDouble() * 11 + 0.5, 2),  // 0.5 to 11.5 kW
        currentA = Math.Round(random.NextDouble() * 16 + 1, 2),   // 1 to 17 A
        voltageV = 230,
        state = state
    };

    // serealize object to JSON string
    var energyDataJSON = JsonSerializer.Serialize(energyData);

    // topic
    var topic = $"stations/{chargerId}/telemetry";

    // Publish energy data in JSON to MQTT broker
    var message = new MqttApplicationMessageBuilder()
        .WithTopic(topic)
        .WithPayload(Encoding.UTF8.GetBytes(energyDataJSON))
        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
        .Build();

    // publish message to broker
    await mqttClient.PublishAsync(message);
    Console.WriteLine($"Published: {energyDataJSON}");

    // publish to broker every x ms
    await Task.Delay(2000);
}

