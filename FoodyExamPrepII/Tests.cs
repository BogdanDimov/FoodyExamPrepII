using FoodyExamPrepII.DTOs;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace FoodyExamPrepII;

public class Tests
{
    private static RestClient? _client;
    private const string BaseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";

    private const string UserName = "bobo19@testa.com";
    private const string Password = "123456789";

    private string? _token;
    private string? _foodId;

    [SetUp]
    public void Setup()
    {
        _client = new RestClient(BaseUrl);

        var requestBody = new
        {
            userName = UserName,
            firstName = "Bobo",
            midName = "Middle",
            lastName = "Test",
            email = UserName,
            password = Password,
            rePassword = Password
        };
        var request = new RestRequest("/api/User/Create", Method.Post);
        request.AddJsonBody(requestBody);
        var response = _client.Execute(request);

        if (response.StatusCode == HttpStatusCode.OK
            || JsonSerializer.Deserialize<JsonElement>(response.Content ?? string.Empty).GetString() ==
            "Email address already exist!")
        {
            request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { userName = UserName, password = Password });
            response = _client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = response.Content ?? string.Empty;
                var json = JsonSerializer.Deserialize<JsonElement>(content);
                if (json.TryGetProperty("accessToken", out var tokenElement))
                {
                    _token = tokenElement.GetString() ?? string.Empty;
                }
                else
                {
                    throw new Exception("Token not found in the response.");
                }
            }
        }
        else
        {
            throw new Exception("Failed to create user.");
        }

        if (string.IsNullOrEmpty(_token))
        {
            throw new Exception("Authentication failed, token is null or empty.");
        }

        var options = new RestClientOptions(BaseUrl)
        {
            Authenticator = new JwtAuthenticator(_token)
        };

        _client = new RestClient(options);
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _client = null;
    }

    [Test, Order(1)]
    //Create a New Food with the Required Fields
    // · Create a test to send a POST request to add a new food.
    // · Assert that the response status code is Created (201).
    // · Assert that the response body contains a foodId property.
    // · Store the foodId of the created food in a static member of the test class to maintain its value between test runs.
    public void CreateFood()
    {
        var request = new RestRequest("/api/Food/Create", Method.Post);
        request.AddJsonBody(new FoodDTO
        {
            Name = "Test Food",
            Description = "This is a test food description.",
        });
        var response = _client.Execute(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var content = response.Content ?? string.Empty;
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.That(json.TryGetProperty("foodId", out var foodIdElement), Is.True);

        _foodId = foodIdElement.GetString() ?? string.Empty;
    }

    //Edit the Title of the Food that you Created
    // · Create a test that sends a PATCH request to edit the title of the food
    // · Use the foodId that you stored in the previous request as a path variable.
    // · Assert that the response status code is OK (200).
    // · Assert that the response message indicates the food was "Successfully edited".
    [Test, Order(2)]
    public void EditFoodTitle()
    {
        var request = new RestRequest($"/api/Food/Edit/{_foodId}", Method.Patch);
        request.AddJsonBody(new[]
        {
            new
            {
                path = "/name",
                op = "replace",
                value = "Updated Test Food"
            }
        });
        var response = _client.Execute(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = response.Content ?? string.Empty;
        var json = JsonSerializer.Deserialize<ApiResponseDTO>(content);
        Assert.That(json.Msg, Is.EqualTo("Successfully edited"));
    }

    //Get All Foods
    // · Create a test to send a GET request to list all foods.
    // · Assert that the response status code is OK (200).
    // · Assert that the response contains a non-empty array.
    [Test, Order(3)]
    public void GetAllFoods()
    {
        var request = new RestRequest("/api/Food/All", Method.Get);
        var response = _client.Execute(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = response.Content ?? string.Empty;
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.That(json.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(json.GetArrayLength(), Is.GreaterThan(0));
    }

    //Delete the Food that you Edited
    // · Create a test that sends a DELETE request.
    // · Use the foodId that you stored as a path variable.
    // · Assert that the response status code is OK (200).
    // · Confirm that the response message is "Deleted successfully!".
    [Test, Order(4)]
    public void DeleteFood()
    {
        var request = new RestRequest($"/api/Food/Delete/{_foodId}", Method.Delete);
        var response = _client.Execute(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = response.Content ?? string.Empty;
        var json = JsonSerializer.Deserialize<ApiResponseDTO>(content);
        Assert.That(json.Msg, Is.EqualTo("Deleted successfully!"));
    }

    //Try to Create a Food without the Required Fields
    // · Write a test that attempts to create a food with missing required fields (Name, Description).
    // · Send the POST request with the incomplete data.
    // · Assert that the response status code is BadRequest (400).
    [Test, Order(5)]
    public void CreateFoodWithoutRequiredFields()
    {
        var request = new RestRequest("/api/Food/Create", Method.Post);
        request.AddJsonBody(new
        {
            Name = string.Empty,
            Description = string.Empty
        });
        var response = _client.Execute(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    //Try to Edit a Non-existing Food
    // · Write a test to send a PUT request to edit an Food with a foodId that does not exist.
    // · Assert that the response status code is NotFound (404).
    // · Assert that the response message is "No food revues...".
    [Test, Order(6)]
    public void EditNonExistingFood()
    {
        var nonExistingFoodId = "non-existing-id";
        var request = new RestRequest($"/api/Food/Edit/{nonExistingFoodId}", Method.Patch);
        request.AddJsonBody(new[]
        {
            new
            {
                path = "/name",
                op = "replace",
                value = "Updated Test Food"
            }
        });
        var response = _client.Execute(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        var content = response.Content ?? string.Empty;
        var json = JsonSerializer.Deserialize<ApiResponseDTO>(content);
        Assert.That(json.Msg, Is.EqualTo("No food revues..."));
    }

    //Try to Delete a Non-existing Food
    // · Write a test to send a DELETE request to edit a food with a foodId that does not exist.
    // · Assert that the response status code is BadRequest (400).
    // · Assert that the response message is "Unable to delete this food revue!".
    [Test, Order(7)]
    public void DeleteNonExistingFood()
    {
        var nonExistingFoodId = "non-existing-id";
        var request = new RestRequest($"/api/Food/Delete/{nonExistingFoodId}", Method.Delete);
        var response = _client.Execute(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var content = response.Content ?? string.Empty;
        var json = JsonSerializer.Deserialize<ApiResponseDTO>(content);
        Assert.That(json.Msg, Is.EqualTo("Unable to delete this food revue!"));
    }
}