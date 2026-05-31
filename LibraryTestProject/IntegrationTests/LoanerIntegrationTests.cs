using LibraryAPI.DTOs;
using LibrarySQLBackend.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

[TestClass]
public class LoanerIntegrationTests
{
    private static TestDatabaseHelper CreateDatabaseHelper()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<LoanerIntegrationTests>()
            .Build();

        var connectionString = configuration.GetConnectionString("TestConnection")
            ?? throw new InvalidOperationException("Missing test database connection string.");

        return new TestDatabaseHelper(connectionString);
    }
    private static readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("https://localhost:7019")
    };

    [TestInitialize]
    public async Task ResetDatabaseBeforeEachTest()
    {
        var databaseHelper = CreateDatabaseHelper();

        await databaseHelper.ResetAndSeedDatabaseAsync();
    }
    private static AppDbContext CreateDbContext()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<LoanerIntegrationTests>()
            .Build();

        var connectionString = configuration.GetConnectionString("TestConnection")
            ?? throw new InvalidOperationException("Missing test database connection string.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString))
            .Options;

        return new AppDbContext(options);
    }

    [TestMethod] // brug service istedet for controller og tilføj ikke kunnel logge ind
    public async Task Loaner_Lifecycle_Register_Login_Update_And_Delete()
    {
        // Arrange
        var registerDto = new RegisterLoanerDto
        {
            FirstName = "John",
            LastName = "Doe",
            Cpr = "0101901234",
            Tlf = "+45 12345678",
            Email = $"john{Guid.NewGuid()}@test.com",
            Password = "Password123"
        };

        // Register
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                registerDto);

        registerResponse.EnsureSuccessStatusCode();

        // Login
        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginDto
                {
                    Email = registerDto.Email,
                    Password = registerDto.Password
                });

        loginResponse.EnsureSuccessStatusCode();

        var auth =
            await loginResponse.Content
                .ReadFromJsonAsync<AuthResponseDto>();

        Assert.IsNotNull(auth);
        Assert.IsTrue(auth.Success);
        Assert.IsNotNull(auth.Token);
        Assert.IsNotNull(auth.User);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                auth.Token);

        int loanerId = auth.User.Id;

        // Update
        var updateDto = new UpdateLoanerDto
        {
            FirstName = "NewJane",
            LastName = "NewDoe",
            Email = $"Newjane{Guid.NewGuid()}@test.com",
            Tlf = "+45 87654321"
        };

        var updateResponse =
            await _client.PutAsJsonAsync(
                $"/api/loaner/{loanerId}",
                updateDto);

        updateResponse.EnsureSuccessStatusCode();

        // Verify update
        using (var context = CreateDbContext())
        {
            var loaner = await context.Loaners.FindAsync(loanerId);

            Assert.IsNotNull(loaner);
            Assert.AreEqual("NewJane", loaner.FirstName);
            Assert.AreEqual(updateDto.Email, loaner.Email);
        }

        // Delete
        var deleteResponse =
            await _client.DeleteAsync(
                $"/api/loaner/{loanerId}");

        Assert.AreEqual(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        // Verify delete
        using (var context = CreateDbContext())
        {
            var deletedLoaner = await context.Loaners.FindAsync(loanerId);

            Assert.IsNull(deletedLoaner);
        }
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _client.Dispose();
    }
}