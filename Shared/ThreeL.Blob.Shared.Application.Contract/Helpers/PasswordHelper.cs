using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace ThreeL.Blob.Shared.Application.Contract.Helpers
{
    public class PasswordHelper
    {
        private string HashPassword(string value, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                password: value,
                salt: Encoding.UTF8.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);

            return Convert.ToBase64String(valueBytes);
        }

        public string HashPassword(string password)
        {
            var salt = GenerateSalt();
            var hash = HashPassword(password, salt);
            var result = $"{salt}.{hash}";

            return result;
        }

        private bool Validate(string password, string salt, string hash)
            => HashPassword(password, salt) == hash;

        public bool VerifyHashedPassword(string password, string storePassword)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            if (string.IsNullOrEmpty(storePassword))
            {
                throw new ArgumentNullException(nameof(storePassword));
            }

            var parts = storePassword.Split('.');
            var salt = parts[0];
            var hash = parts[1];

            return Validate(password, salt, hash); ;
        }

        private string GenerateSalt()
        {
            byte[] randomBytes = new byte[128 / 8];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
    }
}
