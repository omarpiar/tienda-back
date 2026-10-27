namespace tienda_back.Tests;

public class ProgramTests : IClassFixture<WebApplicationFactoryHelper>
{
    private readonly WebApplicationFactoryHelper _factory;

    public ProgramTests(WebApplicationFactoryHelper factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Aplicacion_IniciaCorrectamente()
    {
        using var client = _factory.CreateClient();

        Assert.NotNull(client);
    }
}
