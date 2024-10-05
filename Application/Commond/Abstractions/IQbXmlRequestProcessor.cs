using QbSync.QbXml.Objects;

namespace Application.Commond.Abstractions;

public interface IQbXmlRequestProcessor
{
    string Serialize(IQbRequest request);

    Task<IQbResponse> ProcessAsync(string responXml, CancellationToken cancellation = default);

    TResult Deserialize<TResult>(IQbResponse response) where TResult : IQbResponse;
}
