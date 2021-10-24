using System.ComponentModel;
using ContextWrapper;
namespace Models;

public class BaseModel {
    public int id { get; set; }

    [DefaultValue("getutcdate()")]
    public DateTime createdAt { get; set; }

    [DefaultValue("true")]
    public bool isActive { get; set; }

    public BaseModel() {
        createdAt = DateTime.Now;
        isActive = true;
    }
}
public class ModelWithName : BaseModel {
    public string? name { get; set; }
}

public class Region : ModelWithName {}

public class Country : ModelWithName {
    public string? countryCode { get; set; }
    public Region? region { get; set; }

    public static List<Country> get(TravelLog context) => context.country.ToList();
    public static Country getCountryByName(TravelLog context, string countryName) => 
        context.country.Where(c => c.name == countryName).FirstOrDefault();
}

public class User : BaseModel {
    public string? fullname { get; set; }
    public DateTime? birthdate { get; set; }
}

public class Destination : ModelWithName {
    public Country? country { get; set; }
}

public class Trip : BaseModel {
    public Destination? destination { get; set; }
    public DateTime? tripdate { get; set; }
}

public class TripUser : BaseModel {
    public Trip? trip { get; set; }
    public User? user { get; set; }
}


