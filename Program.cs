using Models;
using Collections;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/heartbeat", async httpContext => {
    await httpContext.Response.WriteAsync("hello");
});

app.MapGet("/test-params", async httpContext => {
    var getParams = httpContext.Request.Query;
    var str = "";
    foreach (var paramKey in getParams.Keys) {
        str += (String.IsNullOrEmpty(str) ? "" : "\n") + $"{paramKey} -> {getParams[paramKey]}";
    }

    await httpContext.Response.WriteAsync(str);
});

app.MapGet("/author", (Func<Author>)(() => {
    return new Author() 
    {
        FirstName ="Carson",
        LastName ="Alexander",
        BirthDate = DateTime.Parse("1985-09-01"),
        Books = new List<Book>()
        {
            new Book { Title = "Introduction to Machine Learning"},
            new Book { Title = "Advanced Topics in Machine Learning"},
            new Book { Title = "Introduction to Computing"}
        }
    };
}));

app.MapGet("/authors", (Func<List<Author>>)(() => {
    return new AuthorCollection().GetAuthors();
}));

app.MapGet("/author/{id}", async(http) => {
    if (!http.Request.RouteValues.TryGetValue("id", out var id)) {
        http.Response.StatusCode = 400;
        return;
    }

    int idAsInt = Int32.Parse((string)id);
    await http.Response.WriteAsJsonAsync(new AuthorCollection()
            .GetAuthors()
            .FirstOrDefault(x => x.AuthorId == idAsInt));
});

app.Run();
