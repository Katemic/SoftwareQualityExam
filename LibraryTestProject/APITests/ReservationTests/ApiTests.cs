using LibraryAPI.Controllers;
using LibraryAPI.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace LibraryTestProject.APITests.ReservationTests
{
    public class ApiTests
    {
        [TestClass]
        public class ReservationApiTests
        {
            private CustomWebApplicationFactory _factory;
            private HttpClient _client;

            [TestInitialize]
            public void Setup()
            {
                _factory = new CustomWebApplicationFactory();

                _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });
            }

            [TestMethod]
            public async Task GetAllReservations_ReturnsOk()
            {
                _factory.ReservationServiceMock
                    .Setup(x => x.GetAllReservations())
                    .ReturnsAsync(new List<ReservationDto>());

                var response = await _client.GetAsync("/api/reservation");

                Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            }

            [TestMethod]
            public async Task GetMyReservations_ReturnsOk()
            {
                _factory.ReservationServiceMock
                    .Setup(x => x.GetAllLoanersReservation(1))
                    .ReturnsAsync(new List<ReservationDto>
                    {
            new ReservationDto { ItemId = 1 }
                    });

                var response = await _client.GetAsync("/api/reservation/MyReservations");

                Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            }

            [TestMethod]
            public async Task GetMyReservations_ReturnsNotFound_WhenEmpty()
            {
                _factory.ReservationServiceMock
                    .Setup(x => x.GetAllLoanersReservation(1))
                    .ReturnsAsync(new List<ReservationDto>());

                var response = await _client.GetAsync("/api/reservation/MyReservations");

                Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            }

            [TestMethod]
            public async Task CreateReservation_ReturnsOk()
            {
                var dto = new CreateReservationDto
                {
                    ItemId = 1
                };

                _factory.ReservationServiceMock
                    .Setup(x => x.CreateReservation(It.IsAny<CreateReservationDto>(), 1))
                    .ReturnsAsync(new ReservationDto { ItemId = 1 });

                var response = await _client.PostAsJsonAsync("/api/reservation", dto);

                Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            }

            [TestMethod]
            public async Task UpdateReservation_ReturnsOk()
            {
                _factory.ReservationServiceMock
                    .Setup(x => x.UpdateReservation(1, It.IsAny<ReservationStatus>()))
                    .ReturnsAsync(new ReservationDto { ItemId = 1 });

                var response = await _client.PutAsync(
                    "/api/reservation/Update/1",
                    null);

                Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            }

            [TestMethod]
            public async Task DeleteReservation_ReturnsNotFound()
            {
                _factory.ReservationServiceMock
                    .Setup(x => x.DeleteReservation(1, 1))
                    .ReturnsAsync(false);

                var response = await _client.DeleteAsync("/api/reservation/1");

                Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            }
        }

    }
}
