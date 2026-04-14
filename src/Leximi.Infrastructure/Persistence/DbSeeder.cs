using Leximi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Leximi.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(LeximiDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return; // Already seeded

        var categories = new List<Category>
        {
            new Category { Name = "Matematyka", Description = "Arytmetyka, algebra, geometria i więcej" },
            new Category { Name = "Języki obce", Description = "Angielski, Niemiecki, Francuski i inne języki" },
            new Category { Name = "Historia", Description = "Historia Polski i świata" },
            new Category { Name = "Biologia", Description = "Biologia komórki, genetyka, ekologia" },
            new Category { Name = "Chemia", Description = "Chemia organiczna, nieorganiczna, analityczna" },
            new Category { Name = "Fizyka", Description = "Mechanika, elektryczność, optyka i więcej" },
            new Category { Name = "Informatyka", Description = "Programowanie, algorytmy, bazy danych" },
            new Category { Name = "Geografia", Description = "Geografia fizyczna i społeczno-ekonomiczna" },
            new Category { Name = "Literatura", Description = "Lektury szkolne, analiza tekstu, epoki literackie" },
            new Category { Name = "Inne", Description = "Pozostałe kategorie i tematy ogólne" },
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }
}
