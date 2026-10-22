using System.Net.Http.Json;
using tienda_back;

namespace tienda_back.Tests;

public class WeatherForecastControllerTests : IClassFixture<WebApplicationFactoryHelper>
{
    private readonly HttpClient _client;

    public WeatherForecastControllerTests(WebApplicationFactoryHelper factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_DevuelveCincoPronosticos()
    {
        var pronosticos = await _client.GetFromJsonAsync<WeatherForecast[]>("/WeatherForecast");

        Assert.NotNull(pronosticos);
        Assert.Equal(5, pronosticos!.Length);
    }

    [Fact]
    public async Task Get_CalculaTemperaturaFahrenheit()
    {
        var pronosticos = await _client.GetFromJsonAsync<WeatherForecast[]>("/WeatherForecast");

        Assert.NotNull(pronosticos);
        foreach (var pronostico in pronosticos!)
        {
            var esperado = 32 + (int)(pronostico.TemperatureC / 0.5556);
            Assert.Equal(esperado, pronostico.TemperatureF);
        }
    }
}
