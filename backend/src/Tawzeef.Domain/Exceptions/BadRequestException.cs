namespace Tawzeef.Domain.Exceptions;

public class BadRequestException : Exception
{
    public List<string> Errors { get; }

    public BadRequestException(string message) : base(message)
    {
        Errors = [message];
    }

    public BadRequestException(IEnumerable<string> errors) : base(string.Join("; ", errors))
    {
        Errors = errors.ToList();
    }
}
