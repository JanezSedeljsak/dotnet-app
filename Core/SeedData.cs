using Core.IData;
using Core.ContextWrapper;

namespace Core.SeedData;

public class SeedRepository {
    public static async Task<bool> CreateMockData(TravelLogContext db, IHost app) {
        Console.WriteLine("----------------------\nStarting seed...\n----------------------");
        Console.WriteLine("----------------------\nFinished seed...\n----------------------");
        return true;
    }
}