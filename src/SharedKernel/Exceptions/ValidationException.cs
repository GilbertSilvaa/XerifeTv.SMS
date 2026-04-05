namespace SharedKernel.Exceptions;

public sealed class ValidationException : DomainException
{
    private const string ERROR_CODE = "Validation.Error";

    private List<string> _errors { get; set; } = [];
    public IReadOnlyList<string> Errors => _errors;

    public ValidationException(string message) : base(ERROR_CODE, message) { }

    public ValidationException(IEnumerable<string> errors) : base(ERROR_CODE, string.Join('|', errors))
    {
        _errors = [.. errors];
    }
}