using LibraryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Linq;
using LibraryAPI;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Expose mocks so tests can configure behavior
    public Mock<IReservationService> ReservationServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // ----------------------------------------------------
            // 1. REMOVE real service registration
            // ----------------------------------------------------
            services.RemoveAll(typeof(IReservationService));

            // ----------------------------------------------------
            // 2. ADD mocked service
            // ----------------------------------------------------
            services.AddSingleton(ReservationServiceMock.Object);

            // ----------------------------------------------------
            // 3. ADD TEST AUTHENTICATION (bypasses [Authorize])
            // ----------------------------------------------------
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "Test", options => { });

            // Set default auth scheme so [Authorize] works
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            });
        });
    }
}