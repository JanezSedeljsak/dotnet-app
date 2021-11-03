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


    public Country GetCountryByName(string name) {
        return db.country.Single(c => c.name == name);
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

    public dynamic AuthRegister(string fullname, DateTime birthdate, string email, string password) {
        return new User {
            fullname = fullname,
            birthdate = birthdate,
            email = email,
            password = BCrypt.Net.BCrypt.HashPassword(password),
        };
    }

    public dynamic AuthLogin(string email, string password) {
        // @TODO implement refresh token auth
        return new {
            token = BCrypt.Net.BCrypt.HashPassword("test")
        };
    }

    public Tuple<bool, User> GetAuth(AuthCredentials credentials) {
        var userByEmail = db.user.Single(u => u.email == credentials.email);
        if (userByEmail == null) {
            return Tuple.Create(false, userByEmail);
        }

        bool verified = BCrypt.Net.BCrypt.Verify(credentials.password, userByEmail.password);
        if (verified) {
            return Tuple.Create(verified, userByEmail);
        }

        return Tuple.Create(false, userByEmail);
    }
}

public class TokenService : ITokenService {
    private TimeSpan ExpiryDuration = new TimeSpan(0, 30, 0);
    public string BuildToken(string key, string issuer, User user) {
        var claims = new[] {
            new Claim(ClaimTypes.Name, user.email),
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