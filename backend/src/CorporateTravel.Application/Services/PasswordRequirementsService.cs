using CorporateTravel.Application.Interfaces;
using System.Text.RegularExpressions;

namespace CorporateTravel.Application.Services;

public class PasswordRequirementsService : IPasswordRequirementsService
{
    public PasswordRequirements GetPasswordRequirements()
    {
        return new PasswordRequirements
        {
            MinimumLength = 8,
            RequireDigit = true,
            RequireLowercase = true,
            RequireUppercase = true,
            RequireNonAlphanumeric = true,
            Description = "A senha deve ter pelo menos 8 caracteres, incluindo pelo menos uma letra maiúscula, uma minúscula, um dígito e um caractere especial."
        };
    }

    public bool ValidatePassword(string password, out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrEmpty(password))
        {
            errors.Add("A senha é obrigatória.");
            return false;
        }

        if (password.Length < 8)
        {
            errors.Add("A senha deve ter pelo menos 8 caracteres.");
        }

        if (!Regex.IsMatch(password, @"\d"))
        {
            errors.Add("A senha deve conter pelo menos um dígito.");
        }

        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            errors.Add("A senha deve conter pelo menos uma letra minúscula.");
        }

        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            errors.Add("A senha deve conter pelo menos uma letra maiúscula.");
        }

        if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
        {
            errors.Add("A senha deve conter pelo menos um caractere especial.");
        }

        return errors.Count == 0;
    }
} 