namespace BuildingBlocks.Infrastructure.Exceptions;

public sealed class UniqueConstraintViolationException : Exception
{
    public string? ConstraintName { get; }

    public UniqueConstraintViolationException(
        string? constraintName = null,
        Exception? innerException = null)
        : base("A unique constraint was violated.", innerException)
    {
        ConstraintName = constraintName;
    }
}