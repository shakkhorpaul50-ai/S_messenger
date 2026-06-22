using Microsoft.AspNetCore.Identity;

namespace MessengerAPI.Helpers;

public class PasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
    {
        return _hasher.HashPassword(null!, password);
    }

    public bool Verify(string password, string storedHash)
    {
        var result = _hasher.VerifyHashedPassword(null!, storedHash, password);
        return result == PasswordVerificationResult.Success;
    }
}
