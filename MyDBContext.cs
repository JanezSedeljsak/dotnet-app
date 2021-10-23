using Microsoft.EntityFrameworkCore;
using Models;
namespace MyDbContext;
public class LibraryDbContext : DbContext {
    public LibraryDbContext() {}
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) {}

    public DbSet<Book> Book { get; set; }
    public DbSet<Author> Author { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        var conf = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var conStr = conf.GetConnectionString("AppDb");
        optionsBuilder.UseMySql(conStr, new MySqlServerVersion(new Version(10, 5)));
    }
}


