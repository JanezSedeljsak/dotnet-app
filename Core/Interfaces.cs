namespace Core.Interfaces;
public interface IDataRepository {
    List<dynamic> GetCountries();
    List<dynamic> GetUsers();
    List<dynamic> GetDestinations();
    List<dynamic> GetTrips();
    Tuple<bool, BaseModel> GetModelById(string model, string id);
    Task<bool> InsertDestination(Destination d, string userId);
    Task<bool> InsertTrip(Trip t, string userId);
    Task<bool> InsertTripUser(TripUser tu, string userId);
    Task<bool> UpdateDestination(Destination d, string id, string userId, bool isAdmin);
    Task<bool> UpdateTrip(Trip t, string id, string userId, bool isAdmin);
    Task<bool> UpdateTripUser(TripUser tu, string id, string userId, bool isAdmin);
    Task<Tuple<bool, string>> SyncCountries();
    List<dynamic> GetShowAsRows(string modelName);
    Task<bool> DeactivateColumn(string modelName, string id);
}

public interface IAuthRepository {
    Tuple<bool, User> AuthRegister(User user);
    Tuple<bool, User, string> GetAuth(AuthCredentials credentials);
    User ParseUser(HttpContext http);
}

public interface ITokenService {
    string BuildToken(string key, string issuer, User user);
}

class StatusResponse {
    public bool status { get; set; }
    public string message { get; set; }
    public StatusResponse(bool s, string m) {
        status = s;
        message = m;
    }
}

public record AuthCredentials([Required] string email, [Required] string password);