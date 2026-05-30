using LibrarySQLBackend.Context;
using LibraryTestProject.IntegrationTests.Reservations;
using Microsoft.EntityFrameworkCore;

public static class TestDatabase
{
    private const string ConnectionString =
        "Server=localhost;Port=3309;Database=mydb;User=root;Password=mysecretpassword;";

    public static async Task<AppDbContext> CreateFreshDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                ConnectionString,
                ServerVersion.AutoDetect(ConnectionString))
            .Options;

        var context = new AppDbContext(options);
        return context;
    }
}