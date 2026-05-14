using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

// web app builder
var builder = WebApplication.CreateBuilder(args);

// one instance (db connection) created for entire application aka singleton  
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient("mongodb://localhost:27017"));

// register MVC controller
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// swagger ui
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();