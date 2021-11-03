using Core.Models;

namespace Core.IData;
public interface IDataRepository {
    List<dynamic> GetCountries();
    List<dynamic> GetUsers();
    List<dynamic> GetDestinations();
    List<dynamic> GetTrips();
    Country GetCountryByName(string name);
    Task<Tuple<bool, string>> SyncCountries();
    List<dynamic> GetShowAsRows(string modelName);
    bool DeactivateColumn(string modelName, string id);
}

public interface IAuthRepository {
    Tuple<bool, User> AuthRegister(User user);
    Tuple<bool, User, string> GetAuth(AuthCredentials credentials);
    User ParseUser(HttpContext http);
}

public interface ITokenService {
    string BuildToken(string key, string issuer, User user);
}
