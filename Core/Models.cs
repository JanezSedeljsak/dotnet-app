namespace Core.Models;

public class BaseModel {
    public string id { get; set; }

    public DateTime createdAt { get; set; }

    public bool isActive { get; set; }

    public BaseModel() {
        createdAt = DateTime.Now;
        isActive = true;
        id = Guid.NewGuid().ToString();
    }

    public string GetShowAs() {
        return "GET_SHOW_AS_NOT_DEFINED";
    }

    public void Deactivate() {
        isActive = false;
    }
}
public class ModelWithName : BaseModel {
    public string? name { get; set; }

    public string GetShowAs() => name;
}

public class Region : ModelWithName {}

public class Country : ModelWithName {
    public string? countryCode { get; set; }
    public Region? region { get; set; }
    public string GetShowAs() => $"{(countryCode == null ? "XXX" : countryCode)} - {name}";
}

public class User : BaseModel {
    public string? fullname { get; set; }
    public string? email { get; set; }

    public string? password { get; set; }
    public DateTime? birthdate { get; set; }
    public string GetShowAs() => fullname;
}

public class Destination : ModelWithName {
    public Country? country { get; set; }
}

public class Trip : ModelWithName {
    public Destination? destination { get; set; }
    public DateTime? tripdate { get; set; }
}

public class TripUser : BaseModel {
    public Trip? trip { get; set; }
    public User? user { get; set; }
}

public record AuthCredentials([Required] string email, [Required] string password);

