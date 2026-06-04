using System;
using System.Security.Cryptography;
using DAO;
using DAO.Interfaces;
using ModelsLibrary.Admin;
using MyORMLibrary;

namespace MiniHttpServer.Services
{
    public class AdminAuthService
    {
        private readonly IAdminUserDAO _adminUserDao;
        private readonly IAdminSessionDAO _adminSessionDao;

        public AdminAuthService(string connectionString)
        {
            _adminUserDao = new AdminUserDAO(connectionString);
            _adminSessionDao = new AdminSessionDAO(connectionString);
        }

        // Метод для входа
        public (bool success, string message, byte[] token) Login(string username, string password)
        {
            // Проверяем логин и пароль
            if (!_adminUserDao.ValidatePassword(username, password))
            {
                return (false, "Invalid credentials", null);
            }

            // Получаем пользователя
            var user = _adminUserDao.GetByUsername(username);
            if (user == null)
            {
                return (false, "User not found", null);
            }

            // Генерируем токен
            byte[] token = GenerateSecureToken();

            // Создаём сессию
            var session = new AdminSession
            {
                Token = token,
                User_Id = user.Id,
                Expires_At = DateTime.UtcNow.AddDays(7) // 7 дней
            };

            _adminSessionDao.Create(session);

            return (true, "Login successful", token);
        }

        // Метод для проверки токена
        public bool IsTokenValid(byte[] token)
        {
            return _adminSessionDao.IsTokenValid(token);
        }

        // Метод для выхода
        public void Logout(byte[] token)
        {
            _adminSessionDao.DeleteByToken(token);
        }

        // Метод для удаления просроченных сессий
        public void DeleteExpiredSessions()
        {
            _adminSessionDao.DeleteExpiredSessions();
        }

        private static byte[] GenerateSecureToken()
        {
            byte[] token = new byte[32]; // 256 бит
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(token);
            }
            return token;
        }
    }
}
