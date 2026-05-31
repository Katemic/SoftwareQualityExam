using LibraryAPI.Controllers;
using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using Moq;

namespace LibraryTestProject.Services
{
    [TestClass]
    public sealed class ReservationServiceTests
    {
        // Test cases for blackbox Status

        [TestMethod]
        public async Task ValidateStatusWhenCreatedTest()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var service = new ReservationService(repoMock.Object);

            repoMock.Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
                    .ReturnsAsync(true);

            repoMock.Setup(x => x.LoanerExistsAsync(It.IsAny<int>()))
                    .ReturnsAsync(true);

            repoMock.Setup(x => x.GetByLoanerId(It.IsAny<int>()))
                    .ReturnsAsync(new List<Reservation>());
            repoMock.Setup(x => x.ItemIsUnavailable(It.IsAny<int>()))
                    .ReturnsAsync(() => true);

            // IMPORTANT: empty queue
            repoMock.Setup(x => x.GetByItemIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new List<Reservation>());

            var dto = new CreateReservationDto
            {
                ItemId = 1
            };

            // Act
            var result = await service.CreateReservation(dto, 1);

            Assert.AreEqual(result.Status, "pending");
        }

        // Skal det her være to unit tests?
        [TestMethod]
        public async Task UpdateReservation_ShouldUpdateStatusCorrectly()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();

            var reservation = new Reservation
            {
                Id = 1,
                Status = "pending"
            };
            var reservation2 = new Reservation
            {
                Id = 2,
                Status = "pending"
            };

            repoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(reservation);
            repoMock.Setup(x => x.GetByIdAsync(2))
                .ReturnsAsync(reservation2);
            repoMock.Setup(x => x.UpdateAsync(It.IsAny<Reservation>()))
                .Returns(Task.CompletedTask);

            Reservation updatedReservation = null;

            repoMock.Setup(x => x.UpdateAsync(It.IsAny<Reservation>()))
                .Callback<Reservation>(r => updatedReservation = r)
                .Returns(Task.CompletedTask);

            var service = new ReservationService(repoMock.Object);

            // Act
            var result = await service.UpdateReservation(1, ReservationStatus.ReadyForPickup);
            var result2= await service.UpdateReservation(2, ReservationStatus.Fulfilled);

            // Assert - verify enum conversion
            Assert.AreEqual("ready for pickup", result.Status);
            Assert.AreEqual("fulfilled", result2.Status);
        }

        //[DataRow("cancelled", false)]
        //[DataRow("Pending", false)]
        //[DataRow("thisispending", false)]
        //[DataRow(" ", false)]
        //[DataRow(null, false)]

        // Test cases for blackbox Queue number

        //[TestMethod]
        //[DataRow(1.5, false)]
        //[DataRow("01/01/2001", false)]
        //[DataRow("Unique", true)]
        //[DataRow("Non-Unique", false)]
        //[DataRow(" ", false)]
        //[DataRow(01, false)]
        //[DataRow("One", false)]

        //[DataRow(-1, false)]
        //[DataRow(-2, false)]
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(98)]
        [DataRow(99)]
        public async Task CreateReservation_ShouldSucceed_WhenQueueIsNotFull(int reservationCount)
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var service = new ReservationService(repoMock.Object);

            repoMock.Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
                    .ReturnsAsync(true);

            repoMock.Setup(x => x.LoanerExistsAsync(It.IsAny<int>()))
                    .ReturnsAsync(true);

            repoMock.Setup(x => x.GetByLoanerId(It.IsAny<int>()))
                    .ReturnsAsync(new List<Reservation>());
            repoMock.Setup(x => x.ItemIsUnavailable(It.IsAny<int>()))
                    .ReturnsAsync(() => true);
            var reservations = Enumerable.Range(1, reservationCount)
                .Select(i => new Reservation { QueueNumber = i })
                .ToList();

            repoMock.Setup(x => x.GetByItemIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(reservations);


            // Act
            var result = await service.CreateReservation(new CreateReservationDto
            {
                ItemId = 1
            }, 1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(reservationCount + 1, result.queue_number);
            Assert.AreEqual("pending", result.Status);
        }

        [TestMethod]
        [DataRow(100)]
        [DataRow(101)]
        public async Task CreateReservation_ShouldFail_WhenQueueIsFull(int reservationCount)
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var service = new ReservationService(repoMock.Object);

            repoMock.Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
                    .ReturnsAsync(true);

            repoMock.Setup(x => x.LoanerExistsAsync(It.IsAny<int>()))
                    .ReturnsAsync(true);

            repoMock.Setup(x => x.GetByLoanerId(It.IsAny<int>()))
                    .ReturnsAsync(new List<Reservation>());

            var reservations = Enumerable.Range(1, reservationCount)
                .Select(i => new Reservation { QueueNumber = i })
                .ToList();

            repoMock.Setup(x => x.GetByItemIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(reservations);
            repoMock.Setup(x => x.ItemIsUnavailable(It.IsAny<int>()))
                    .ReturnsAsync(() => true);

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateReservation(new CreateReservationDto(), 1));
        }

        [TestMethod]
        public async Task CreateReservation_ShouldSetQueueNumberTo1_WhenQueueIsEmpty()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var service = new ReservationService(repoMock.Object);

            repoMock.Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
                    .ReturnsAsync(true);

            repoMock.Setup(x => x.LoanerExistsAsync(It.IsAny<int>()))
                    .ReturnsAsync(true);

            repoMock.Setup(x => x.GetByLoanerId(It.IsAny<int>()))
                    .ReturnsAsync(new List<Reservation>());
            repoMock.Setup(x => x.ItemIsUnavailable(It.IsAny<int>()))
                    .ReturnsAsync(() => true);

            // IMPORTANT: empty queue
            repoMock.Setup(x => x.GetByItemIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new List<Reservation>());

            var dto = new CreateReservationDto
            {
                ItemId = 1
            };

            // Act
            var result = await service.CreateReservation(dto, 1);
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.queue_number);
        }

        [TestMethod]
        public async Task CreateReservation_ShouldGenerateSequentialQueueNumbers_AndIncrementLoanerId()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var service = new ReservationService(repoMock.Object);

            var reservations = new List<Reservation>();

            repoMock.Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
                    .ReturnsAsync(true);

            repoMock.Setup(x => x.LoanerExistsAsync(It.IsAny<int>()))
                    .ReturnsAsync(true);

            repoMock.Setup(x => x.GetByLoanerId(It.IsAny<int>()))
                    .ReturnsAsync(new List<Reservation>());

            repoMock.Setup(x => x.GetByItemIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(() => reservations);
            repoMock.Setup(x => x.ItemIsUnavailable(It.IsAny<int>()))
                    .ReturnsAsync(() => true);
            repoMock.Setup(x => x.CreateReservationAsync(It.IsAny<Reservation>()))
                    .Callback<Reservation>(r => reservations.Add(r));

            // Act + Assert
            for (int i = 1; i <= 4; i++)
            {
                var dto = new CreateReservationDto
                {
                    ItemId = 1
                };

                var result = await service.CreateReservation(dto, i);

                Assert.AreEqual(i, result.queue_number);
            }
        }
        // Test cases for blackbox Buisness rules
        /* 
         * Test cases

        User has an unpaid fine when creating a reservation
        Invalid - Deny reservation and give error message
        User is not logged in
        Invalid - Deny reservation and give error message
        User cancels one of their reservations
        Valid - Remove reservation and give confirmation
        User tries to cancel someone else's reservation
        Invalid - Deny cancellation and give error message

        Item is available
        Invalid - Can’t create a reservation for an available item, make loan instead

        */
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public async Task TestAmountOfReservationsValid(int currentReservations)
        {
            // Arrange
            var reservationRepositoryMock = new Mock<IReservationRepository>();
            var fakeReservations = Enumerable
                .Range(0, currentReservations)
                .Select(i => new Reservation())
                .ToList();
            reservationRepositoryMock
            .Setup(x => x.GetByItemIdAsync(It.IsAny<int>()))
             .ReturnsAsync(new List<Reservation>());
            reservationRepositoryMock
            .Setup(x => x.GetByLoanerId(It.IsAny<int>()))
            .Returns(Task.FromResult(fakeReservations));
            reservationRepositoryMock.Setup(x => x.ItemIsUnavailable(It.IsAny<int>()))
                    .ReturnsAsync(() => true);
            reservationRepositoryMock
            .Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
        .ReturnsAsync(true);

            reservationRepositoryMock
                .Setup(x => x.LoanerExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            var service = new ReservationService(reservationRepositoryMock.Object);
            // Act
            var dto = new CreateReservationDto
            {
                ItemId = 1
            };
            var result = await service.CreateReservation(dto, 1);
            // Assert
            Assert.IsNotNull(result);
        }
        [TestMethod]
        [DataRow(3)]
        [DataRow(4)]
        public async Task TestAmountOfReservationsInvalid(int currentReservations)
        {
            // Arrange
            var reservationRepositoryMock = new Mock<IReservationRepository>();

            var fakeReservations = Enumerable
                .Range(0, currentReservations)
                .Select(i => new Reservation())
                .ToList();

            reservationRepositoryMock
            .Setup(x => x.GetByLoanerId(It.IsAny<int>()))
            .Returns(Task.FromResult(fakeReservations));
            reservationRepositoryMock
            .Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
            reservationRepositoryMock.Setup(x => x.ItemIsUnavailable(It.IsAny<int>()))
                    .ReturnsAsync(() => true);

            reservationRepositoryMock
                .Setup(x => x.LoanerExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            var service = new ReservationService(reservationRepositoryMock.Object);

            // Act
            var dto = new CreateReservationDto
            {
                ItemId = 1
            };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            service.CreateReservation(dto, 1));
        }

        [TestMethod]
        public async Task CreateReservation_ShouldFAIL_WhenUserAlreadyReservedItem()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();

            repoMock.Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            repoMock.Setup(x => x.LoanerExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            repoMock.Setup(x => x.GetByLoanerId(It.IsAny<int>()))
                .ReturnsAsync(new List<Reservation>());

            repoMock.Setup(x => x.GetByItemIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Reservation>
                {
            new Reservation
            {
                LoanerId = 1,
                QueueNumber = 1
            }
                });
            repoMock.Setup(x => x.ItemIsUnavailable(It.IsAny<int>()))
                    .ReturnsAsync(() => true);
            var service = new ReservationService(repoMock.Object);

            var dto = new CreateReservationDto
            {
                ItemId = 1
            };

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                service.CreateReservation(dto, 1));
        }

        //[TestMethod]
        //public async Task CancelReservation_ShouldThrow_WhenNotOwner()
        //{
        //    var repoMock = new Mock<IReservationRepository>();

        //    repoMock.Setup(x => x.GetReservationByIdAsync(1))
        //        .ReturnsAsync(new Reservation
        //        {
        //            Id = 1,
        //            LoanerId = 999
        //        });

        //    var service = new ReservationService(repoMock.Object);

        //    await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(() =>
        //        service.DeleteReservation(1, 1));
        //}

        [TestMethod]
        public async Task User_Cancels_NonexistentRerservation()
        {
            var repoMock = new Mock<IReservationRepository>();

            repoMock.Setup(x => x.GetByLoanerId(1))
                .ReturnsAsync(new List<Reservation>());

            var service = new ReservationService(repoMock.Object);

            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () =>
            {
                await service.DeleteReservation(1, 1);
            });
        }

        [TestMethod]
        public async Task CreateReservation_ShouldThrow_WhenItemDoesNotExist()
        {
            var repoMock = new Mock<IReservationRepository>();

            repoMock.Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            var service = new ReservationService(repoMock.Object);

            var dto = new CreateReservationDto
            {
                ItemId = 1
            };

            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() =>
                service.CreateReservation(dto, 1));
        }

        [TestMethod]
        public async Task CreateReservation_ShouldFail_WhenItemIsAvailable()
        {
            var repoMock = new Mock<IReservationRepository>();
            repoMock.Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            repoMock.Setup(x => x.LoanerExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            repoMock.Setup(x => x.GetByLoanerId(It.IsAny<int>()))
                .ReturnsAsync(new List<Reservation>());
            repoMock.Setup(x => x.GetByItemIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Reservation>());
            repoMock.Setup(x => x.ItemIsUnavailable(It.IsAny<int>()))
                .ReturnsAsync(false);
            var service = new ReservationService(repoMock.Object);
            var dto = new CreateReservationDto
            {
                ItemId = 1
            };
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                service.CreateReservation(dto, 1));
        }


        [TestMethod]
        public async Task CancelReservation_OwnReservation()
        {
            var repoMock = new Mock<IReservationRepository>();

            repoMock.Setup(x => x.GetByLoanerId(1))
                .ReturnsAsync(new List<Reservation>
                {
            new Reservation
            {
                Id = 1,
                ItemId = 10,
                LoanerId = 1,
                QueueNumber = 1
            }
                });

            repoMock.Setup(x => x.GetByItemIdAsync(10))
                .ReturnsAsync(new List<Reservation>());

            repoMock.Setup(x => x.DeleteAsync(It.IsAny<Reservation>()))
                .Returns(Task.CompletedTask);

            repoMock.Setup(x => x.UpdateAsync(It.IsAny<Reservation>()))
                .Returns(Task.CompletedTask);

            var service = new ReservationService(repoMock.Object);

            var result = await service.DeleteReservation(10, 1);

            Assert.IsTrue(result);
        }
    }
}
