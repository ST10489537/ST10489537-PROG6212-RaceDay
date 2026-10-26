using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests
{
    public class EnrolmentTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public EnrolmentTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<(HttpClient client, string email)> NewParticipantAsync()
        {
            var email = $"{Guid.NewGuid():N}@test.com";
            var client = _factory.CreateClient();
            await client.PostAsJsonAsync("/api/auth/register", new
            {
                fullName = "New Participant",
                email,
                password = TestData.Password,
                role = "Participant"
            });
            await client.PostAsJsonAsync("/api/auth/login", new { email, password = TestData.Password });
            return (client, email);
        }

        [Fact]
        public async Task Participant_CanEnrol_AndEnrolmentIsRecorded()
        {
            var (client, email) = await NewParticipantAsync();

            var response = await client.PostAsJsonAsync("/api/events/1/enrolments", new { categoryId = 1 });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Check the database holds the right participant, event, category and status
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
            var saved = await db.Enrolments.Include(e => e.Participant)
                .SingleAsync(e => e.Participant.Email == email);

            Assert.Equal(1, saved.EventID);
            Assert.Equal(1, saved.CategoryID);
            Assert.Equal(EnrolmentStatus.Pending, saved.Status);
        }

        [Fact]
        public async Task Participant_EnrollingTwiceInSameEvent_Returns409()
        {
            var (client, _) = await NewParticipantAsync();

            await client.PostAsJsonAsync("/api/events/1/enrolments", new { categoryId = 1 });
            var second = await client.PostAsJsonAsync("/api/events/1/enrolments", new { categoryId = 2 });

            Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        }

        [Fact]
        public async Task Enrol_WithCategoryFromAnotherEvent_Returns404()
        {
            var (client, _) = await NewParticipantAsync();
            var response = await client.PostAsJsonAsync("/api/events/1/enrolments", new { categoryId = 4 });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Enrol_InMissingEvent_Returns404()
        {
            var (client, _) = await NewParticipantAsync();
            var response = await client.PostAsJsonAsync("/api/events/999/enrolments", new { categoryId = 1 });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Participant_SeesOnlyTheirOwnEnrolments()
        {
            var (client, _) = await NewParticipantAsync();
            await client.PostAsJsonAsync("/api/events/2/enrolments", new { categoryId = 3 });

            var response = await client.GetAsync("/api/enrolments/my");
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, json.GetArrayLength());   // not the seeded users' enrolments
        }

        [Fact]
        public async Task Organiser_CanViewEnrolmentsForOwnEvent_Returns200()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Organiser1);
            var response = await client.GetAsync("/api/events/1/enrolments");
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(json.GetArrayLength() >= 2);
        }

        [Fact]
        public async Task Organiser_CannotViewEnrolmentsForAnotherOrganisersEvent_Returns403()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Organiser1);
            var response = await client.GetAsync("/api/events/3/enrolments");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}