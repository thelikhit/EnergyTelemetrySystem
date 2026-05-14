using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("api/chargers")]
public class EnergyTelemetryController : ControllerBase
{
    private readonly IMongoCollection<EnergyTelemetryDocument> _collection;

    // receives IMongoClient via DI
    public EnergyTelemetryController(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("energy_system");
        _collection = database.GetCollection<EnergyTelemetryDocument>("telemetry");
    }

    // GET /api/chargers
    // no args
    [HttpGet]
    public async Task<IActionResult> GetStations()
    {
        // gets all distinct stations
        var stationIds = await _collection
            .Distinct<string>("stationId", FilterDefinition<EnergyTelemetryDocument>.Empty)
            .ToListAsync();

        // returns station id
        return Ok(stationIds);
    }

    // GET /api/chargers/{id}/telemetry?from=2026-05-10&to=2026-05-11
    // {id} is route parameter
    [HttpGet("{id}/telemetry")]
    public async Task<IActionResult> GetTelemetry(
        string id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        // Validate
        if (from.HasValue && to.HasValue && from > to)
            return BadRequest("'from' must be earlier than 'to'.");

        // build the filter dynamically i.e. build the query dynmically 
        // filter for id == chargerId
        var builder = Builders<EnergyTelemetryDocument>.Filter;
        var filter = builder.Eq(x => x.ChargerId, id);

        // filter for from and to timestamps. 
        if (from.HasValue)
            filter &= builder.Gte(x => x.Timestamp, from.Value);
        if (to.HasValue)
            filter &= builder.Lte(x => x.Timestamp, to.Value);

        // execute query
        var docs = await _collection
            .Find(filter)
            .SortByDescending(x => x.Timestamp)
            .Limit(100)
            .ToListAsync();

        // map db model to api model 
        var dtos = docs.Select(d => new EnergyTelemetryDto
        {
            ChargerId = d.ChargerId,
            Timestamp = d.Timestamp,
            PowerKw = d.PowerKw,
            CurrentA = d.CurrentA,
            VoltageV = d.VoltageV,
            State = d.State
        });

        return Ok(dtos);
    }
}