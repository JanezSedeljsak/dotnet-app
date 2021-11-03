using Core.Models;

namespace Core.IData;
public interface IDataRepository {
    List<dynamic> GetCountries();
    List<dynamic> GetUsers();
    List<dynamic> GetDestinations();
    List<dynamic> GetTrips();
    Country GetCountryByName(string name);
    Task<Tuple<bool, string>> SyncCountries();
}

public interface IAuthRepository {
    Tuple<bool, User> AuthRegister(User user);
    Tuple<bool, User, string> GetAuth(AuthCredentials credentials);
}

public interface ITokenService {
    string BuildToken(string key, string issuer, User user);
}
