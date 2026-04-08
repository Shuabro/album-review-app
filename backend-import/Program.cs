using backend.Data;
using backend.Import;
using Microsoft.EntityFrameworkCore;

class Program
{
    static void Main(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("ERROR: CONNECTION_STRING environment variable is not set.");
            return;
        }
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
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
