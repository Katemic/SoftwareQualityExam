using LibraryAPI.Controllers;
using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using Moq;

namespace LibraryTestProject.UnitTests
{
    [TestClass]
    public sealed class ReservationServiceTests
    {
        // Test cases for blackbox Status

        // Test cases:
        // Status = pending, on creation
        [TestMethod]
        public async Task ValidateStatusWhenCreatedTest()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();
            var service = new ReservationService(repoMock.Object, loanMock.Object);

            repoMock.Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
                    .ReturnsAsync(true);

            repoMock.Setup(x => x.LoanerExistsAsync(It.IsAny<int>()))
                    .ReturnsAsync(true);

            repoMock.Setup(x => x.GetByLoanerId(It.IsAny<int>()))
                    .ReturnsAsync(new List<Reservation>());
            repoMock.Setup(x => x.ItemIsUnavailable(It.IsAny<int>()))
                    .ReturnsAsync(() => true);

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

        // Test cases:
        // Status = fulfilled, on update
        [TestMethod]
        public async Task UpdateReservation_ShouldUpdateStatusFulfilled()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();

            var reservation = new Reservation
            {
                Id = 1,
                Status = "pending"
            };

            repoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(reservation);
            repoMock.Setup(x => x.UpdateAsync(It.IsAny<Reservation>()))
                .Returns(Task.CompletedTask);

            Reservation updatedReservation = null;

            repoMock.Setup(x => x.UpdateAsync(It.IsAny<Reservation>()))
                .Callback<Reservation>(r => updatedReservation = r)
                .Returns(Task.CompletedTask);

            var service = new ReservationService(repoMock.Object, loanMock.Object);

            // Act
            var result= await service.UpdateReservation(1, ReservationStatus.Fulfilled);

            // Assert - verify enum conversion
            Assert.AreEqual("fulfilled", result.Status);
        }

        // Test cases:
        // Status = ready for pickup, on update
        [TestMethod]
        public async Task UpdateReservation_ShouldUpdateStatusReadyForPickup()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();

            var reservation = new Reservation
            {
                Id = 1,
                Status = "pending"
            };

            repoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(reservation);

            repoMock.Setup(x => x.UpdateAsync(It.IsAny<Reservation>()))
                .Returns(Task.CompletedTask);

            Reservation updatedReservation = null;

            repoMock.Setup(x => x.UpdateAsync(It.IsAny<Reservation>()))
                .Callback<Reservation>(r => updatedReservation = r)
                .Returns(Task.CompletedTask);

            var service = new ReservationService(repoMock.Object, loanMock.Object);

            // Act
            var result = await service.UpdateReservation(1, ReservationStatus.ReadyForPickup);

            // Assert - verify enum conversion
            Assert.AreEqual("ready for pickup", result.Status);
        }

        // Test cases:
        // Status = pending, on update
        [TestMethod]
        public async Task UpdateReservation_ShouldUpdateStatusPending()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();

            var reservation = new Reservation
            {
                Id = 1,
                Status = "ready for pickup"
            };

            repoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(reservation);
            repoMock.Setup(x => x.UpdateAsync(It.IsAny<Reservation>()))
                .Returns(Task.CompletedTask);

            Reservation updatedReservation = null;

            repoMock.Setup(x => x.UpdateAsync(It.IsAny<Reservation>()))
                .Callback<Reservation>(r => updatedReservation = r)
                .Returns(Task.CompletedTask);

            var service = new ReservationService(repoMock.Object, loanMock.Object);

            // Act
            var result = await service.UpdateReservation(1, ReservationStatus.Pending);

            // Assert - verify enum conversion
            Assert.AreEqual("pending", result.Status);
        }

        // Test cases:
        // Invalid status updates throws exception
        [TestMethod]
        [DataRow("cancelled")]
        [DataRow("thisispending")]
        [DataRow(" ")]
        public async Task UpdateReservation_ShouldUpdateStatusInvalid(string invalidStatus)
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();

            var reservation = new Reservation
            {
                Id = 1,
                Status = "pending"
            };

            repoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(reservation);

            repoMock.Setup(x => x.UpdateAsync(It.IsAny<Reservation>()))
                .Returns(Task.CompletedTask);

            Reservation updatedReservation = null;

            repoMock.Setup(x => x.UpdateAsync(It.IsAny<Reservation>()))
                .Callback<Reservation>(r => updatedReservation = r)
                .Returns(Task.CompletedTask);

            var service = new ReservationService(repoMock.Object, loanMock.Object);

            // Assert - verify enum conversion
            Assert.ThrowsException<ArgumentException>(() => service.UpdateReservation(1, (ReservationStatus)Enum.Parse(typeof(ReservationStatus), invalidStatus, true)));
        }

        // Test cases:
        // Null status updates throws exception
        [TestMethod]
        public async Task UpdateReservation_ShouldUpdateStatusNull()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();

            var reservation = new Reservation
            {
                Id = 1,
                Status = "pending"
            };

            repoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(reservation);

            repoMock.Setup(x => x.UpdateAsync(It.IsAny<Reservation>()))
                .Returns(Task.CompletedTask);

            Reservation updatedReservation = null;

            repoMock.Setup(x => x.UpdateAsync(It.IsAny<Reservation>()))
                .Callback<Reservation>(r => updatedReservation = r)
                .Returns(Task.CompletedTask);

            var service = new ReservationService(repoMock.Object, loanMock.Object);

            // Assert - verify enum conversion
            Assert.ThrowsException<ArgumentNullException>(() => service.UpdateReservation(1, (ReservationStatus)Enum.Parse(typeof(ReservationStatus), null, true)));
        }

        // Test cases:
        // Update non-existent reservation should return null
        [TestMethod]
        public async Task UpdateReservation_ShouldUpdateNull()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();
            var service = new ReservationService(repoMock.Object, loanMock.Object);

            // Act
            var result = await service.UpdateReservation(1, ReservationStatus.Fulfilled);

            // Assert
            Assert.IsNull(result);
        }

        // Test cases for blackbox Queue number

        // Test cases:
        // Positive test, when queue is not full, should assign next queue number
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
            var loanMock = new Mock<ILoanRepository>();
            var service = new ReservationService(repoMock.Object, loanMock.Object);

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

        // Test cases:
        // Negative test, when queue is full (100+ reservations), should throw exception
        [TestMethod]
        [DataRow(100)]
        [DataRow(101)]
        public async Task CreateReservation_ShouldFail_WhenQueueIsFull(int reservationCount)
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();
            var service = new ReservationService(repoMock.Object, loanMock.Object);

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

        // Test cases:
        // Starting queue number should be 1 when queue is empty
        [TestMethod]
        public async Task CreateReservation_ShouldSetQueueNumberTo1_WhenQueueIsEmpty()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();
            var service = new ReservationService(repoMock.Object, loanMock.Object);

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

        // Test cases:
        // Multiple reservations should generate sequential queue numbers and increment loaner id
        // Also tests queue numbers are unique per item, when system under normal load (No race conditions tested)
        [TestMethod]
        public async Task CreateReservation_ShouldGenerateSequentialQueueNumbers_AndIncrementLoanerId()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();
            var service = new ReservationService(repoMock.Object, loanMock.Object);

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

        // Test case:
        // Negative, database has rotten data, so queue number is already in use
        [TestMethod]
        public async Task CreateReservation_ShouldFail_WhenQueueNumberAlreadyInUse()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();
            var service = new ReservationService(repoMock.Object, loanMock.Object);
            repoMock.Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
                    .ReturnsAsync(true);
            repoMock.Setup(x => x.LoanerExistsAsync(It.IsAny<int>()))
                    .ReturnsAsync(true);
            repoMock.Setup(x => x.GetByLoanerId(It.IsAny<int>()))
                    .ReturnsAsync(new List<Reservation>());
            repoMock.Setup(x => x.GetByItemIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new List<Reservation>
                    {
                new Reservation { QueueNumber = 2 }
                    });
            repoMock.Setup(x => x.ItemIsUnavailable(It.IsAny<int>()))
                    .ReturnsAsync(() => true);
            var dto = new CreateReservationDto
            {
                ItemId = 1
            };
            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateReservation(dto, 1));
        }

        // Test cases for blackbox Buisness rules

        // Test cases:
        // Positive, user can create reservation when they have less than 3 active reservations, and item is unavailable
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public async Task TestAmountOfReservationsValid(int currentReservations)
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();
            var fakeReservations = Enumerable
                .Range(0, currentReservations)
                .Select(i => new Reservation())
                .ToList();
            repoMock
            .Setup(x => x.GetByItemIdAsync(It.IsAny<int>()))
             .ReturnsAsync(new List<Reservation>());
            repoMock
            .Setup(x => x.GetByLoanerId(It.IsAny<int>()))
            .Returns(Task.FromResult(fakeReservations));
            repoMock.Setup(x => x.ItemIsUnavailable(It.IsAny<int>()))
                    .ReturnsAsync(() => true);
            repoMock
            .Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
        .ReturnsAsync(true);

            repoMock
                .Setup(x => x.LoanerExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            var service = new ReservationService(repoMock.Object, loanMock.Object);
            // Act
            var dto = new CreateReservationDto
            {
                ItemId = 1
            };
            var result = await service.CreateReservation(dto, 1);
            // Assert
            Assert.IsNotNull(result);
        }

        // Test cases:
        // Negative, user cannot create reservation when they have 3 or more active reservations, even if item is unavailable
        [TestMethod]
        [DataRow(3)]
        [DataRow(4)]
        public async Task TestAmountOfReservationsInvalid(int currentReservations)
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();

            var fakeReservations = Enumerable
                .Range(0, currentReservations)
                .Select(i => new Reservation())
                .ToList();

            repoMock
            .Setup(x => x.GetByLoanerId(It.IsAny<int>()))
            .Returns(Task.FromResult(fakeReservations));
            repoMock
            .Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
            repoMock.Setup(x => x.ItemIsUnavailable(It.IsAny<int>()))
                    .ReturnsAsync(() => true);

            repoMock
                .Setup(x => x.LoanerExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            var service = new ReservationService(repoMock.Object, loanMock.Object);

            // Act
            var dto = new CreateReservationDto
            {
                ItemId = 1
            };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            service.CreateReservation(dto, 1));
        }

        // Test cases:
        // Negative, user cannot create reservation for same item if they already have a reservation for it, even if item is unavailable
        [TestMethod]
        public async Task CreateReservation_ShouldFAIL_WhenUserAlreadyReservedItem()
        {
            // Arrange
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();

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
            var service = new ReservationService(repoMock.Object, loanMock.Object);

            var dto = new CreateReservationDto
            {
                ItemId = 1
            };

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                service.CreateReservation(dto, 1));
        }

        // Test cases:
        // Negative, user cannot cancel reservation that doesn't exist
        [TestMethod]
        public async Task User_Cancels_NonexistentRerservation()
        {
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();

            repoMock.Setup(x => x.GetByLoanerId(1))
                .ReturnsAsync(new List<Reservation>());

            var service = new ReservationService(repoMock.Object, loanMock.Object);

            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () =>
            {
                await service.DeleteReservation(1, 1);
            });
        }

        // Test cases:
        // Negative, user cannot create reservation for loaner that doesn't exist
        [TestMethod]
        public async Task CreateReservation_ShouldThrow_WhenLoanerDoesNotExist()
        {
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();

            repoMock.Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            repoMock.Setup(x => x.LoanerExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            var service = new ReservationService(repoMock.Object, loanMock.Object);

            var dto = new CreateReservationDto
            {
                ItemId = 1
            };

            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() =>
                service.CreateReservation(dto, 1));
        }

        // Test cases:
        // Negative, user cannot create reservation for item that doesn't exist
        [TestMethod]
        public async Task CreateReservation_ShouldThrow_WhenItemDoesNotExist()
        {
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();

            repoMock.Setup(x => x.ItemExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            var service = new ReservationService(repoMock.Object, loanMock.Object);

            var dto = new CreateReservationDto
            {
                ItemId = 1
            };

            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() =>
                service.CreateReservation(dto, 1));
        }

        // Test cases:
        // Negative, user cannot create reservation for item that is available
        [TestMethod]
        public async Task CreateReservation_ShouldFail_WhenItemIsAvailable()
        {
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();

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
            var service = new ReservationService(repoMock.Object, loanMock.Object);
            var dto = new CreateReservationDto
            {
                ItemId = 1
            };
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                service.CreateReservation(dto, 1));
        }

        // Test cases:
        // Positive, user can cancel their own reservation
        [TestMethod]
        public async Task CancelReservation_OwnReservation()
        {
            var repoMock = new Mock<IReservationRepository>();
            var loanMock = new Mock<ILoanRepository>();

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

            var service = new ReservationService(repoMock.Object, loanMock.Object);

            var result = await service.DeleteReservation(10, 1);

            Assert.IsTrue(result);
        }
    }
}
