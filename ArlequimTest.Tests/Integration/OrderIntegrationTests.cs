using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using ArlequimTest.Tests.Helpers;
using ArlequimTest.Api.DTOs;

namespace ArlequimTest.Tests.Integration;

public class OrderIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrderIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    //Order Creation (POST) TESTS
    [Fact]
    public async Task POST_CreateOrderUnauthenticated_ShouldReturn401()
    {
        var body = new
        {
            customerDocument = "Customer02",
            sellerName = "Seller02",
            items = new[] {
                new {
                    productName = "IntOrderNoStockTest",
                    quantity = 100
                }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/orders", body);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task POST_CreateOrderWithoutStock_ShouldReturn400()
    {
        await TestHelper.AuthenticateAsAdmin(_client);
        await TestHelper.CreateProduct(_client,
            "IntOrderNoStockTest",
            "Integration order product test Description",
            100.99m);
        await TestHelper.CreateStock(_client, "IntOrderNoStockTest", 50, "2026000000151");

        await TestHelper.AuthenticateAsSeller(_client);

        var body = new
        {
            customerDocument = "Customer02",
            sellerName = "Seller02",
            items = new[] {
                new {
                    productName = "IntOrderNoStockTest",
                    quantity = 100
                }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/orders", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_CreateProductAsAdmin_ShouldReturn201()
    {
        await TestHelper.AuthenticateAsAdmin(_client);
        await TestHelper.CreateProduct(_client,
            "IntOrderSuccessTest",
            "Integration order success Description",
            100.99m);
        await TestHelper.CreateStock(_client, "IntOrderSuccessTest", 100, "2026000000152");
        await TestHelper.CreateProduct(_client,
            "IntOrderSuccessTest2",
            "Integration order success Description",
            100.99m);
        await TestHelper.CreateStock(_client, "IntOrderSuccessTest2", 10, "2026000000153");

        await TestHelper.AuthenticateAsSeller(_client);

        var body = new
        {
            customerDocument = "Customer03",
            sellerName = "Seller03",
            items = new[] {
                new {
                    productName = "IntOrderSuccessTest",
                    quantity = 77
                },
                new {
                    productName = "IntOrderSuccessTest2",
                    quantity = 10
                }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/orders", body);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(body.customerDocument, content.GetProperty("customerDocument").GetString());
        Assert.Equal(body.sellerName, content.GetProperty("sellerName").GetString());
        Assert.Equal(body.items[0].productName, content.GetProperty("items")[0].GetProperty("productName").GetString());
        Assert.Equal(body.items[0].quantity, content.GetProperty("items")[0].GetProperty("quantity").GetInt32());
        Assert.Equal(body.items[1].productName, content.GetProperty("items")[1].GetProperty("productName").GetString());
        Assert.Equal(body.items[1].quantity, content.GetProperty("items")[1].GetProperty("quantity").GetInt32());
    }

}