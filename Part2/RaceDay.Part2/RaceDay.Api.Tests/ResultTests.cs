using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Api.Tests
{
    public class ResultTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public ResultTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Organiser_CanCaptureResultForOwnEvent_Returns201()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Organiser1);
            // Enrolment 3 (Lindiwe, Vaal Family Fun Walk) belongs to Thandeka's event and has no result yet
            var response = await client.PostAsJsonAsync("/api/enrolments/3/results",
                new { finishTime = "00:58:30", finishPosition = 12 });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_DuplicateResult_Returns409()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Organiser1);
            // Enrolment 1 already has a seeded result
            var response = await client.PostAsJsonAsync("/api/enrolments/1/results",
                new { finishTime = "03:00:00", finishPosition = 1 });
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CannotCaptureResultForAnotherOrganisersEvent_Returns403()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Organiser1);
            // Enrolment 4 is in event 3, which belongs to Sipho
            var response = await client.PostAsJsonAsync("/api/enrolments/4/results",
                new { finishTime = "03:00:00", finishPosition = 1 });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_ResultForMissingEnrolment_Returns404()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Organiser1);
            var response = await client.PostAsJsonAsync("/api/enrolments/999/results",
                new { finishTime = "03:00:00", finishPosition = 1 });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CanViewOwnResults_Returns200()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Participant1);
            var response = await client.GetAsync("/api/results/my");
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(json.GetArrayLength() >= 1);
            Assert.Equal("Joburg City Marathon", json[0].GetProperty("eventName").GetString());
        }
    }
}