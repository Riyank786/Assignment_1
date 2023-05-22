using BCrypt.Net;

namespace Assignment_1.Services {
    public class PasswordService {
        public string HashPassword(string password) {
            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }

        public bool VerifyPassword(string password, string passwordHash) {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
