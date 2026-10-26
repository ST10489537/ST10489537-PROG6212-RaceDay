using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests
{
    public class AuthTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public AuthTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private static string NewEmail() => $"{Guid.NewGuid():N}@test.com";

        [Fact]
        public async Task Register_NewParticipant_Returns201()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                fullName = "Test Runner",
                email = NewEmail(),
                password = "Password123!",
                role = "Participant"
            });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns409()
        {
            var client = _factory.CreateClient();
            var body = new { fullName = "Dup User", email = NewEmail(), password = "Password123!", role = "Participant" };

            await client.PostAsJsonAsync("/api/auth/register", body);
            var second = await client.PostAsJsonAsync("/api/auth/register", body);

            Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        }

        [Fact]
        public async Task Register_OrganiserWithWrongCode_Returns403()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                fullName = "Fake Organiser",
                email = NewEmail(),
                password = "Password123!",
                role = "Organiser",
                organiserCode = "WRONG"
            });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Register_OrganiserWithCorrectCode_Returns201()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                fullName = "Real Organiser",
                email = NewEmail(),
                password = "Password123!",
                role = "Organiser",
                organiserCode = TestData.OrganiserCode
            });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_InvalidRole_Returns400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                fullName = "Odd Role",
                email = NewEmail(),
                password = "Password123!",
                role = "Admin"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_ShortPassword_Returns400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                fullName = "Short Pass",
                email = NewEmail(),
                password = "abc",
                role = "Participant"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_ValidCredentials_Returns200()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login",
                new { email = TestData.Participant1, password = TestData.Password });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_WrongPassword_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login",
                new { email = TestData.Participant1, password = "WrongPassword!" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_UnknownEmail_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login",
                new { email = "nobody@test.com", password = TestData.Password });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Logout_WhenNotLoggedIn_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsync("/api/auth/logout", null);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Profile_AfterLogin_Returns200_AndAfterLogout_Returns401()
        {
            var client = await _factory.LoggedInClientAsync(TestData.Participant1);

            var whileLoggedIn = await client.GetAsync("/api/profile");
            Assert.Equal(HttpStatusCode.OK, whileLoggedIn.StatusCode);

            await client.PostAsync("/api/auth/logout", null);

            var afterLogout = await client.GetAsync("/api/profile");
            Assert.Equal(HttpStatusCode.Unauthorized, afterLogout.StatusCode);
        }
    }
}