namespace CorporateTravel.Application.Interfaces;

public interface IPasswordRequirementsService
{
    PasswordRequirements GetPasswordRequirements();
    bool ValidatePassword(string password, out List<string> errors);
}

public class PasswordRequirements
{
    public int MinimumLength { get; set; }
    public bool RequireDigit { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireNonAlphanumeric { get; set; }
    public string Description { get; set; } = string.Empty;
} 