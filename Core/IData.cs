using Core.Models;

namespace Core.IData;
public interface IDataRepository {
    List<dynamic> GetCountries();
    Country GetCountryByName(string name);
    Task<Tuple<bool, string>> SyncCountries();
}
