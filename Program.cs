using System;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/heartbeat", async httpContext => {
    await httpContext.Response.WriteAsync("hello");
});

app.MapGet("/test-params", async httpContext => {
    var getParams = httpContext.Request.Query;
    foreach (var paramKey in getParams.Keys) {
        Console.WriteLine($"{paramKey} -> {getParams[paramKey]}");
    }
    
    await httpContext.Response.WriteAsync(getParams.ToString());
});

app.Run();
