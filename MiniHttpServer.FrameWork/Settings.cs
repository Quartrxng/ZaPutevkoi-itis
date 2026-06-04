using System;
using System.IO;
using System.Text.Json;

namespace MiniHttpServer.FrameWork
{
    public sealed class Settings
    {
        private static Settings _instance;
        public static Settings Instance => _instance ??= LoadSettings();

        public string PublicDirectoryPath { get; set; }
        public string Domain { get; set; }
        public string Port { get; set; }
        public string Sql { get; set; }

        private class SettingsData
        {
            public string PublicDirectoryPath { get; set; }
            public string Domain { get; set; }
            public string Port { get; set; }
            public string Sql { get; set; }
        }


        private Settings(string publicDirectoryPath, string domain, string port, string sql)
        {
            PublicDirectoryPath = publicDirectoryPath;
            Domain = domain;
            Port = port;
            Sql = sql;
        }

        private static Settings LoadSettings()
        {
            try
            {
                var json = File.ReadAllText("settings.json");
                var settingsData = JsonSerializer.Deserialize<SettingsData>(json);

                return new Settings(
                    settingsData.PublicDirectoryPath,
                    settingsData.Domain,
                    settingsData.Port,
                    settingsData.Sql
                );
            }


            catch (FileNotFoundException)
            {
                Console.WriteLine("Файл settings.json не найден");
                return CreateDefaultSettings();
            }
            catch (JsonException)
            {
                Console.WriteLine("Ошибка формата JSON");
                return CreateDefaultSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
                return CreateDefaultSettings();
            }
        }

        private static Settings CreateDefaultSettings()
        {
            return new Settings("public", "localhost", "1337", "Host=localhost;Port=5432;Database=HttpServer;Username=Admin;Password=123456789");
        }
    }
}