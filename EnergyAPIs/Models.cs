using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

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

// DTO
public class EnergyTelemetryDto
{
    public string ChargerId { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public double PowerKw { get; set; }
    public double CurrentA { get; set; }
    public double VoltageV { get; set; }
    public string State { get; set; } = "";
}