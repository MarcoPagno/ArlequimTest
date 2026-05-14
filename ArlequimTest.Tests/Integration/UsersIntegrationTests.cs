using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;

namespace ArlequimTest.Tests.Integration;

public class UsersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsersIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_CreateUser_ShouldReturn201()
    {
        var body = new
        {
            name = "IntCreateTest",
            email = "int_create_test@email.com",
            password = "123456",
            role = "Seller"
        };

        var response = await _client.PostAsJsonAsync("/api/users", body);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        Assert.Equal(body.name, content.GetProperty("name").GetString());
        Assert.Equal(body.email, content.GetProperty("email").GetString());
        Assert.Equal(body.role, content.GetProperty("role").GetString());
    }

    [Fact]
    public async Task POST_Login_ShouldReturnToken()
    {

        await _client.PostAsJsonAsync("/api/users", new
        {
            name = "IntTokenTest",
            email = "IntTokenTest@email.com",
            password = "123456",
            role = "Seller"
        });

        var response = await _client.PostAsJsonAsync("/api/users/login", new
        {
            email = "IntTokenTest@email.com",
            password = "123456"
        });

        var data = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True(data.GetProperty("token").GetString()?.Length > 0);
    }
}