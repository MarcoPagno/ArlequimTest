using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using ArlequimTest.Tests.Helpers;
using ArlequimTest.Api.Models;
using ArlequimTest.Api.Exceptions;

namespace ArlequimTest.Tests.Integration;

public class ProductsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    //Product Creation (POST) TESTS
    [Fact]
    public async Task POST_CreateProductAsSeller_ShouldReturn403()
    {
        await TestHelper.AuthenticateAsSeller(_client);

        var body = new
        {
            name = "IntCreateProductTest",
            description = "Integration product test Description",
            price = 100.99m
        };

        var response = await _client.PostAsJsonAsync("/api/products", body);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task POST_CreateProductAsAdmin_ShouldReturn201()
    {
        await TestHelper.AuthenticateAsAdmin(_client);

        var body = new
        {
            name = "IntCreateProductTest",
            description = "Integration product test Description",
            price = 100.99m
        };

        var response = await _client.PostAsJsonAsync("/api/products", body);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(body.name, content.GetProperty("name").GetString());
        Assert.Equal(body.description, content.GetProperty("description").GetString());
        Assert.Equal(body.price, content.GetProperty("price").GetDecimal());
    }

    //Product List (GET) TESTS
    [Fact]
    public async Task GET_FindProduct_ShouldReturn200()
    {
        await TestHelper.AuthenticateAsAdmin(_client);
        var product = await TestHelper.CreateProduct(_client, "IntFindTest", "Integration find test", 1.00m);

        await TestHelper.AuthenticateAsSeller(_client);

        //Verifica se realmente trocou de perfil
        var me = await _client.GetAsync("/api/users/me");
        var meContent = await me.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Seller", meContent.GetProperty("role").GetString());

        var response = await _client.GetAsync("/api/products/" + "IntFindTest");
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(product.GetProperty("name").GetString(), content.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GET_ListProducts_ShouldReturn200()
    {
        await TestHelper.AuthenticateAsAdmin(_client);

        await TestHelper.CreateProduct(_client, "IntListTest1", "Integration list test 1", 1.00m);
        await TestHelper.CreateProduct(_client, "IntListTest2", "Integration list test 2", 2.00m);

        await TestHelper.AuthenticateAsSeller(_client);

        //Verifica se realmente trocou de perfil
        var me = await _client.GetAsync("/api/users/me");
        var meContent = await me.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Seller", meContent.GetProperty("role").GetString());

        var response = await _client.GetAsync("/api/products");
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = content.EnumerateArray().ToList();
        Assert.Contains(products, p => p.GetProperty("name").GetString() == "IntListTest1");
        Assert.Contains(products, p => p.GetProperty("name").GetString() == "IntListTest2");
    }

    //Product DELETE (DELETE) TESTS
    [Fact]
    public async Task DELETE_ProductAsSeller_ShouldReturn403()
    {
        await TestHelper.AuthenticateAsAdmin(_client);

        await TestHelper.CreateProduct(_client, "IntDeleteErrorTest", "Integration delete test", 99.00m);

        await TestHelper.AuthenticateAsSeller(_client);
        var response = await _client.DeleteAsync("/api/products/" + "IntDeleteErrorTest");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DELETE_ProductAsAdmin_ShouldReturn204()
    {
        await TestHelper.AuthenticateAsAdmin(_client);

        await TestHelper.CreateProduct(_client, "IntDeleteTest", "Integration delete test", 99.00m);

        var response = await _client.DeleteAsync("/api/products/"+ "IntDeleteTest");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var findResponse = await _client.GetAsync("/api/products/IntDeleteTest");
        Assert.Equal(HttpStatusCode.NotFound, findResponse.StatusCode);
    }

    //Product Update (PATCH) TESTS
    [Fact]
    public async Task PATCH_ProductAsSeller_ShouldReturn403()
    {
        await TestHelper.AuthenticateAsAdmin(_client);
        await TestHelper.CreateProduct(_client, "IntPatchErrorTest", "Integration patch test", 99.00m);

        await TestHelper.AuthenticateAsSeller(_client);
        var body = new
        {
            name = "IntPatchedErrorTest",
            description = "Integration patch error test",
            price = 0.99m
        };
        var response = await _client.PatchAsJsonAsync("/api/products/" + "IntPatchErrorTest", body);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PATCH_ProductAsAdmin_ShouldReturn200()
    {
        await TestHelper.AuthenticateAsAdmin(_client);
        await TestHelper.CreateProduct(_client, "IntPatchTest", "Integration patch test", 100.00m);

        var body = new
        {
            name = "IntPatchedTest",
            description = "Integration patched test",
            price = 99.99m
        };
        var response = await _client.PatchAsJsonAsync("/api/products/" + "IntPatchTest", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var findResponse = await _client.GetAsync("/api/products/IntPatchedTest");
        var findContent = await findResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(body.name, findContent.GetProperty("name").GetString());
        Assert.Equal(body.description, findContent.GetProperty("description").GetString());
        Assert.Equal(body.price, findContent.GetProperty("price").GetDecimal());
    }
}