using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniHttpServer.FrameWork.Shared
{
    public class JsonParser
    {
        public static List<RoomDTO> ParseRooms(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                var rooms = JsonSerializer.Deserialize<List<RoomDTO>>(json, options);
                return rooms ?? new List<RoomDTO>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Ошибка парсинга JSON: {ex.Message}");
                return new List<RoomDTO>();
            }
        }
    }
}
