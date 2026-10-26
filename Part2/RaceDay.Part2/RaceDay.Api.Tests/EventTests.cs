using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Api.Tests
{
    public class EventTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public EventTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private static object NewEvent(string name = "Test 10km Run") => new
        {
            name,
            description = "A test event",
            eventDate = "2027-03-01T00:00:00",
            location = "Pretoria, Gauteng",
            distanceKm = 10.0,
            eventType = "Run"
        };

        [Fact]
        public async Task Organiser_CanCreateEvent_Returns201()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Organiser1);
            var response = await client.PostAsJsonAsync("/api/events", NewEvent());
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CanUpdateOwnEvent_Returns200()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Organiser1);
            var response = await client.PutAsJsonAsync("/api/events/1", NewEvent("Renamed Marathon"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CanDeleteOwnEventWithoutEnrolments_Returns204()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Organiser1);
            var created = await client.PostAsJsonAsync("/api/events", NewEvent("Event To Delete"));
            var json = await created.Content.ReadFromJsonAsync<JsonElement>();
            var id = json.GetProperty("eventID").GetInt32();

            var response = await client.DeleteAsync($"/api/events/{id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CannotDeleteEventWithEnrolments_Returns409()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Organiser1);
            var response = await client.DeleteAsync("/api/events/1");
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CannotUpdateAnotherOrganisersEvent_Returns403()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Organiser1);
            var response = await client.PutAsJsonAsync("/api/events/3", NewEvent());   // event 3 belongs to Sipho
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithInvalidData_Returns400()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Organiser1);
            var response = await client.PostAsJsonAsync("/api/events", new
            {
                name = "",
                location = "",
                distanceKm = 0,
                eventDate = "2027-03-01T00:00:00",
                eventType = "Run"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CanViewEvents_Returns200()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Participant1);
            var response = await client.GetAsync("/api/events");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task EventDetail_IncludesItsCategories()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Participant1);
            var response = await client.GetAsync("/api/events/1");
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, json.GetProperty("categories").GetArrayLength());
        }

        [Fact]
        public async Task EventDetail_ForMissingEvent_Returns404()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Participant1);
            var response = await client.GetAsync("/api/events/999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Unauthenticated_ViewEvents_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/events");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}