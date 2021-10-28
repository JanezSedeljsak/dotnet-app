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
                CountryName = c.name,
                RegionName = r.name,
                CountryCode = c.countryCode,
            }).ToList<dynamic>();

        return data;
    }

    public Country GetCountryByName(string name) {
        return db.country.Single(c => c.name == name);
    }

    public async Task<bool> SyncCountries() {
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
     
        return true;
    }
}