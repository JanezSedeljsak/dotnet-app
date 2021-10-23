namespace Models;

public class User {
    public int id { get; set; }
    public string fullname { get; set; }
    public DateTime birthdate { get; set; }
}

public class Destination {
    public int id { get; set; }
    public string name { get; set; }
    public int height { get; set; }
}

public class Trip {
    public int id { get; set; }
    public Destination destination { get; set; }
    public DateTime tripdate { get; set; }
}

public class TripUser {
    public int id { get; set; }
    public Trip trip { get; set; }
    public User user { get; set; }
}


