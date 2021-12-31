using Newtonsoft.Json;
using System.Globalization;

namespace Core.DataRepositoryWrapper;

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

    public async Task<bool> DeactivateColumn(string modelName, string id, string userId, bool isAdmin) {
        Console.WriteLine($"Deleting -> {id}");
        var dbRow = db.GetDbSet(modelName).FirstOrDefault(row => row.id == id);
        if (isAdmin || dbRow.createdBy == userId) {
            dbRow.Deactivate();
            return (await db.SaveChangesAsync()) > 0;
        }
        return false;
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
            where u.isAdmin == false
            select new {
                Id = u.id,
                FullName = u.fullname,
                Email = u.email,
                BirthDate = u.birthdate,
                Trips = (
                    from tu in db.tripuser
                    join t in db.trip on tu.tripid equals t.id
                    join d in db.destination on t.destinationid equals d.id
                    join c in db.country on d.countryid equals c.id
                    where tu.userid == u.id
                    where t.isActive == true
                    select new {
                        Id = t.id,
                        TripName = t.name,
                        TripDate = t.tripdate,
                        Destination = d.name,
                        CountryName = c.name,
                    }
                ).ToList<dynamic>()
            }).ToList<dynamic>();

        return data;
    }

     public List<dynamic> GetTrips(String userId) {
        var tripUsers = (
            from tu in db.tripuser
            join u in db.user on tu.user.id equals u.id
            select new {
                TripId = tu.trip.id,
                FullName = u.fullname,
                Email = u.email,
                BirthDate = u.birthdate,
                Rating = tu.rating,
                Notes = tu.notes,
                UserId = u.id
            }).ToList<dynamic>();

        var tripData = (
            from t in db.trip
            join d in db.destination on t.destination.id equals d.id
            join c in db.country on d.country.id equals c.id
            join r in db.region on c.region.id equals r.id
            where t.isActive == true
            select new { 
                Id = t.id,
                TripName = t.name,
                TripDate = t.tripdate,
                Destination = d.name,
                CountryName = c.name,
                RegionName = r.name,
                CountryCode = c.countryCode,
                CreatedBy = t.createdBy,
            }).ToList<dynamic>();
        
        var data = new List<dynamic>();
        foreach (var row in tripData) {
            var userList = tripUsers.Where(u => u.TripId == row.Id).ToList<dynamic>();
            var ratingSum = .0;
            var ratingCount = 0;

            foreach (var tripUser in userList) {
                if (tripUser.Rating != null) {
                    ratingSum += tripUser.Rating;
                    ratingCount += 1;
                }
            }

            if (userId != null && !userList.Any(x => x.UserId == userId)) {
                continue;
            }

            data.Add(new {
                Id = row.Id,
                TripName = row.TripName,
                TripDate = row.TripDate,
                Destination = row.Destination,
                CountryName = row.CountryName,
                RegionName = row.RegionName,
                CountryCode = row.CountryCode,
                CreatedBy = row.CreatedBy,
                AvgRating = ratingCount != 0 ? ratingSum / (double)ratingCount : -1,
                UserList = userList
            });
        }

        return data.OrderByDescending(o => o.TripDate).ToList();
    }

    public async Task<bool> InsertDestination(Destination d, string userId) {
        d.createdBy = userId;
        db.destination.AddRange(d);
        return (await db.SaveChangesAsync()) > 0;
    }

    public async Task<bool> InsertTrip(Trip t, string userId) {
        t.createdBy = userId;
        db.trip.AddRange(t);
        var tripUserList = t.tripUsers;

        if (t.tripUsers != null) {
            foreach (var user in t.tripUsers) {
                user.tripid = t.id; // bind parent
            }
            db.tripuser.AddRange(tripUserList);
        }
       
        return (await db.SaveChangesAsync()) > 0;
    }

    public async Task<bool> InsertTripUser(TripUser tu, string userId) {
        tu.createdBy = userId;
        db.tripuser.AddRange(tu);
        return (await db.SaveChangesAsync()) > 0;
    }

    public async Task<bool> UpdateDestination(Destination d, string id, string userId, bool isAdmin) {
        var record = db.destination.FirstOrDefault(r => r.id == id);
        if (record == null || !record.AllowEdit(userId, isAdmin)) {
            return false;
        }

        record.name = d.name != null ? d.name : record.name;
        return (await db.SaveChangesAsync()) > 0;
    }

    public async Task<bool> UpdateTrip(Trip t, string id, string userId, bool isAdmin) {
        var record = db.trip.FirstOrDefault(r => r.id == id);
        if (record == null || !record.AllowEdit(userId, false)) {
            return false;
        }

        record.name = t.name != null ? t.name : record.name;
        record.tripdate = t.tripdate != null ? t.tripdate : record.tripdate;
        if ((await db.SaveChangesAsync()) <= 0) {
            return false;
        }

        if (record.tripUsers != null) {
            foreach (var tripUser in t.tripUsers) {
                bool tmpStatus;
                if (tripUser.id != null) {
                    tmpStatus = await this.UpdateTripUser(tripUser, tripUser.id, userId, isAdmin);
                } else {
                    tmpStatus = await this.InsertTripUser(tripUser, userId);
                }

                if (!tmpStatus) {
                    return false;
                }
            }
        }

        return true;
    }

    public async Task<bool> UpdateTripUser(TripUser tu, string id, string userId, bool isAdmin) {
        var record = db.tripuser.FirstOrDefault(r => r.id == id);
        if (record == null || !record.AllowEdit(userId, isAdmin)) {
            return false;
        }

        record.notes = tu.notes != null ? tu.notes : record.notes;
        record.rating = tu.rating != null ? tu.rating : record.rating;
        return (await db.SaveChangesAsync()) > 0;
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

    public List<dynamic> PopularDestinations() {
        var topDestinations = (
            from t in db.trip
            join d in db.destination on t.destination.id equals d.id
            where t.isActive == true
            group t by t.destination.id into g
            select new {
                DestinationId = g.Key,
                Name = g.Select(g => g.destination.name).FirstOrDefault(),
                Country = g.Select(g => g.destination.country.name).FirstOrDefault(),
                Trips = g.Select(g => g.name),
                Count = g.Count(),
            }).ToList<dynamic>();  
            
        return topDestinations.OrderByDescending(o => o.Count).Take(5).ToList();
    }

    public List<dynamic> GetActiveUsers() {
        var topDestinations = (
            from tu in db.tripuser
            group tu by tu.user.id into g
            select new {
                UserId = g.Key,
                User = g.Select(g => g.user).FirstOrDefault(),
                Visited = g.Select(g => g.trip.destination.name).Distinct(),
                Count = g.Count(),
            }).ToList<dynamic>();  
            
        return topDestinations.OrderByDescending(o => o.Count).Take(5).ToList();
    }

    public List<dynamic> TopCountries() {
        var topDestinations = (
            from t in db.trip
            join d in db.destination on t.destination.id equals d.id
            join c in db.country on d.countryid equals c.id 
            where t.isActive == true
            group c by c.id into g
            select new {
                CountryId = g.Key,
                CountryName = g.Select(g => g.name).FirstOrDefault(),
                Count = g.Count()
            }).ToList<dynamic>();  
            
        return topDestinations.OrderByDescending(o => o.Count).Take(5).ToList();
    }

    public List<dynamic> AvgTripsPerMonth() {
        var tripData = (
            from t in db.trip
            where t.isActive == true
            group t by t.tripdate.Value.Month into g
            select new {
                Month = g.Key,
                Count = g.Count()
            }
        ).ToList<dynamic>();

        dynamic[] grouppedData = new dynamic[12];
        for (int i = 0; i < 12; i++) {
            grouppedData[i] = new {
                Month = i+1,
                MonthName = new DateTime(2015, i+1, 1).ToString("MMMM", CultureInfo.CreateSpecificCulture("en")),
                Count = 0
            };
        }

        foreach (dynamic trip in tripData) {
            grouppedData[trip.Month-1] = new {
                Month = trip.Month,
                MonthName = grouppedData[trip.Month-1].MonthName,
                Count = trip.Count
            };
        }

        return grouppedData.ToList<dynamic>();
    }

    public dynamic GetUserById(String userId) {
        var record = (from u in db.user select new {
            id = u.id,
            fullname = u.fullname,
            email = u.email,
            createdAt = u.createdAt,
            langCode = u.langCode,
            birthdate = u.birthdate
        }).FirstOrDefault(u => u.id == userId);
        return record;
    }
}