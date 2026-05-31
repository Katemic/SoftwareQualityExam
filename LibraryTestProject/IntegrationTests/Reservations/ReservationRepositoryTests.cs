using LibrarySQLBackend.Context;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories;
namespace LibraryTestProject.IntegrationTests.Reservations;
[TestClass]
public class ReservationRepositoryTests
{
    private static AppDbContext _context = null!;
    private static ReservationRepository _repository = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext testContext)
    {
        _context = await TestDatabase.CreateFreshDatabase();

        _repository = new ReservationRepository(_context);
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
        }
    }

    [TestMethod]
    public async Task CreateReservationAsync_Should_Save_Reservation()
    {
        var reservation = new Reservation
        {
            ItemId = 1,
            LoanerId = 1,
            QueueNumber = 1,
            Status = "pending"
        };

        await _repository.CreateReservationAsync(reservation);

        var saved =
            await _context.Reservations.FindAsync(reservation.Id);

        Assert.IsNotNull(saved);
    }
}