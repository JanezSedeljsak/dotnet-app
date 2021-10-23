using Services.Helpers;
using ContextWrapper;
using Microsoft.EntityFrameworkCore;
using Response;

var builder = WebApplication.CreateBuilder(args);
var conStr = builder.Configuration.GetConnectionString("AppDb");
builder.Services.AddDbContext<TravelLog>(x => x.UseMySQL(conStr));

var app = builder.Build();
var workingFunc = () => "Working...";

app.MapGet("/heartbeat", workingFunc);
app.MapGet("/", workingFunc);

app.MapGet("api/sync/countries", async(http) => {
    var syncStatus = await Helpers.SyncCountries();
    var statResponse = new StatusResponse(syncStatus, syncStatus ? "Successfully synced countries" : "Error while syncing countries");

    await http.Response.WriteAsJsonAsync(statResponse);
});

app.Run();
