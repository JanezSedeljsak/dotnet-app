using ContextWrapper;
using Newtonsoft.Json;
using Models;

namespace Services.Helpers;
public class Neki {}

public class Helpers {
    public static async Task<bool> SyncCountries() {
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
                name = region,
                createdAt = DateTime.Now,
                isActive = true
            };

            countries.Add(Tuple.Create(region, new Country {
                name = countryName,
                countryCode = countryCode
            }));
        }

        var countryData = new List<Country>();
        foreach (var country in countries) {
            var tempCountry = country.Item2;
            tempCountry.createdAt = DateTime.Now;
            tempCountry.isActive = true;
            if (regions.TryGetValue(country.Item1, out var tempRegion)) {
                tempCountry.region = tempRegion;
            }

            countryData.Add(tempCountry);
        }

        var regionData = regions.Values.ToList();
        using (var context = new TravelLog()) {
            context.region.AddRange(regionData);
            context.country.AddRange(countryData);
            context.SaveChanges();
        }
     
        return true;
    }
}