using Core.Models;

namespace Core.IData;
public interface IDataRepository {
    List<Country> GetCountries();
    Country GetCountryByName(string name);
    Task<bool> SyncCountries();
}
