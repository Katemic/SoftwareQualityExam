using LibraryAPI.Controllers;
using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Repositories;
using LibraryTestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibraryTestProject.IntegrationTests.Reservations
{
    [TestClass]
    [DoNotParallelize]
    public class ReservationIntegrationTests
    {
        private static TestDatabaseHelper CreateDatabaseHelper()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<ReservationIntegrationTests>()
                .Build();

            var connectionString = configuration.GetConnectionString("TestConnection")
                ?? throw new InvalidOperationException("Missing test database connection string.");

            return new TestDatabaseHelper(connectionString);
        }

        [TestInitialize]
        public async Task ResetDatabaseBeforeEachTest()
        {
            var databaseHelper = CreateDatabaseHelper();
            await databaseHelper.ResetAndSeedDatabaseAsync();
        }

        // Test cases:
        // Positive, tests queue number is equal to count of reservations of said item
        [TestMethod]
        public async Task ReservationScenario_CreateReservation_AssignsQueueNumberAsCountPlusOne()
        {
            // Arrange
            var databaseHelper = CreateDatabaseHelper();

            await using var context = databaseHelper.CreateContext();

            var reservationRepository = new ReservationRepository(context);
            var loanRepository = new LoanRepository(context);
            var service = new ReservationService(reservationRepository, loanRepository);

            var dto = new CreateReservationDto
            {
                ItemId = 1
            };

            var loanerId = 2;

            // Act
            var created = await service.CreateReservation(dto, loanerId);

            var saved = await context.Reservations.FirstAsync(x => x.ItemId == 1 && x.LoanerId == loanerId);

            var expectedQueue = (await context.Reservations.CountAsync(x => x.ItemId == 1));

            // Assert
            Assert.AreEqual(expectedQueue, saved.QueueNumber);
        }

        // Test case:
        // Negative, tests that reservation is rejected if loaner has unpaid fine
        [TestMethod]
        public async Task ReservationScenario_LoanerHasUnpaidFine_RejectsReservation()
        {
            // Arrange
            var databaseHelper = CreateDatabaseHelper();

            await using var context = databaseHelper.CreateContext();

            var reservationRepository = new ReservationRepository(context);
            var loanRepository = new LoanRepository(context);
            var service = new ReservationService(reservationRepository, loanRepository);

            var dto = new CreateReservationDto
            {
                ItemId = 1
            };

            var loanerId = TestIds.LoanerWithUnpaidFineId;

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateReservation(dto, loanerId));
        }

        // Test case:
        // Negative, tests that reservation is rejected if loaner has three active reservations
        [TestMethod]
        public async Task ReservationScenario_LoanerHasThreeReservations_RejectsNewOne()
        {
            // Arrange
            var databaseHelper = CreateDatabaseHelper();

            await using var context = databaseHelper.CreateContext();

            var reservationRepository = new ReservationRepository(context);
            var loanRepository = new LoanRepository(context);
            var service = new ReservationService(reservationRepository, loanRepository);

            var dto = new CreateReservationDto
            {
                ItemId = 12
            };

            var loanerId = TestIds.LoanerWithThreeReservationsId;

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateReservation(dto, loanerId));
        }

        // Test case:
        // Positive, tests that after deleting a reservation, the queue numbers of remaining reservations stay sequential
        // Tests queue numbers stay unique
        [TestMethod]
        public async Task QueueNumber_StaysSequential_AfterDeletion() {             
            // Arrange
            var databaseHelper = CreateDatabaseHelper();
            await using var context = databaseHelper.CreateContext();
            var reservationRepository = new ReservationRepository(context);
            var loanRepository = new LoanRepository(context);
            var service = new ReservationService(reservationRepository, loanRepository);

            // Act
            var user1 = service.GetAllLoanersReservation(TestIds.QueueNumber1).Result.First();
            var user2 = await service.DeleteReservation(TestIds.QueueNumber2[0], TestIds.QueueNumber2[1]);
            var user3 = service.GetAllLoanersReservation(TestIds.QueueNumber3).Result.First();

            // Assert
            Assert.AreEqual(user1.queue_number, 1);
            Assert.AreEqual(user3.queue_number, 2);
        }

        // Test case:
        // Positive, update works and updates to correct status
        [TestMethod]
        [DataRow(ReservationStatus.Fulfilled, "fulfilled")]
        [DataRow(ReservationStatus.ReadyForPickup, "ready for pickup")]
        public async Task CreateReservationAsync_Should_Save_Reservation(ReservationStatus status, string expectedStatus)
        {
            // Arrange
            var databaseHelper = CreateDatabaseHelper();
            await using var context = databaseHelper.CreateContext();
            var reservationRepository = new ReservationRepository(context);
            var loanRepository = new LoanRepository(context);
            var service = new ReservationService(reservationRepository, loanRepository);

            // Act
            // Update reservation
            await service.UpdateReservation(TestIds.UpdateReservationId, status);
            // Assert
            Assert.AreEqual(expectedStatus, (await reservationRepository.GetByIdAsync(TestIds.UpdateReservationId))?.Status);
        }
    }
}