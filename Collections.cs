using Models;

namespace Collections {
    public class AuthorCollection
    {
        public List<Author> Authors { get; set; }

        public List<Author> GetAuthors()
        {
            return new List<Author>()
            {
                new Author()
                {
                    AuthorId = 1,
                    FirstName ="Carson",
                    LastName ="Alexander",
                    BirthDate = DateTime.Parse("1985-09-01"),
                    Books = new List<Book>()
                    {
                        new Book { Title = "Introduction to Machine Learning"},
                        new Book { Title = "Advanced Topics in Machine Learning"},
                        new Book { Title = "Introduction to Computing"}
                    }
                },
                new Author()
                {
                    AuthorId = 2,
                    FirstName ="Meredith",
                    LastName ="Alonso",
                    BirthDate = DateTime.Parse("1970-09-01"),
                    Books = new List<Book>()
                    {
                        new Book { Title = "Introduction to Microeconomics"}
                    }
                },
                new Author()
                {
                    AuthorId = 3,
                    FirstName ="Arturo",
                    LastName ="Anand",
                    BirthDate = DateTime.Parse("1963-09-01"),
                    Books = new List<Book>()
                    {
                        new Book { Title = "Calculus I"},
                        new Book { Title = "Calculus II"}
                    }
                }
            };
        }
    }
}