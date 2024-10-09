using Application.Common.Abstractions.Qb;
using MediatR;
using QbSync.QbXml;
using QbSync.QbXml.Objects;

namespace Application.Features.Qb.Queries.GetQbList;

internal sealed class GetQbListQueryHandler<TRequest, TResponse> : IRequestHandler<GetQbListQuery<TRequest, TResponse>, TResponse>
    where TRequest : class, IQbRequest where TResponse : class
{
    private readonly IQuickBooksXmlService _requestProcessor;
    public GetQbListQueryHandler(IQuickBooksXmlService requestProcessor)
    {
        _requestProcessor = requestProcessor;
    }

    public async Task<TResponse> Handle(GetQbListQuery<TRequest, TResponse> request, CancellationToken cancellationToken)
    {
        // Build the QuickBooks XML request
        var qbXmlRequest = new QbXmlRequest();
        qbXmlRequest
            .AddToSingle(request.Message);
        var xmlRequestString = qbXmlRequest
            .GetRequest();

        var responseXml = await _requestProcessor
            .SendRequestAsync(xmlRequestString);

        // Build the QuickBooks XML response 
        var qbResponse = new QbXmlResponse();
        var response = qbResponse
            .GetSingleItemFromResponse<TResponse>(responseXml);

        return response!;
    }
}
