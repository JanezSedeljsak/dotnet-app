namespace Core.IData;
public interface IDataRepository {
    List<dynamic> GetCountries();
    List<dynamic> GetUsers();
    List<dynamic> GetDestinations();
    List<dynamic> GetTrips();
    Tuple<bool, BaseModel> GetModelById(string model, string id);
    Task<bool> InsertDestination(Destination d);
    Task<bool> InsertTrip(Trip t);
    Task<bool> InsertTripUser(TripUser tu);
    Task<bool> UpdateDestination(Destination d, string id);
    Task<bool> UpdateTrip(Trip t, string id);
    Task<bool> UpdateTripUser(TripUser tu, string id);
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
