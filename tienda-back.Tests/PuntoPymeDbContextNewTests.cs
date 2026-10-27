using Microsoft.EntityFrameworkCore;
using tienda_back.Infrastructure.Data;

namespace tienda_back.Tests;

public class PuntoPymeDbContextNewTests
{
    [Fact]
    public void DbContext_CreaInstanciaCorrectamente()
    {
        var options = new DbContextOptionsBuilder<PuntoPymeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new PuntoPymeDbContext(options);

        Assert.NotNull(context);
        Assert.NotNull(context.Productos);
    }
}
