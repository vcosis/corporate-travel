namespace CorporateTravel.Application.Dtos;

public class CommandResult
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? Message { get; set; }

    public static CommandResult Success(string? message = null)
    {
        return new CommandResult
        {
            Succeeded = true,
            Message = message
        };
    }

    public static CommandResult Failure(params string[] errors)
    {
        return new CommandResult
        {
            Succeeded = false,
            Errors = errors.ToList()
        };
    }
} 