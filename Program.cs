using ContextWrapper;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var conStr = builder.Configuration.GetConnectionString("AppDb");
builder.Services.AddDbContext<TravelLog>(x => x.UseMySQL(conStr));

var app = builder.Build();

app.MapGet("/heartbeat", async httpContext => {
    await httpContext.Response.WriteAsync("hello");
});

app.Run();
