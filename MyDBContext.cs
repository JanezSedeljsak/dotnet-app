using Models;
using Microsoft.EntityFrameworkCore;
namespace ContextWrapper;
public class TravelLog : DbContext {
    public TravelLog() {}
    public TravelLog(DbContextOptions<TravelLog> options) : base(options) {}
    public DbSet<Region> region { get; set; }
    public DbSet<Country> country { get; set; }
    public DbSet<User> user { get; set; }
    public DbSet<Destination> destination { get; set; }
    public DbSet<Trip> trip { get; set; }
    public DbSet<TripUser> tripuser { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        var conf = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var conStr = conf.GetConnectionString("AppDb");
        optionsBuilder.UseMySql(conStr, new MySqlServerVersion(new Version(10, 5)));
    }
}


