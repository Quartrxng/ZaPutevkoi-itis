using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelsLibrary;
using MyORMLibrary;
using Npgsql;
using System.Collections.Generic;

namespace MyORMLibraryTests
{
    [TestClass]
    [DoNotParallelize]
    public class ORMContextTests
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=YOUR_DB_PASSWORD;Database=test_db;Pooling=false;";
        private ORMContext _context;

        [TestInitialize]
        public void Setup()
        {
            _context = new ORMContext(ConnectionString);
            // Создаём таблицу один раз, если не существует
            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS ""Users"" (
                    ""id"" SERIAL PRIMARY KEY,
                    ""name"" VARCHAR(100),
                    ""password"" VARCHAR(100),
                    ""email"" VARCHAR(100),
                    ""username"" VARCHAR(50),
                    ""age"" INT
                ); TRUNCATE TABLE ""Users"" RESTART IDENTITY;", conn);
            cmd.ExecuteNonQuery();
        }

        [TestCleanup]
        public void Cleanup()
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(@"TRUNCATE TABLE ""Users"" RESTART IDENTITY;", conn);
            cmd.ExecuteNonQuery();
        }

        [TestMethod]
        public void Create_ShouldInsertRecord()
        {
            var user = new User
            {
                Name = "Alice",
                Password = "pass123",
                Email = "alice@test.com",
                UserName = "alice123",
                Age = 25
            };

            _context.Create<User>(user);

            List<User> users = _context.ReadByAll<User>();

            Assert.AreEqual(1, users.Count);
            Assert.AreEqual("Alice", users[0].Name);
            Assert.AreEqual("pass123", users[0].Password);
            Assert.AreEqual("alice@test.com", users[0].Email);
            Assert.AreEqual("alice123", users[0].UserName);
            Assert.AreEqual(25, users[0].Age);
        }

        [TestMethod]
        public void ReadById_ShouldReturnCorrectRecord()
        {
            var user = new User
            {
                Name = "Bob",
                Password = "bobpass",
                Email = "bob@test.com",
                UserName = "bob123",
                Age = 30
            };
            _context.Create<User>(user);

            var users = _context.ReadByAll<User>();
            int id = users[0].Id;

            var fetched = _context.ReadById<User>(id);

            Assert.IsNotNull(fetched);
            Assert.AreEqual("Bob", fetched.Name);
            Assert.AreEqual("bobpass", fetched.Password);
            Assert.AreEqual("bob@test.com", fetched.Email);
            Assert.AreEqual("bob123", fetched.UserName);
            Assert.AreEqual(30, fetched.Age);
        }

        [TestMethod]
        public void Update_ShouldModifyExistingRecord()
        {
            var user = new User
            {
                Name = "Charlie",
                Password = "charliepass",
                Email = "charlie@test.com",
                UserName = "charlie123",
                Age = 20
            };
            _context.Create(user);

            var users = _context.ReadByAll<User>();
            int id = users[0].Id;

            user.Name = "Charlie Updated";
            user.Password = "newpass";
            user.Email = "updated@test.com";
            user.UserName = "charlieUpdated";
            user.Age = 22;

            _context.Update<User>(id, user);

            var updated = _context.ReadById<User>(id);
            Assert.AreEqual("Charlie Updated", updated.Name);
            Assert.AreEqual("newpass", updated.Password);
            Assert.AreEqual("updated@test.com", updated.Email);
            Assert.AreEqual("charlieUpdated", updated.UserName);
            Assert.AreEqual(22, updated.Age);
        }

        [TestMethod]
        public void Delete_ShouldRemoveRecord()
        {
            var user = new User
            {
                Name = "David",
                Password = "davidpass",
                Email = "david@test.com",
                UserName = "david123",
                Age = 40
            };
            _context.Create(user);

            var users = _context.ReadByAll<User>();
            int id = users[0].Id;

            _context.Delete<User>(id);

            var remaining = _context.ReadByAll<User>();
            Assert.AreEqual(0, remaining.Count);
        }

        [TestMethod]
        public void ReadByAll_ShouldReturnAllRecords()
        {
            _context.Create(new User { Name = "User1", Password = "p1", Email = "u1@test.com", UserName = "u1", Age = 18 });
            _context.Create(new User { Name = "User2", Password = "p2", Email = "u2@test.com", UserName = "u2", Age = 19 });

            var users = _context.ReadByAll<User>();

            Assert.AreEqual(2, users.Count);
        }
    }
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int Age { get; set; }
    }
}