using Microsoft.Extensions.DependencyInjection;
using QbXmlTest.Test;
using Application.Common.Abstractions.Qb;
using Application.Infrastructure.QuickBooksIntegration;

namespace Application.Test.QuickBooksIntegration;

[TestFixture]
public class QuickBooksXmlServiceTests : BaseIntegrationTest
{
    [SetUp]
    public void SetUp()
    {

    }

    protected override void OnConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IQuickBooksXmlService, QuickBooksXmlService>();
    }

    [Test]
    public void SendRequestAsync_Should_Throw_ArgumentNullException_When_RequestXml_Is_Empty()
    {
        using(var scope = Services.CreateScope())
        {
            var qbService = scope.ServiceProvider
                .GetRequiredService<IQuickBooksXmlService>();

            var ex = Assert.ThrowsAsync<ArgumentNullException>(() => qbService.SendRequestAsync(""));

            // Verify the exception contains the correct parameter name
            Assert.That(ex.ParamName, Is.EqualTo("requestXml"));
        }
    }
}
