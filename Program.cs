using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Core.ContextWrapper;
using Services.Translations;
using Services.Response;
using Core.IData;
using Core.DataRep;

var builder = WebApplication.CreateBuilder(args);
var conStr = builder.Configuration.GetConnectionString("AppDb");
builder.Services.AddDbContext<TravelLogContext>(x => x.UseMySql(conStr, new MySqlServerVersion(new Version(10, 5, 4))));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IDataRepository, DataRepository>();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Travel log service API", Version = "v1" });
});

var app = builder.Build();
app.UseSwaggerUI();
app.UseSwagger(x => x.SerializeAsV2 = true);

var workingFunc = () => "Working...";
var T = new TranslateService();

if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}

app.MapGet("/heartbeat", workingFunc);
app.MapGet("/", workingFunc);

app.MapGet("api/v1/sync/countries", async ([FromServices] IDataRepository db) => {
    var syncStatus = await db.SyncCountries();
    var responseMsg = T.get("RESPONSE_STATUS", syncStatus ? "SYNC_SUCCESS" : "SYNC_FAIL");

    return new StatusResponse(syncStatus, responseMsg);
});

app.MapGet("api/v1/countries", ([FromServices] IDataRepository db) => {
    return db.GetCountries();
});

app.MapGet("api/v1/country/{name}", ([FromServices] IDataRepository db, string name) => {
    return db.GetCountryByName(name);
});

app.Run();
