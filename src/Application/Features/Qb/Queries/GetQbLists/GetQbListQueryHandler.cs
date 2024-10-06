using Application.Commond.Abstractions.Qb;
using MediatR;
using QbSync.QbXml;
using QbSync.QbXml.Objects;

namespace Application.Features.Qb.Queries.GetQbLists;

internal sealed class GetQbListQueryHandler : IRequestHandler<GetQbListQuery, string>
{
    private readonly IQbXmlRequestProcessor _qbXmlRequestProcessor;

    public GetQbListQueryHandler(IQbXmlRequestProcessor qbXmlRequestProcessor)
    {
        _qbXmlRequestProcessor = qbXmlRequestProcessor;
    }

    public async Task<string> Handle(GetQbListQuery request, CancellationToken cancellationToken)
    {
        var qbXmlRequest = request
            .XmlRequest
            .GetRequest();

        return await _qbXmlRequestProcessor
            .ProcessAsync(qbXmlRequest, cancellationToken);
    }
}
