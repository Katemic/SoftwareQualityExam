using LibraryTestUtilities;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public TestController(
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        [HttpPost("reset-database")]
        public async Task<IActionResult> ResetDatabase()
        {
            if (!_environment.IsEnvironment("Testing"))
            {
                return NotFound();
            }

            var connectionString = _configuration.GetConnectionString("TestConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return Problem("Missing database connection string.");
            }

            if (!connectionString.Contains("mydb_test", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Refusing to reset database because connection string does not point to mydb_test."
                });
            }

            var databaseHelper = new TestDatabaseHelper(connectionString);
            try
            {
                await databaseHelper.ResetAndSeedDatabaseAsync();

                return Ok(new { message = "Test database reset and seeded." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error resetting database.", details = ex.Message });
            }
        }
    }
}
