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