using Core.SeedData;

var builder = WebApplication.CreateBuilder(args);
var conStr = builder.Configuration.GetConnectionString("ProdDb");
builder.Services.AddDbContext<TravelLogContext>(x => x.UseSqlServer(conStr));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IDataRepository, DataRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddAuthorization();
builder.Services.AddCors();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt => TokenService.buildTokenOptions(opt, builder));

builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Travel log service API", Version = "v1" });
});

var app = builder.Build();
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .AllowCredentials()); // allow credentials
    
app.UseAuthentication();
app.UseAuthorization();

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

app.MapGet("api/v1/sync/countries", async (IDataRepository db) => {
    var (syncStatus, syncMessage) = await db.SyncCountries();
    var responseMsg = T.get("RESPONSE_STATUS", syncMessage);

    return new StatusResponse(syncStatus, responseMsg);
});

app.MapGet("api/v1/pickers/{model}", [Authorize] (IDataRepository db, string model) => {
    return db.GetShowAsRows(model);
});

app.MapDelete("api/v1/{model}/{id}", async (IDataRepository db, string model, string id) => {
    var status = await db.DeactivateColumn(model, id);
    return new StatusResponse(status, status == false ? "DEACTIVATE_FAILED" : "");
});

app.MapGet("api/v1/{model}", [AllowAnonymous] (IDataRepository db, string model) => {
    return model switch {
        "countries" => db.GetCountries(),
        "users" => db.GetUsers(),
        "destinations" => db.GetDestinations(),
        "trips" => db.GetTrips(null),
        _ => throw new Exception($"Invalid model name: {model}")
    };
});

app.MapGet("api/v1/my-trips", [Authorize] async (HttpContext http, IDataRepository db, string model) => {
    var (userId, _, _, _) = TokenService.destructureToken(http);
    return db.GetTrips(userId);
});

app.MapGet("api/v1/my-profile", [Authorize] async (HttpContext http, IDataRepository db, string model) => {
    var (userId, _, _, _) = TokenService.destructureToken(http);
    return db.GetUserById(userId);
});

app.MapGet("api/v1/{model}/{id}", (IDataRepository db, string model, string id) => {
    var (status, data) = db.GetModelById(model, id);
    return status == false ? null : data;
});

app.MapPost("api/v1/{model}", [Authorize] async (HttpContext http, IDataRepository db, string model) => {
    var (userId, _, _, _) = TokenService.destructureToken(http);

    var insertStatus = model switch {
        "destinations" => await db.InsertDestination(await http.Request.ReadFromJsonAsync<Destination>(), userId),
        "trip" => await db.InsertTrip(await http.Request.ReadFromJsonAsync<Trip>(), userId),
        "tripuser" => await db.InsertTripUser(await http.Request.ReadFromJsonAsync<TripUser>(), userId),
        _ => throw new Exception($"Invalid model name: {model}")
    };

    return new StatusResponse(insertStatus, (!insertStatus ? "DATA_INSERT_FAILED" : "DATA_INSERT_SUCCESS"));
});

app.MapPut("api/v1/{model}/{id}", [Authorize] async (HttpContext http, IDataRepository db, string model, string id) => {
    var (userId, _, isAdmin, _) = TokenService.destructureToken(http);
    var updateStatus = model switch {
        "destinations" => await db.UpdateDestination(await http.Request.ReadFromJsonAsync<Destination>(), id, userId, isAdmin),
        "trip" => await db.UpdateTrip(await http.Request.ReadFromJsonAsync<Trip>(), id, userId, isAdmin),
        "tripuser" => await db.UpdateTripUser(await http.Request.ReadFromJsonAsync<TripUser>(), id, userId, isAdmin),
        _ => throw new Exception($"Invalid model name: {model}")
    };

    return new StatusResponse(updateStatus, (!updateStatus ? "DATA_UPDATE_FAILED" : "DATA_UPDATE_SUCCESS"));
});

app.MapPost("api/v1/auth/register", [AllowAnonymous] async (HttpContext http, IAuthRepository db) => {
    var newUserData = await http.Request.ReadFromJsonAsync<User>();
    var (status, userCreated) = db.AuthRegister(newUserData);
    await http.Response.WriteAsJsonAsync(new { status = status, user = userCreated });
});

app.MapPost("api/v1/auth/login", [AllowAnonymous] async (HttpContext http, ITokenService tokenService, IAuthRepository db) => {
    var userModel = await http.Request.ReadFromJsonAsync<AuthCredentials>();
    var (status, authUser, responseMessage) = db.GetAuth(userModel);
    if (!status) {
        http.Response.StatusCode = 401;
        await http.Response.WriteAsJsonAsync(new { message = responseMessage });
    }

    var token = tokenService.BuildToken(builder.Configuration["Jwt:Key"], builder.Configuration["Jwt:Issuer"], authUser);
    await http.Response.WriteAsJsonAsync(new { token = token });
});

app.MapGet("api/v1/stats/popular-destinations", [AllowAnonymous] async (HttpContext http, IDataRepository db) => {
    var destinations = db.PopularDestinations();
    await http.Response.WriteAsJsonAsync(destinations);
});

app.MapGet("api/v1/stats/active-users", [AllowAnonymous] async (HttpContext http, IDataRepository db) => {
    var topUsers = db.GetUsers();
    await http.Response.WriteAsJsonAsync(topUsers);
});

app.Run();
