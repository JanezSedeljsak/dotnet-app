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
    dynamic AuthRegister(string fullname, DateTime birthdate, string email, string password);
    dynamic AuthLogin(string email, string password);
    Tuple<bool, User> GetAuth(AuthCredentials credentials);
}

public interface ITokenService {
    string BuildToken(string key, string issuer, User user);
}
