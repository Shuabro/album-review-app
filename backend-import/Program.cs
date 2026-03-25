using backend.Data;
using backend.Import;
using Microsoft.EntityFrameworkCore;

class Program
{
    static void Main(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=localhost;Database=AlbumReviewDb;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        using var context = new AppDbContext(options);
        var importer = new AlbumImporter(context);
        try
        {
            importer.Import("../backend/Album Review.xlsx"); // Adjust path if needed
            Console.WriteLine("Import complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Import failed: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }
}
