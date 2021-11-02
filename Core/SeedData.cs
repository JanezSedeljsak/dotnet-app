using Core.IData;
using Core.Models;
using Core.DataRep;
using Core.ContextWrapper;
using System.Linq;
using BC = BCrypt.Net.BCrypt;

namespace Core.SeedData;

public class SeedRepository {
    public static async Task<bool> CreateMockData(TravelLogContext db, IHost app) {
        Console.WriteLine("----------------------\nStarting seed...\n----------------------");

        var mockDestinations = new List<Destination> {
            new Destination {
                country = db.country.Single(c => c.name == "France"),
                name = "Eiffel Tower, Paris"
            },
            new Destination {
                country = db.country.Single(c => c.name == "Italy"),
                name = "The Colosseum, Rome"
            },
            new Destination {
                country = db.country.Single(c => c.name == "United States"),
                name = "Statue of Liberty, New York City"
            },
            new Destination {
                country = db.country.Single(c => c.name == "Peru"),
                name = "Machu Picchu"
            },
            new Destination {
                country = db.country.Single(c => c.name == "Egypt"),
                name = "Pyramids of Giza"
            }
        };

        var mockUsers = new List<User> {
            new User {
                fullname = "Janez Sedeljsak",
                email = "janez.sedeljsak@gmail.com",
                password = BCrypt.Net.BCrypt.HashPassword("janez123"),
                birthdate = new DateTime(2000, 12, 12)
            },
            new User {
                fullname = "Lorem Ipsum",
                email = "lorem.ipsum@gmail.com",
                password = BCrypt.Net.BCrypt.HashPassword("lorem123"),
                birthdate = new DateTime(1970, 10, 3)
            },
            new User {
                fullname = "John Doe",
                email = "john.doe@gmail.com",
                password = BCrypt.Net.BCrypt.HashPassword("john123"),
                birthdate = new DateTime(2000, 1, 1)
            }
        };

        db.user.AddRange(mockUsers);
        db.destination.AddRange(mockDestinations);
        db.SaveChanges();
        
        Console.WriteLine("----------------------\nFinished seed...\n----------------------");
        return true;
    }
}