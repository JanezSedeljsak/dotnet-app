namespace Core.Models;

public class BaseModel {
    public string id { get; set; }

    public DateTime createdAt { get; set; }

    public string createdBy { get; set; }

    public bool isActive { get; set; }

    public BaseModel() {
        createdAt = DateTime.Now;
        isActive = true;
        id = Guid.NewGuid().ToString();
        createdBy = "NOT_DEFINED";
    }

    public string GetShowAs() {
        return "GET_SHOW_AS_NOT_DEFINED";
    }

    public bool AllowEdit(String userId, bool isAdmin) {
        return isAdmin || userId == this.createdBy;
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
    public string regionid { get; set; }
    public string GetShowAs() => $"{(countryCode == null ? "XXX" : countryCode)} - {name}";
}

public class User : BaseModel {
    public string? fullname { get; set; }
    public string? email { get; set; }
    public string? password { get; set; }
    public DateTime? birthdate { get; set; }
    public bool? isAdmin { get; set; }
    public string? langCode { get; set; }
    public string GetShowAs() => fullname;
    public User() {
        isAdmin = false;
        langCode = "en";
    }
}

public class Destination : ModelWithName {
    public Country? country { get; set; }
    public string countryid { get; set; }
}

public class Trip : ModelWithName {
    public Destination? destination { get; set; }
    public string destinationid { get; set; }
    public DateTime? tripdate { get; set; }
    public List<TripUser>? tripUsers { get; set; }
}

public class TripUser : BaseModel {
    public int? rating { get; set; }
    public string? notes { get; set; }
    public Trip? trip { get; set; }
    public User? user { get; set; }
    public string tripid { get; set; }
    public string userid { get; set; }
}

public class UserUpdateModel : User {
    public string oldpassword { get; set; }
}

