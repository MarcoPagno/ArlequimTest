using ArlequimTest.Api.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArlequimTest.Tests.Helpers;

public static class TestHelper
{
    // ==================
    // AUTH
    // ==================

    public static async Task AuthenticateAsAdmin(HttpClient client)
    {
        await CreateUser(client, "admin_helper@email.com", "Admin");
        await Authenticate(client, "admin_helper@email.com");
    }

    public static async Task AuthenticateAsSeller(HttpClient client)
    {
        await CreateUser(client, "seller_helper@email.com", "Seller");
        await Authenticate(client, "seller_helper@email.com");
    }

    private static async Task Authenticate(HttpClient client, string email)
    {
        var loginResponse = await client.PostAsJsonAsync("/api/users/login", new
        {
            email,
            password = "123456"
        });

        var data = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = data.GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    // ==================
    // USERS
    // ==================

    public static async Task<JsonElement> CreateUser(HttpClient client, string email, string role = "Seller", string name = "Test User", string password = "123456")
    {
        var response = await client.PostAsJsonAsync("/api/users", new { name, email, password, role });

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    // ==================
    // PRODUCTS
    // ==================

    public static async Task<JsonElement> CreateProduct(HttpClient client, string name, string description = "Test description", decimal price = 10.00m)
    {
        var response = await client.PostAsJsonAsync("/api/products", new { name, description, price });

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    // ==================
    // STOCK
    // ==================

    public static async Task<JsonElement> CreateStock(HttpClient client, string productName, int quantity, string invoiceNumber)
    {
        var response = await client.PostAsJsonAsync("/api/stock", new
        {
            productName,
            quantity,
            invoiceNumber
        });

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    // ==================
    // ORDERS
    // ==================

    public static async Task<JsonElement> CreateOrder(HttpClient client, string customerDocument, string sellerName, IEnumerable<(string ProductName, int Quantity)> products)
    {
        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            customerDocument,
            sellerName,
            items = products.Select(p => new { p.ProductName, p.Quantity })
        });

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}
