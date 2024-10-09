using Application.Common.Abstractions.Services;

namespace Application.Infrastructure.Services;

internal class CurrentUserService : ICurrentUserService
{
    public CurrentUserService()
    {
        UserId = Guid.NewGuid()
            .ToString();
    }
    
    public string? UserId { get; set; }
}
