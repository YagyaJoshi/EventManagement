using System.Security.Cryptography;
using System.Text;

namespace EventManagement.Utilities.Helpers
{
    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            // Compute hash of the password using SHA-256
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                string hashedPassword = Convert.ToBase64String(hashBytes);
                return hashedPassword;
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Compute hash of the provided password
            string hashedInputPassword = HashPassword(password);

            // Compare the computed hash with the stored hash
            return string.Equals(hashedInputPassword, hashedPassword);
        }
    }
}
