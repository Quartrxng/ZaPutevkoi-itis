using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniHttpServer.FrameWork;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MiniHttpServer.Framework.UnitTests
{
    [TestClass]
    public sealed class HttpServerTest
    {
        private static HttpServer? _server;
        private static HttpClient? _httpClient;
        private static string _baseUrl = "http://localhost:9999";
        private static string _publicDir = "test_public";

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            CreateTestSettings();
            CreateTestPublicDirectory();

            _server = new HttpServer();
            bool keepAlive = false;
            _server.Start(ref keepAlive);

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };

            Thread.Sleep(1000);
        }

        [ClassCleanup]
        public static void ClassTeardown()
        {
            bool keepRunning = true;
            _server?.Stop(ref keepRunning);
            _httpClient?.Dispose();
            Thread.Sleep(500);
            CleanupTestFiles();
        }

        [TestMethod]
        public void Server_Should_Start_Successfully()
        {
            // Assert
            Assert.IsNotNull(_server);
            Assert.IsNotNull(_httpClient);
        }

        [TestMethod]
        public async Task Server_Should_Respond_To_Html_Request()
        {
            // Arrange
            var testFile = Path.Combine(_publicDir, "test.html");
            File.WriteAllText(testFile, "<html><body>Test</body></html>");

            // Act
            var response = await _httpClient!.GetAsync("/test.html");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(content.Contains("Test"));
        }

        [TestMethod]
        public async Task NonExistent_File_Should_Return_404()
        {
            // Arrange

            // Act
            var response = await _httpClient!.GetAsync("/nonexistent.html");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Html_File_Should_Have_Correct_ContentType()
        {
            // Arrange
            var testFile = Path.Combine(_publicDir, "page.html");
            File.WriteAllText(testFile, "<html><body>Page</body></html>");

            // Act
            var response = await _httpClient!.GetAsync("/page.html");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(response.Content.Headers.ContentType?.MediaType?.Contains("text/html") ?? false);
        }

        [TestMethod]
        public async Task Css_File_Should_Have_Correct_ContentType()
        {
            // Arrange
            var testFile = Path.Combine(_publicDir, "style.css");
            File.WriteAllText(testFile, "body { margin: 0; }");

            // Act
            var response = await _httpClient!.GetAsync("/style.css");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(response.Content.Headers.ContentType?.MediaType?.Contains("text/css") ?? false);
        }

        [TestMethod]
        public async Task JavaScript_File_Should_Have_Correct_ContentType()
        {
            // Arrange
            var testFile = Path.Combine(_publicDir, "script.js");
            File.WriteAllText(testFile, "console.log('test');");

            // Act
            var response = await _httpClient!.GetAsync("/script.js");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(response.Content.Headers.ContentType?.MediaType?.Contains("application/javascript") ?? false);
        }

        [TestMethod]
        public async Task Json_File_Should_Have_Correct_ContentType()
        {
            // Arrange
            var jsonContent = JsonSerializer.Serialize(new { test = "value" });
            var testFile = Path.Combine(_publicDir, "data.json");
            File.WriteAllText(testFile, jsonContent);

            // Act
            var response = await _httpClient!.GetAsync("/data.json");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(response.Content.Headers.ContentType?.MediaType?.Contains("application/json") ?? false);
        }

        [TestMethod]
        public async Task File_In_Subdirectory_Should_Be_Accessible()
        {
            // Arrange
            var subDir = Path.Combine(_publicDir, "assets");
            Directory.CreateDirectory(subDir);
            var testFile = Path.Combine(subDir, "page.html");
            File.WriteAllText(testFile, "<html><body>Subdirectory</body></html>");

            // Act
            var response = await _httpClient!.GetAsync("/assets/page.html");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(content.Contains("Subdirectory"));
        }

        [TestMethod]
        public async Task Response_Should_Have_Correct_ContentLength()
        {
            // Arrange
            var testContent = "Hello, World!";
            var testFile = Path.Combine(_publicDir, "length.txt");
            File.WriteAllText(testFile, testContent);

            // Act
            var response = await _httpClient!.GetAsync("/length.txt");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(testContent.Length, response.Content.Headers.ContentLength);
        }

        private static void CreateTestSettings()
        {
            var settings = new
            {
                PublicDirectoryPath = _publicDir,
                Domain = "localhost",
                Port = "9999",
                Sql = "Host=localhost;Port=5432;Database=TestDb;Username=test;Password=test"
            };

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText("settings.json", json);
        }

        private static void CreateTestPublicDirectory()
        {
            if (Directory.Exists(_publicDir))
            {
                Directory.Delete(_publicDir, true);
                Thread.Sleep(100);
            }
            Directory.CreateDirectory(_publicDir);
        }

        private static void CleanupTestFiles()
        {
            try
            {
                if (Directory.Exists(_publicDir))
                {
                    Directory.Delete(_publicDir, true);
                }
                if (File.Exists("settings.json"))
                {
                    File.Delete("settings.json");
                }
            }
            catch { }
        }
    }
}