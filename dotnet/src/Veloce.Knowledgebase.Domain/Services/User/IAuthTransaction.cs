namespace Veloce.Knowledgebase.Services.User;

public interface IAuthTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
