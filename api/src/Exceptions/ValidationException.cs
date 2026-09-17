namespace TbtbChallenge.Api.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string field, string message) : base(message)
    {
        Field = field;
    }

    public string Field { get; }
}
