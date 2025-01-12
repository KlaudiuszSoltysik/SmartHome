using System.Text.RegularExpressions;

namespace backend_application.Utilities;

public class ValidatePasswordClass
{
    public static void ValidatePassword(string password)
    {
        if (password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long");
        }
        if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?:{}|<>]"))
        {
            throw new ArgumentException("Password must contain at least one special character (!@#$%^&*(),.?:{}|<>)");
        }
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            throw new ArgumentException("Password must contain at least one uppercase letter");
        }
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            throw new ArgumentException("Password must contain at least one lowercase letter");
        }
        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            throw new ArgumentException("Password must contain at least one number");
        }
    }
}