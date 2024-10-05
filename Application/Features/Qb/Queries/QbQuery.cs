using MediatR;
using QbSync.QbXml.Objects;
using Application.Commond.Abstractions;

namespace Application.Features.Qb.Queries;

public class QbQuery<TMessage, TResult> : IRequest<TResult>
    where TMessage : IQbRequest where TResult : IQbResponse
{
    public TMessage Message { get; }

    public QbQuery(TMessage message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }
}

internal sealed class QbQueryHandler<TMessage, TResult> : IRequestHandler<QbQuery<TMessage, TResult>, TResult>
    where TMessage : IQbRequest where TResult : IQbResponse
{
    private readonly IQbXmlRequestProcessor _qbXmlRequestProcessor;

    public QbQueryHandler(IQbXmlRequestProcessor qbXmlRequestProcessor)
    {
        _qbXmlRequestProcessor = qbXmlRequestProcessor;
    }

    public async Task<TResult> Handle(QbQuery<TMessage, TResult> request, CancellationToken cancellationToken)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var qbRequest = request
            .Message;

        var qbXmlRequest = _qbXmlRequestProcessor
            .Serialize(qbRequest);

        var qbXmlResponse = await _qbXmlRequestProcessor
            .ProcessAsync(qbXmlRequest, cancellationToken);

        var qbResponse = _qbXmlRequestProcessor
            .Deserialize<TResult>(qbXmlResponse);

        return qbResponse;
    }
}

