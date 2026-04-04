using BCrypt.Net;

namespace AracKiralamaPortali.API.Services
{
    public interface IPasswordHasherService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly int _workFactor;

        public PasswordHasherService(int workFactor = 12)
        {
            _workFactor = workFactor;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password, _workFactor);
        }

        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
            }
            catch
            {
                return false;
            }
        }
    }
}
