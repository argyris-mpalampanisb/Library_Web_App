using Library_Web_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Library_Web_App.Data;

public static class DbInitializer
{
    public static void Initialize(LibraryDbContext context)
    {
        context.Database.Migrate();

        if (!context.Books.Any())
        {
            context.Books.AddRange(
                new Book
                {
                    Title = "IT",
                    Author = "Stephen King",
                    Description = "A group of childhood friends must reunite to defeat an ancient evil that takes the form of a clown in a small town in Maine."
                },
                new Book
                {
                    Title = "Pet Sematary",
                    Author = "Stephen King",
                    Description = "A family discovers a buried ground behind their house that brings the dead back to life — but not always in a good way."
                },
                new Book
                {
                    Title = "The Outsider",
                    Author = "Stephen King",
                    Description = "A detective investigates the murder of a young boy, with the prime suspect being the town's beloved baseball coach."
                },
                new Book
                {
                    Title = "The Institute",
                    Author = "Stephen King",
                    Description = "A young boy with extraordinary psychic abilities is recruited into a secret government facility for gifted children."
                },
                new Book
                {
                    Title = "Insomnia",
                    Author = "Stephen King",
                    Description = "A small Alaskan town is cut off from the world when a volcano erupts, and its residents begin to lose their minds as the sun never sets."
                }
            );
            context.SaveChanges();
        }
    }
}