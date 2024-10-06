using QbSync.QbXml.Objects;

namespace Application.Commond.Abstractions.Qb;

public interface IQbXmlRequestProcessor
{
    Task<string> ProcessAsync(string responXml, CancellationToken cancellation = default);
}
