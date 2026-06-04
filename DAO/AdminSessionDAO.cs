using DAO.Interfaces;
using ModelsLibrary.Admin;
using MyORMLibrary;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO
{
    public class AdminSessionDAO : GenericDAO<AdminSession>, IAdminSessionDAO
    {
        public AdminSessionDAO(string connectionString) : base(connectionString) { }

        public AdminSession? GetByToken(byte[] token)
        {
            try
            {
                Console.WriteLine($"[GetByToken] Looking for token, length: {token?.Length}");

                if (token == null || token.Length == 0)
                    return null;

                // Получаем все активные сессии и фильтруем в памяти
                var activeSessions = _context.ReadByAll<AdminSession>()
                    .Where(s => s.Expires_At > DateTime.UtcNow)
                    .ToList();

                Console.WriteLine($"[GetByToken] Found {activeSessions.Count} active sessions");

                // Ищем сессию с совпадающим токеном
                foreach (var session in activeSessions)
                {
                    if (session.Token != null && session.Token.Length == token.Length)
                    {
                        bool isEqual = true;
                        for (int i = 0; i < token.Length; i++)
                        {
                            if (token[i] != session.Token[i])
                            {
                                isEqual = false;
                                break;
                            }
                        }

                        if (isEqual)
                        {
                            Console.WriteLine($"[GetByToken] Token found in session ID: {session.Id}");
                            return session;
                        }
                    }
                }

                Console.WriteLine($"[GetByToken] Token not found in active sessions");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetByToken] Error: {ex.Message}");
                return null;
            }
        }

        public void DeleteExpiredSessions()
        {
            try
            {
                string sql = $"DELETE FROM \"AdminSession\" WHERE \"expires_at\" < @expired";

                var parameters = new[]
                {
                    new NpgsqlParameter("@expired", DateTime.UtcNow)
                };

                int deletedCount = _context.ExecuteNonQuery(sql, parameters);
                Console.WriteLine($"[DeleteExpiredSessions] Deleted {deletedCount} expired sessions");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteExpiredSessions] Error: {ex.Message}");
                throw;
            }
        }

        public bool IsTokenValid(byte[] token)
        {
            try
            {
                Console.WriteLine($"[IsTokenValid] Validating token, length: {token?.Length}");
                var session = GetByToken(token);

                if (session == null)
                {
                    Console.WriteLine($"[IsTokenValid] No session found for token");
                    return false;
                }

                bool isValid = session.Expires_At > DateTime.UtcNow;
                Console.WriteLine($"[IsTokenValid] Session found, expires at: {session.Expires_At}, valid: {isValid}");

                return isValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IsTokenValid] Error: {ex.Message}");
                return false;
            }
        }

        public void DeleteByToken(byte[] token)
        {
            try
            {
                Console.WriteLine($"[DeleteByToken] Deleting session by token, length: {token?.Length}");

                // Сначала находим сессию чтобы получить ID для логирования
                var session = GetByToken(token);
                if (session != null)
                {
                    string sql = $"DELETE FROM \"AdminSession\" WHERE \"id\" = @id";

                    var parameters = new[]
                    {
                        new NpgsqlParameter("@id", session.Id)
                    };

                    int deletedCount = _context.ExecuteNonQuery(sql, parameters);
                    Console.WriteLine($"[DeleteByToken] Deleted session ID: {session.Id}, affected rows: {deletedCount}");
                }
                else
                {
                    Console.WriteLine($"[DeleteByToken] Session not found for token");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteByToken] Error: {ex.Message}");
                throw;
            }
        }

        // Альтернативный метод для массового удаления по токену (если нужно)
        public void DeleteByTokenDirect(byte[] token)
        {
            try
            {
                // Если ваша база данных поддерживает сравнение bytea, можно использовать этот метод
                string sql = $"DELETE FROM \"AdminSession\" WHERE \"token\" = @token";

                var parameters = new[]
                {
                    new NpgsqlParameter("@token", token)
                };

                int deletedCount = _context.ExecuteNonQuery(sql, parameters);
                Console.WriteLine($"[DeleteByTokenDirect] Deleted sessions by token, affected rows: {deletedCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteByTokenDirect] Error: {ex.Message}");
                throw;
            }
        }
    }
}