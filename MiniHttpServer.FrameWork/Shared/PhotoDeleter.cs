using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.FrameWork.Shared
{
    public static class PhotoDeleter
    {
        public static bool DeleteHotelImage(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Console.WriteLine($"Файл удален: {path}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Файл не найден: {path}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении файла отеля: {ex.Message}");
                return false;
            }
        }
    }
}
