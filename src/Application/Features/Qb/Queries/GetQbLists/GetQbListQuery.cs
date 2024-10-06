using FluentValidation;
using MediatR;
using QbSync.QbXml;

namespace Application.Features.Qb.Queries.GetQbLists;

public record GetQbListQuery(QbXmlRequest XmlRequest) : IRequest<string>;

public class GetQbListQueryValidator : AbstractValidator<GetQbListQuery>
{
    public GetQbListQueryValidator()
    {
        RuleFor(query => query.XmlRequest)
             .Must(BeValidRequest);
    }

    private bool BeValidRequest(QbXmlRequest request)
    {
        return true;
    }
}

