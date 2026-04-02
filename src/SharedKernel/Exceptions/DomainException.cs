namespace SharedKernel.Exceptions;

public abstract class DomainException : Exception
{
    public string Code { get; private set; } = default!;

    public DomainException(string code, string message) : base(message)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentNullException(nameof(code));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message));

        Code = code;
    }
}
