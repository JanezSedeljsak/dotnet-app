using Core.IData;
using Core.Models;
using Core.ContextWrapper;
using Newtonsoft.Json;

namespace Core.DataRep;

public class DataRepository : IDataRepository {
    private readonly TravelLogContext db;

    public DataRepository(TravelLogContext db) {
        this.db = db;
    }

    public List<dynamic> GetShowAsRows(string modelName) {
        var dataList = db.GetDbSet(modelName);
        var result = new List<dynamic>();

        foreach (var item in dataList) {
            result.Add(new {
                Id = item.id,
                ShowAs = item.GetShowAs()
            });
        }

        return result;
    }

    public bool DeactivateColumn(string modelName, string id) {
        var dbRow = db.GetDbSet(modelName).FirstOrDefault(row => row.id == id);
        dbRow.Deactivate();
        db.SaveChanges();
        return true;
    }

    public List<dynamic> GetCountries() {
        var data = (
            from c in db.country
            join r in db.region
            on c.region.id equals r.id
            select new { 
                Id = r.id,
                CountryName = c.name,
                RegionName = r.name,
                CountryCode = c.countryCode,
            }).ToList<dynamic>();

        return data;
    }

    public List<dynamic> GetDestinations() {
        var data = (
            from d in db.destination
            join c in db.country on d.country.id equals c.id
            join r in db.region on c.region.id equals r.id
            select new { 
                Id = d.id,
                Destination = d.name,
                CountryName = c.name,
                RegionName = r.name,
                CountryCode = c.countryCode,
            }).ToList<dynamic>();

        return data;
    }

    public List<dynamic> GetUsers() {
        var data = (
            from u in db.user
            select new {
                Id = u.id,
                FullName = u.fullname,
                Email = u.email,
                BirthDate = u.birthdate
            }).ToList<dynamic>();

        return data;
    }

     public List<dynamic> GetTrips() {
        var tripUsers = (
            from tu in db.tripuser
            join u in db.user on tu.user.id equals u.id
            select new {
                TripId = tu.trip.id,
                FullName = u.fullname,
                Email = u.email,
                BirthDate = u.birthdate
            }).ToList<dynamic>();

        var tripData = (
            from t in db.trip
            join d in db.destination on t.destination.id equals d.id
            join c in db.country on d.country.id equals c.id
            join r in db.region on c.region.id equals r.id
            select new { 
                Id = t.id,
                TripName = t.name,
                TripDate = t.tripdate,
                Destination = d.name,
                CountryName = c.name,
                RegionName = r.name,
                CountryCode = c.countryCode
            }).ToList<dynamic>();
        
        var data = new List<dynamic>();
        foreach (var row in tripData) {
            data.Add(new {
                TripName = row.TripName,
                TripDate = row.TripDate,
                Destination = row.Destination,
                CountryName = row.CountryName,
                RegionName = row.RegionName,
                CountryCode = row.CountryCode,
                UserList = tripUsers.Where(u => u.TripId == row.Id).ToList<dynamic>()
            });
        }

        return data;
    }

    public bool InsertDestination(Destination d) {
        db.destination.AddRange(d);
        db.SaveChanges();

        return true;
    }

    public bool InsertTrip(Trip t) {
        return true;
    }

    public bool InsertTripUser(TripUser tu) {
        return true;
    }

    public bool UpdateDestination(Destination d, string id) {
        return true;
    }

    public bool UpdateTrip(Trip t, string id) {
        return true;
    }

    public bool UpdateTripUser(TripUser tu, string id) {
        return true;
    }

    public Tuple<bool, BaseModel> GetModelById(string model, string id) {
        string[] allowedModels = {"destinations", "users", "countries", "trips"};
        if (allowedModels.Contains(model)) {
            return Tuple.Create(true, db.GetDbSet(model).FirstOrDefault(row => row.id == id));
        }

        return Tuple.Create(false, new BaseModel());
    }

    public async Task<Tuple<bool, string>> SyncCountries() {
        if (db.country.Any() || db.region.Any()) return Tuple.Create(false, "DATA_EXISTS");

        var client = new HttpClient();
        var data = await client.GetStringAsync("https://restcountries.com/v3.1/all");
        var response = JsonConvert.DeserializeObject<dynamic>(data);

        var countries = new List<Tuple<string, Country>>();
        var regions = new Dictionary<string, Region>();

        foreach (var country in response) {
            string region = country["region"];
            string countryName = country["name"]["common"];
            string countryCode = country["fifa"];

            regions[region] = new Region {
                name = region
            };

            countries.Add(Tuple.Create(region, new Country {
                name = countryName,
                countryCode = countryCode
            }));
        }

        var countryData = new List<Country>();
        foreach (var country in countries) {
            var tempCountry = country.Item2;
            if (regions.TryGetValue(country.Item1, out var tempRegion)) {
                tempCountry.region = tempRegion;
            }

            countryData.Add(tempCountry);
        }

        var regionData = regions.Values.ToList();
        db.region.AddRange(regionData);
        db.country.AddRange(countryData);
        db.SaveChanges();
     
        return Tuple.Create(true, "SYNC_SUCCESS");
    }
}

public class AuthRepository : IAuthRepository {
    private readonly TravelLogContext db;

    public AuthRepository(TravelLogContext db) {
        this.db = db;
    }

    public Tuple<bool, User> AuthRegister(User user) {
        var newUser = new User {
            fullname = user.fullname,
            birthdate = user.birthdate,
            email = user.email,
            password = BCrypt.Net.BCrypt.HashPassword(user.password),
        };

        db.user.AddRange(newUser);
        db.SaveChanges();

        return Tuple.Create(true, newUser);
    }

    public Tuple<bool, User, string> GetAuth(AuthCredentials credentials) {
        var userByEmail = db.user.FirstOrDefault(u => u.email == credentials.email);
        if (userByEmail == null) {
            return Tuple.Create(false, userByEmail, "NO_USER");
        }

        bool verified = BCrypt.Net.BCrypt.Verify(credentials.password, userByEmail.password);
        if (verified) {
            return Tuple.Create(verified, userByEmail, "");
        }

        return Tuple.Create(false, userByEmail, "PASSWORD_MISSMATCH");
    }

    public User ParseUser(HttpContext http) {
        User currentUser;
        if (http.User.Identity is ClaimsIdentity identity) {
            string userId = identity.FindFirst(ClaimTypes.Name)?.Value;
            currentUser = db.user.FirstOrDefault(u => u.id == userId);
            if (currentUser != null) {
                return currentUser;
            }
        }

        http.Response.StatusCode = 401;
        http.Response.WriteAsJsonAsync(new { message = "TOKEN_PARSE_FAILED" });
        throw new Exception("Token parse failed");
    }
}

public class TokenService : ITokenService {
    private TimeSpan ExpiryDuration = new TimeSpan(0, 30, 0);
    public string BuildToken(string key, string issuer, User user) {
        var claims = new[] {
            new Claim(ClaimTypes.Name, user.id),
            new Claim(ClaimTypes.NameIdentifier,
            Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new JwtSecurityToken(issuer, issuer, claims,
            expires: DateTime.Now.Add(ExpiryDuration), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}