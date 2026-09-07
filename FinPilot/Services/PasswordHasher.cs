using System;
using System.Security.Cryptography;
using System.Text;

namespace FinPilot.Services
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int KeySize = 32;  // 256 bits
        private const int Iterations = 100000;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithm,
                KeySize);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            var parts = hashedPassword.Split(':');
            if (parts.Length != 2)
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] storedHash = Convert.FromBase64String(parts[1]);

                byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password),
                    salt,
                    Iterations,
                    HashAlgorithm,
                    KeySize);

                return CryptographicOperations.FixedTimeEquals(inputHash, storedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
