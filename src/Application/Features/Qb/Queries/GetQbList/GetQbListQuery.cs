using MediatR;
using QbSync.QbXml.Objects;

namespace Application.Features.Qb.Queries.GetQbList;

public record GetQbListQuery<TRequest, TResponse>(TRequest Message) : IRequest<TResponse>
    where TRequest : class, IQbRequest where TResponse : class; 

