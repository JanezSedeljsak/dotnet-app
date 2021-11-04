using Newtonsoft.Json;

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

    public async Task<bool> DeactivateColumn(string modelName, string id) {
        var dbRow = db.GetDbSet(modelName).FirstOrDefault(row => row.id == id);
        dbRow.Deactivate();
        return (await db.SaveChangesAsync()) > 0;
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
                BirthDate = u.birthdate,
                Rating = tu.rating,
                Notes = tu.notes
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
            var userList = tripUsers.Where(u => u.TripId == row.Id).ToList<dynamic>();
            var ratingSum = .0;
            var ratingCount = 0;

            foreach (var tripUser in userList) {
                if (tripUser.Rating != null) {
                    ratingSum += tripUser.Rating;
                    ratingCount += 1;
                }
            }

            data.Add(new {
                TripName = row.TripName,
                TripDate = row.TripDate,
                Destination = row.Destination,
                CountryName = row.CountryName,
                RegionName = row.RegionName,
                CountryCode = row.CountryCode,
                AvgRating = ratingCount != 0 ? ratingSum / (double)ratingCount : -1,
                UserList = userList
            });
        }

        return data;
    }

    public async Task<bool> InsertDestination(Destination d) {
        db.destination.AddRange(d);
        return (await db.SaveChangesAsync()) > 0;
    }

    public async Task<bool> InsertTrip(Trip t) {
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

    public async Task<bool> InsertTripUser(TripUser tu) {
        db.tripuser.AddRange(tu);
        return (await db.SaveChangesAsync()) > 0;
    }

    public async Task<bool> UpdateDestination(Destination d, string id) {
        // @TODO add adming privellages for destination editing || if createdby me
        var record = db.destination.FirstOrDefault(r => r.id == id);
        if (record == null) return false;
        record.name = d.name != null ? d.name : record.name;
        return (await db.SaveChangesAsync()) > 0;
    }

    public async Task<bool> UpdateTrip(Trip t, string id) {
        var record = db.trip.FirstOrDefault(r => r.id == id);
        if (record == null) return false;
        record.name = t.name != null ? t.name : record.name;
        record.tripdate = t.tripdate != null ? t.tripdate : record.tripdate;
        if ((await db.SaveChangesAsync()) <= 0) {
            return false;
        }

        if (record.tripUsers != null) {
            foreach (var tripUser in t.tripUsers) {
                bool tmpStatus;
                if (tripUser.id != null) {
                    tmpStatus = await this.UpdateTripUser(tripUser, tripUser.id);
                } else {
                    tmpStatus = await this.InsertTripUser(tripUser);
                }

                if (!tmpStatus) {
                    return false;
                }
            }
        }

        return true;
    }

    public async Task<bool> UpdateTripUser(TripUser tu, string id) {
        var record = db.tripuser.FirstOrDefault(r => r.id == id);
        if (record == null) return false;
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
}