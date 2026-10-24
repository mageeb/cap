using Translation.Backend.Contracts;

namespace Translation.Backend.Drafts;

public interface IDraftTranslationService
{
    // Checks access and coordinates the provider with durable draft storage.
    // Repeated requests must not silently overwrite reviewed text.
    Task<OperationResult<DraftOperation>> RequestAsync(
        ReviewerContext reviewer, Guid bookId, string RequestId,
        CancellationToken cancellationToken = default);

    Task<OperationResult<DraftOperation>> GetStatusAsync(
        ReviewerContext reviewer, Guid bookId, Guid operationId,
        CancellationToken cancellationToken = default);
}

// Logical operation state does not imply a queue, worker or separate deployment.
public sealed record DraftOperation(Guid Id, DraftState State, string? FailureMessage);
public enum DraftState { Pending, Succeeded, Failed }
