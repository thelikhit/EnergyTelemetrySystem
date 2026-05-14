using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

// What arrives from the MQTT simulator
public class EnergyTelemetry
{
    public string ChargerId { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public double PowerKw { get; set; }
    public double CurrentA { get; set; }
    public double VoltageV { get; set; }
    public string State { get; set; } = "";
}

// What gets stored in MongoDB
// The [Bson...] attributes tell the MongoDB driver how to map C# properties to document fields
public class EnergyTelemetryDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("chargerId")]
    public string ChargerId { get; set; } = "";

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("powerKw")]
    public double PowerKw { get; set; }

    [BsonElement("currentA")]
    public double CurrentA { get; set; }

    [BsonElement("voltageV")]
    public double VoltageV { get; set; }

    [BsonElement("state")]
    public string State { get; set; } = "";
}