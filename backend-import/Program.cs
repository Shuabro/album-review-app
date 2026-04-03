using backend.Data;
using backend.Import;
using Microsoft.EntityFrameworkCore;

class Program
{
    static void Main(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=AlbumReviewDb;Username=postgres;Password=Montezuma1969")
            .Options;

        using var context = new AppDbContext(options);
        var importer = new AlbumImporter(context);
        try
        {
            importer.Import(@"C:\src\album-review-app\backend\powersrankings.xlsx"); // Adjust path if needed
            Console.WriteLine("Import complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Import failed: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }
}
