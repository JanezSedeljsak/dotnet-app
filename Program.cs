
using Microsoft.EntityFrameworkCore;
using ContextWrapper;
using Services.Helpers;
using Services.Translations;
using Response;
using Models;

var builder = WebApplication.CreateBuilder(args);
var conStr = builder.Configuration.GetConnectionString("AppDb");
builder.Services.AddDbContext<TravelLog>(x => x.UseMySQL(conStr));

var app = builder.Build();
var workingFunc = () => "Working...";
var T = new TranslateService();

app.MapGet("/heartbeat", workingFunc);
app.MapGet("/", workingFunc);

app.MapGet("api/v1/sync/countries", async (http) => {
    var syncStatus = await Helpers.SyncCountries();
    var responseMsg = T.get("RESPONSE_STATUS", syncStatus ? "SYNC_SUCCESS" : "SYNC_FAIL");
    var statResponse = new StatusResponse(syncStatus, responseMsg);

    await http.Response.WriteAsJsonAsync(statResponse);
});

app.MapGet("api/v1/countries", async (http) => {
    var context = new TravelLog();
    var listOfCountries = Country.get(context);
    await http.Response.WriteAsJsonAsync(listOfCountries);
});

app.MapGet("api/v1/country/{name}", async (http) => {
    var context = new TravelLog();
    var countryName = http.Request.RouteValues["name"].ToString();
    var country = Country.getCountryByName(context, countryName);
    await http.Response.WriteAsJsonAsync(country);

}); 

app.Run();
