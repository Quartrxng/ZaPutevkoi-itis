using DAO;
using ModelsLibrary.Admin;
using MyORMLibrary;
using Npgsql;
using System;
using System.Security.Cryptography;

public static class AdminSeeder
{
    public static void CreateDefaultAdmin(string connectionString)
    {
        const string defaultUsername = "admin";
        const string defaultPassword = "12345";

        DeleteExistingAdmin(defaultUsername, connectionString);

        var (hash, salt) = HashPassword(defaultPassword);

        var adminUserDao = new AdminUserDAO(connectionString);

        var admin = new AdminUser
        {
            Username = defaultUsername,
            Password_Hash = hash,
            Password_Salt = salt
        };

        adminUserDao.Create(admin);
        Console.WriteLine($"✅ Администратор '{defaultUsername}' создан.");
        Console.WriteLine($"   Hash: {BitConverter.ToString(hash).Replace("-", "").ToLower()}");
        Console.WriteLine($"   Salt: {BitConverter.ToString(salt).Replace("-", "").ToLower()}");
    }

    private static void DeleteExistingAdmin(string username, string connectionString)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        var sql = "DELETE FROM \"AdminUsers\" WHERE username = @username";
        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@username", username);
        int rowsAffected = cmd.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            Console.WriteLine($"🗑️ Старый администратор '{username}' удалён.");
        }
    }

    private static (byte[] hash, byte[] salt) HashPassword(string password)
    {
        const int saltSize = 32; // 256 бит
        const int iterations = 10000;
        const int hashSize = 32; // 256 бит

        byte[] salt = RandomNumberGenerator.GetBytes(saltSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(hashSize);

        return (hash, salt);
    }
}