using Core.Models;

namespace Core.IData;
public interface IDataRepository {
    List<dynamic> GetCountries();
    List<dynamic> GetUsers();
    List<dynamic> GetDestinations();
    Country GetCountryByName(string name);
    Task<Tuple<bool, string>> SyncCountries();
}
