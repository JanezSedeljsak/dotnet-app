using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Core.ContextWrapper;
using Services.Translations;
using Services.Response;
using Core.SeedData;
using Core.IData;
using Core.DataRep;
using Core.Models;

var builder = WebApplication.CreateBuilder(args);
var conStr = builder.Configuration.GetConnectionString("AppDb");
builder.Services.AddDbContext<TravelLogContext>(x => x.UseMySql(conStr, new MySqlServerVersion(new Version(10, 5, 4))));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IDataRepository, DataRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt => {
    opt.TokenValidationParameters = new () {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Travel log service API", Version = "v1" });
});

var app = builder.Build();
app.UseSwaggerUI();
app.UseSwagger(x => x.SerializeAsV2 = true);


var workingFunc = () => new StatusResponse(true, "Server running...");
var T = new TranslateService();

if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}

// base actions on server init
if (args.Length == 1) {
    switch (args[0].ToLower()) {
        case "seeddata":
            using (var db = new TravelLogContext()) {
                bool seedStatus = await SeedRepository.CreateMockData(db, app);
                Console.WriteLine($"Seed finished with status {seedStatus}");
            }
            break;
        case "synccountries":
            var dbRep = new DataRepository(new TravelLogContext());
            var (_, syncMessage) = await dbRep.SyncCountries();
            var responseMsg = T.get("RESPONSE_STATUS", syncMessage);
            Console.WriteLine($"Response message from country sync: {responseMsg}");
            break;
        default:
            break;
    }
}

app.MapGet("/heartbeat", workingFunc);
app.MapGet("/", workingFunc);

app.MapGet("api/v1/sync/countries", async ([FromServices] IDataRepository db) => {
    var (syncStatus, syncMessage) = await db.SyncCountries();
    var responseMsg = T.get("RESPONSE_STATUS", syncMessage);

    return new StatusResponse(syncStatus, responseMsg);
});

app.MapGet("api/v1/{model}", ([AllowAnonymous]([FromServices] IDataRepository db, string model) => {
    return model switch {
        "countries" => db.GetCountries(),
        "users" => db.GetUsers(),
        "destinations" => db.GetDestinations(),
        "trips" => db.GetTrips(),
        _ => throw new Exception($"Invalid model name: {model}")
    };
}));

app.MapGet("api/v1/country/{name}", ([Authorize]([FromServices] IDataRepository db, string name) => {
    return db.GetCountryByName(name);
}));

app.MapPost("api/v1/auth/register", ([AllowAnonymous](ITokenService tokenService, IAuthRepository db) => {
    return db.AuthRegister("Random Person", new DateTime(1980, 1, 1), "random.person@gmail.com", "blabla123");
}));

app.MapPost("api/v1/auth/login", [AllowAnonymous]  async (HttpContext http,ITokenService tokenService, IAuthRepository db) => {
    var userModel = await http.Request.ReadFromJsonAsync<AuthCredentials>();
    var (status, authUser) = db.GetAuth(userModel);
    if (!status) {
        http.Response.StatusCode = 401;
        return;
    }

    var token = tokenService.BuildToken(builder.Configuration["Jwt:Key"], builder.Configuration["Jwt:Issuer"], authUser);
    await http.Response.WriteAsJsonAsync(new { token = token });
});

app.Run();
