using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.FrameWork.Shared
{
    public static class FileHelper
    {
        public static string? SaveBase64FileAs(string dataUrl, string targetDirectory, string fileName)
        {
            if (string.IsNullOrEmpty(dataUrl) || string.IsNullOrEmpty(targetDirectory) || string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine("SaveBase64FileAs: Недостаточно данных для сохранения файла.");
                return null;
            }

            try
            {
                string base64Data = dataUrl.Substring(dataUrl.IndexOf(",") + 1);
                byte[] imageBytes = Convert.FromBase64String(base64Data);
                string fullPath = Path.Combine(targetDirectory, fileName);
                Directory.CreateDirectory(targetDirectory);

                File.WriteAllBytes(fullPath, imageBytes);

                Console.WriteLine($"Файл сохранён: {fullPath}");
                return fullPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении файла {fileName}: {ex.Message}");
                return null;
            }
        }
    }
}
