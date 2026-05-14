using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using ArlequimTest.Tests.Helpers;

namespace ArlequimTest.Tests.Integration;

public class StockIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public StockIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    //Product Creation (POST) TESTS
    [Fact]
    public async Task POST_CreateStockAsSeller_ShouldReturn403()
    {
        await TestHelper.AuthenticateAsAdmin(_client);

        await TestHelper.CreateProduct(_client, 
        "IntCreateStockTest",
        "Integration product test Description",
        100.99m);

        await TestHelper.AuthenticateAsSeller(_client);

        var body = new
        {
            productName = "IntCreateStockTest",
            quantity = 10,
            invoiceNumber = "2026000000128"
        };
        var response = await _client.PostAsJsonAsync("/api/stock", body);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task POST_CreateProductAsAdmin_ShouldReturn201()
    {
        await TestHelper.AuthenticateAsAdmin(_client);

        await TestHelper.CreateProduct(_client,
            "IntCreateStockSuccessfullTest",
            "Integration product successfull test Description",
            100.99m
        );

        var body = new
        {
            productName = "IntCreateStockSuccessfullTest",
            quantity = 100,
            invoiceNumber = "2026000000129"
        };
        var response = await _client.PostAsJsonAsync("/api/stock", body);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(body.productName, content.GetProperty("productName").GetString());
        Assert.Equal(body.quantity, content.GetProperty("quantity").GetInt32());
        Assert.Equal(body.invoiceNumber, content.GetProperty("invoiceNumber").GetString());
    }

    //Product List (GET) TESTS
    [Fact]
    public async Task GET_FindProduct_ShouldReturn200()
    {
        await TestHelper.AuthenticateAsAdmin(_client);

        var product = await TestHelper.CreateProduct(_client,
            "IntFindStockSuccessfullTest", 
            "Integration find successfull test", 
            1.00m
        );

        var body = new
        {
            productName = "IntFindStockSuccessfullTest",
            quantity = 10,
            invoiceNumber = "2026000000130"
        };
        await _client.PostAsJsonAsync("/api/stock", body);

        var response = await _client.GetAsync("/api/stock/" + "IntFindStockSuccessfullTest");
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(body.quantity, content.GetProperty("availableStock").GetInt32());
    }
}