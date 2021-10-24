namespace Services.Helpers;
using Newtonsoft.Json;

public class Neki {}

public class Helpers {
    public static async Task<bool> SyncCountries() {
        var client = new HttpClient();
        var data = await client.GetStringAsync("https://restcountries.com/v3.1/all");
        var response = JsonConvert.DeserializeObject<dynamic>(data);
        foreach (var country in response) {
            Console.WriteLine(country);
        }
        //Console.WriteLine(obj.ToString());
        return true;
    }
}