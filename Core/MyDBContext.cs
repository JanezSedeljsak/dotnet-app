using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.ContextWrapper;
public class TravelLogContext : DbContext {
    public TravelLogContext() {}
    public TravelLogContext(DbContextOptions<TravelLogContext> options) : base(options) {}
    public DbSet<Region> region { get; set; }
    public DbSet<Country> country { get; set; }
    public DbSet<User> user { get; set; }
    public DbSet<Destination> destination { get; set; }
    public DbSet<Trip> trip { get; set; }
    public DbSet<TripUser> tripuser { get; set; }
    public IQueryable<BaseModel> GetDbSet(string modelName) {
        return modelName switch {
            "countries" => country,
            "users" => user,
            "destinations" => destination
        };
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        var conf = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var conStr = conf.GetConnectionString("AppDb");
        optionsBuilder.UseMySql(conStr, new MySqlServerVersion(new Version(10, 5)));
    }
}


