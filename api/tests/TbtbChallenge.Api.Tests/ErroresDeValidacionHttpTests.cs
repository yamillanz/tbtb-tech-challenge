using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TbtbChallenge.Api.Dtos;

namespace TbtbChallenge.Api.Tests;

public class ErroresDeValidacionHttpTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ErroresDeValidacionHttpTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ErroresDeValidacion_CuandoElCanalEstaFueraDelCatalogo_LaRespuestaEsProblemDetailsConElCampoIncumplido()
    {
        var client = _factory.CreateClient();
        var request = new CreateContactRequest(
            999999,
            999999,
            DateOnly.FromDateTime(DateTime.UtcNow),
            "telegrafía",
            "contestado",
            null);

        var response = await client.PostAsJsonAsync("/api/contacts", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Solicitud inválida", body.GetProperty("title").GetString());
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.Contains("canal", body.GetProperty("detail").GetString());

        var errors = body.GetProperty("errors");
        Assert.True(errors.TryGetProperty("channel", out var channelError));
        Assert.Contains("canal", channelError.GetString());
    }
}
