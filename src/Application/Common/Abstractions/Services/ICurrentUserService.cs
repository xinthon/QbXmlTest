namespace Application.Common.Abstractions.Services;

public interface ICurrentUserService
{
    public string? UserId { get; }
}
