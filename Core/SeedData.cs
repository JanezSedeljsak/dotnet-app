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

        db.destination.AddRange(mockDestinations);
        db.user.AddRange(mockUsers);
        db.SaveChanges();
        Console.WriteLine("----------------------\nFinished seed for users and destinations...\n----------------------");

        var mockTrips = new List<Trip> {
            new Trip {
                name = "Potovanje v Rim",
                tripdate = new DateTime(2020, 1, 1),
                destination = db.destination.Single(c => c.name == "The Colosseum, Rome"),
            },
            new Trip {
                name = "Korona polet v Egipt",
                tripdate = new DateTime(2020, 5, 1),
                destination = db.destination.Single(c => c.name == "Pyramids of Giza"),
            },
            new Trip {
                name = "Ogled velike jabke",
                tripdate = new DateTime(2019, 3, 6),
                destination = db.destination.Single(c => c.name == "Statue of Liberty, New York City"),
            },
            new Trip {
                name = "Tradicionalni potep v Rim",
                tripdate = new DateTime(2021, 1, 1),
                destination = db.destination.Single(c => c.name == "The Colosseum, Rome"),
            }
        };

        db.trip.AddRange(mockTrips);
        db.SaveChanges();
        Console.WriteLine("----------------------\nFinished seed for trips...\n----------------------");

        var mockTripUsers = new List<TripUser> {
            new TripUser {
               trip = db.trip.Single(c => c.name == "Potovanje v Rim"),
               user = db.user.Single(c => c.fullname == "Janez Sedeljsak"),
            },
            new TripUser {
               trip = db.trip.Single(c => c.name == "Potovanje v Rim"),
               user = db.user.Single(c => c.fullname == "Lorem Ipsum"),
            },
            new TripUser {
               trip = db.trip.Single(c => c.name == "Korona polet v Egipt"),
               user = db.user.Single(c => c.fullname == "John Doe"),
            },
            new TripUser {
               trip = db.trip.Single(c => c.name == "Ogled velike jabke"),
               user = db.user.Single(c => c.fullname == "Lorem Ipsum"),
            },
            new TripUser {
               trip = db.trip.Single(c => c.name == "Ogled velike jabke"),
               user = db.user.Single(c => c.fullname == "John Doe"),
            },
            new TripUser {
               trip = db.trip.Single(c => c.name == "Ogled velike jabke"),
               user = db.user.Single(c => c.fullname == "Janez Sedeljsak"),
            },
            new TripUser {
               trip = db.trip.Single(c => c.name == "Tradicionalni potep v Rim"),
               user = db.user.Single(c => c.fullname == "Janez Sedeljsak"),
            },
            new TripUser {
               trip = db.trip.Single(c => c.name == "Tradicionalni potep v Rim"),
               user = db.user.Single(c => c.fullname == "John Doe"),
            }
        };

        db.tripuser.AddRange(mockTripUsers);
        db.SaveChanges();
        Console.WriteLine("----------------------\nFinished seed for user-trips...\n----------------------");
        Console.WriteLine("----------------------\nFinished seed...\n----------------------");
        return true;
    }
}