using DAO.Interfaces;
using DAO.Repositories;
using ModelsLibrary.Admin;
using MyORMLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DAO
{
    public class AdminUserDAO : GenericDAO<AdminUser>, IAdminUserDAO
    {
        public AdminUserDAO(string connectionString) : base(connectionString) { }

        public AdminUser? GetByUsername(string username)
        {
            return FirstOrDefault(u => u.Username == username);
        }

        public bool ValidatePassword(string username, string password)
        {
            var user = GetByUsername(username);
            if (user == null)
            {
                Console.WriteLine($"User '{username}' not found");
                return false;
            }

            Console.WriteLine($"User '{username}' found, validating password...");
            bool isValid = VerifyPassword(password, user.Password_Hash, user.Password_Salt);
            Console.WriteLine($"Password valid: {isValid}");
            return isValid;
        }

        private static bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            const int iterations = 10000;
            const int hashSize = 32;

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] inputHash = pbkdf2.GetBytes(hashSize);

            return CryptographicOperations.FixedTimeEquals(hash, inputHash);
        }
    }
}
