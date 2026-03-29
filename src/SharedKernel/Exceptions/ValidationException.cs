namespace SharedKernel.Exceptions;

public sealed class ValidationException : Exception
{
    private List<string> _errors { get; set; } = [];
    public IReadOnlyList<string> Errors => _errors;

    public ValidationException(string message) : base(message) { }

    public ValidationException(IEnumerable<string> errors) : base(string.Join('|', errors))
    {
        _errors = [.. errors];
    }
}