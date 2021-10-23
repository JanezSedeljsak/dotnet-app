namespace Services.Helpers;
using Newtonsoft.Json;

public class Helpers {
    public static async Task<bool> SyncCountries() {
        var client = new HttpClient();
        var data = await client.GetStringAsync("https://api.first.org/data/v1/countries");
        dynamic obj = JsonConvert.DeserializeObject<dynamic>(data);
        return obj.status == "OK";
    }
}