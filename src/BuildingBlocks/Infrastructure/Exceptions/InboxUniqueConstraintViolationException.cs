namespace BuildingBlocks.Infrastructure.Exceptions;

public sealed class InboxUniqueConstraintViolationException : Exception
{
    public string? ConstraintName { get; }

    public InboxUniqueConstraintViolationException(
        string? constraintName = null,
        Exception? innerException = null)
        : base("A unique constraint was violated.", innerException)
    {
        ConstraintName = constraintName;
    }
}