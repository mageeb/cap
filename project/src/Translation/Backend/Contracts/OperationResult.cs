namespace Translation.Backend.Contracts;

// Application outcomes, not HTTP responses. Cancellation propagates as cancellation.
public abstract record OperationResult<T>
{
    private OperationResult() { }
    public sealed record Success(T Value) : OperationResult<T>;
    public sealed record Failure(ErrorCode Code, string Message) : OperationResult<T>;
}

public enum ErrorCode
{
    InvalidInput, NotFound, AccessDenied, Conflict, DependencyUnavailable
}

// Construct from validated server-side identity, never from a request body's user ID.
public sealed record ReviewerContext(string SubjectId);
