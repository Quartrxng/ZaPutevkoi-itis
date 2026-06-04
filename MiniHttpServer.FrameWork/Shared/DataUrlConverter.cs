using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniHttpServer.FrameWork.Shared
{
    public class DataUrlConverter
    {
        public static bool TryConvertDataUrlToJpg(string dataUrl, string outputPath)
        {

            try
            {
                var match = Regex.Match(dataUrl, @"^data:image/jpeg;base64,(.+)$");
                if (!match.Success)
                {
                    Console.WriteLine("Некорректный формат DataURL для JPEG");
                    return false;
                }

                string base64Data = match.Groups[1].Value;

                if (string.IsNullOrWhiteSpace(base64Data))
                {
                    Console.WriteLine("Base64 данные отсутствуют");
                    return false;
                }

                byte[] imageBytes = Convert.FromBase64String(base64Data);
                File.WriteAllBytes(outputPath, imageBytes);

                if (!File.Exists(outputPath))
                {
                    Console.WriteLine("Не удалось создать файл");
                    return false;
                }

                return true;
            }
            catch (FormatException)
            {
                Console.WriteLine("Некорректные base64 данные");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return false;
            }
        }
    }
}
