using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests
{
    public class RoleTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public RoleTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private static readonly object SomeEvent = new
        {
            name = "Sneaky Event",
            description = "x",
            eventDate = "2027-03-01T00:00:00",
            location = "Nowhere",
            distanceKm = 5.0,
            eventType = "Walk"
        };

        [Fact]
        public async Task Participant_CannotCreateEvent_Returns403()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Participant1);
            var response = await client.PostAsJsonAsync("/api/events", SomeEvent);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CannotDeleteEvent_Returns403()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Participant1);
            var response = await client.DeleteAsync("/api/events/2");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CannotViewEventEnrolments_Returns403()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Participant1);
            var response = await client.GetAsync("/api/events/1/enrolments");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CannotCaptureResult_Returns403()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Participant1);
            var response = await client.PostAsJsonAsync("/api/enrolments/3/results",
                new { finishTime = "01:00:00", finishPosition = 1 });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CannotAddCategory_Returns403()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Participant1);
            var response = await client.PostAsJsonAsync("/api/events/1/categories", new { name = "Sneaky" });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CannotEnrolInEvent_Returns403()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Organiser1);
            var response = await client.PostAsJsonAsync("/api/events/3/enrolments", new { categoryId = 4 });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CannotViewParticipantResults_Returns403()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Organiser1);
            var response = await client.GetAsync("/api/results/my");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Unauthenticated_CannotCreateEvent_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/events", SomeEvent);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Unauthenticated_CannotEnrol_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/events/1/enrolments", new { categoryId = 1 });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}