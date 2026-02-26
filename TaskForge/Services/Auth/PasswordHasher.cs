using System.Security.Cryptography;
using System.Text;

namespace TaskForge.Services.Auth;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128-bit
    private const int KeySize = 32;  // 256-bit
    private const int Iterations = 100_000;

    public string HashPassword(string password)
    {
        // Generate salt
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Convert password to bytes
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        // Derive key using the new static API
        var key = Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 3)
            return false;

        var iterations = int.Parse(parts[0]);
        var salt = Convert.FromBase64String(parts[1]);
        var key = Convert.FromBase64String(parts[2]);

        var passwordBytes = Encoding.UTF8.GetBytes(password);

        var keyToCheck = Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return keyToCheck.SequenceEqual(key);
    }
}