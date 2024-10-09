using QbSync.QbXml.Objects;

namespace Application.Common.Abstractions.Qb;

public interface IQuickBooksXmlService
{
    Task<string> SendRequestAsync(string responXml, CancellationToken cancellation = default);
}
