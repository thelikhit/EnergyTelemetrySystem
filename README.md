# Energy Telemetry System

A simulated EV charging station telemetry pipeline built with .NET 8. A simulator publishes MQTT messages
mimicking charger data, a background worker ingests and persists them to MongoDB, and a REST API exposes
the data for querying.

## Architecture
```
[ChargingStation] --MQTT--> [EnergyDataIngestor  (.NET Worker Service)]
                                          |
                                          v
                                [MongoDB (NoSQL)]
                                          |
                                          v
                               [EnergyApi (ASP.NET Core REST)]
                                          |
                                          v
                               [HTTP Client]
```

## Prerequisites

The following must be installed before running the project.

### 1. .NET 8 SDK

Download from https://dotnet.microsoft.com/download — make sure to get the **SDK**, not just the Runtime.

Verify:
```bash
dotnet --version
# Expected: 8.0.x
```

### 2. Docker Desktop

Download from https://www.docker.com/products/docker-desktop

Docker is used to run MongoDB and the MQTT broker locally without native installation.
Start Docker Desktop and leave it running in the background before proceeding.

### 3. MongoDB Compass (optional, recommended)

Download from https://www.mongodb.com/try/download/compass

A GUI for inspecting the MongoDB database. Useful for verifying that telemetry records are being saved correctly.

---

## Configuration

Connection strings are hardcoded in the respective projects:

| Service        | Default Address              |
|----------------|------------------------------|
| MongoDB        | mongodb://localhost:27017     |
| MQTT Broker    | mqtt://localhost:1883         |

To change these, update the relevant values directly in the source before running.

---

## Setup

### Start the MQTT Broker

An MQTT broker acts as the message hub. The simulator publishes to it; the ingestor subscribes and reads
from it. Eclipse Mosquitto is used here, run via Docker.

```bash
docker run -d --name mosquitto -p 1883:1883 eclipse-mosquitto
```

Verify the container is running:
```bash
docker ps
```

### Start MongoDB

```bash
docker run -d --name mongodb -p 27017:27017 mongo
```

MongoDB will be available at `mongodb://localhost:27017`. If you installed MongoDB Compass, connect to
this address to inspect the database.

---

## Running the System

The system has three components that must run concurrently. Use a separate terminal window for each.

### Terminal 1 — EnergyDataIngestor

Subscribes to MQTT and writes incoming telemetry to MongoDB.

```bash
cd EnergyDataIngestor
dotnet run
```

### Terminal 2 — ChargingStation

Publishes simulated charger telemetry to the MQTT broker every 5 seconds.

```bash
cd ChargingStation
dotnet run
```

Once both are running, Terminal 1 should begin showing `Saved to MongoDB` messages every x seconds.

### Terminal 3 — API

Exposes the telemetry data over HTTP. The ingestor and simulator must already be running.

```bash
cd EnergyAPIs
dotnet run
```

The port is assigned at launch. Note the URL printed on startup. Use that URL for all API calls below.

---

## API Reference

### List all stations
GET /api/stations

### Query telemetry by time range
GET /api/stations/{stationId}/telemetry?from={ISO8601}&to={ISO8601}

### Swagger UI

An interactive API explorer is available

## Project Structure
/ChargingStation - Publishes simulated MQTT telemetry

/EnergyDataIngestor - NET Worker Service which subscribes to MQTT, persists to MongoDB

/EnergyAPIs - ASP.NET Core REST API that serves telemetry data over HTTP

