using System.Text;
using System.Text.Json;
using Autodesk.Api.Startup;
using Microsoft.AspNetCore.Mvc.Testing;
using Autodesk.Domain;
using System.Net;

namespace Autodesk.Api.Test
{
    public class UserControllerTests: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        // Expanded pools of realistic names:
        private readonly string[] _firstNames =
        [
        "Olivia","Liam","Emma","Noah","Ava","Oliver","Sophia","Elijah","Isabella","Lucas",
        "Mia","Mason","Amelia","Logan","Harper","Ethan","Evelyn","James","Abigail","Benjamin",
        "Ella","Jacob","Avery","Michael","Scarlett","Alexander","Grace","William","Chloe","Daniel"
        ];

        private readonly string[] _lastNames =
        [
        "Smith","Johnson","Williams","Brown","Jones","Garcia","Miller","Davis","Rodriguez","Martinez",
        "Hernandez","Lopez","Gonzalez","Wilson","Anderson","Thomas","Taylor","Moore","Jackson","Martin",
        "Lee","Perez","Thompson","White","Harris","Sanchez","Clark","Ramirez","Lewis","Robinson"
        ];

        public UserControllerTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Insert_1000_Users_ShouldReturn_Created()
        {
            var rnd = new Random();

            for (int i = 0; i < 1_000; i++)
            {
                // pick realistic names at random
                var first = _firstNames[rnd.Next(_firstNames.Length)];
                var last = _lastNames[rnd.Next(_lastNames.Length)];

                var user = new User(Guid.NewGuid().ToString())
                {
                    Name     = first,
                    Lastname = last,
                    // ensures unique email addresses
                    Email    = $"{first.ToLower()}.{last.ToLower()}{i}@example.com",
                    Active   = true
                };

                var json = JsonSerializer.Serialize(user, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync("/api/v1/users", content);
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            }
        }
    }

}
