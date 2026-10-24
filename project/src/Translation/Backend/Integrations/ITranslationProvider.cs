using Translation.Backend.Contracts;

namespace Translation.Backend.Integrations;

public interface ITranslationProvider
{
    // One paragraph per call preserves alignment without a provider-specific batch protocol.
    // No persistence or reviewer authorization here; failures must not contain secrets.
    Task<OperationResult<string>> TranslateAsync(
        TranslationInput input, CancellationToken cancellationToken = default);
}

public sealed record TranslationInput(
    string SourceText, string SourceLanguage, string TargetLanguage);
