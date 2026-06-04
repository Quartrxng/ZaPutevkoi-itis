using System;
using System.Text;
using System.Text.Json;
using DAO;
using MiniHttpServer.FrameWork;
using MiniHttpServer.FrameWork.Core;
using MiniHttpServer.FrameWork.Core.Attributes;
using MiniHttpServer.FrameWork.Core.HttpResponse;
using MiniHttpServer.FrameWork.Shared;
using MiniHttpServer.Services;
using ModelsLibrary;
using ModelsLibrary.Admin;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    public class AdminApiEndpoint : EndpointBase
    {
        private readonly AdminAuthService _authService;
        private readonly HotelService _hotelService;
        private readonly string _connectionString;


        public AdminApiEndpoint()
        {
            _connectionString = Settings.Instance.Sql;
            _authService = new AdminAuthService(_connectionString);
            _hotelService = new HotelService(_connectionString);
        }

        private bool IsAuthorized()
        {
            string tokenCookie = Cookie.GetCookie("admin_session", Context.Request);
            if (string.IsNullOrEmpty(tokenCookie)) return false;

            byte[] token;
            try
            {
                token = Convert.FromBase64String(tokenCookie);
            }
            catch
            {
                return false;
            }

            return _authService.IsTokenValid(token);
        }


        [HttpGet("hotels")]
        public IHttpResult GetHotels()
        {
            if (!IsAuthorized()) return Unauthorized();

            try
            {
                var hotels = _hotelService.GetHotels();
                hotels = hotels.OrderBy(h => h.Id).ToList();
                foreach (var hotel in hotels) 
                { 
                    hotel.ServicesIds = _hotelService.GetIdsServices(hotel.Id).ToList();
                }
                return Page("Templates/AdminHotels.thtml", new { Hotels = hotels });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetHotels error: {ex.Message}");
                return Json(new { error = "Failed to load hotel" });
            }
        }

        [HttpGet("hotels/{id}")]
        public IHttpResult GetHotelById(int id)
        {
            if (!IsAuthorized()) return Unauthorized();

            try
            {
                var hotel = _hotelService.FindHotel(id);
                hotel.ServicesIds = _hotelService.GetIdsServices(hotel.Id).ToList();
                return Json(hotel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetHotelById error: {ex.Message}");
                return Json(new { error = "Failed to load hotel" });
            }
        }
        [HttpPut("hotels/{id}")]
        public IHttpResult UpdateHotel(
            int id,
            string? name = null,
            int stars = 0,
            string? rating = null,
            string? city = null,
            string? country = null,
            string? description = null,
            string? about_Hotel = null,
            string? card = null,
            string? meal = "[]",
            string? servicesIds = "[]",
            string? deletedImages = "[]",
            string? deletedRooms = "[]",
            string? photoFiles = "[]",
            string? rooms = "[]"
            
        )
        {
            if (!IsAuthorized()) return Unauthorized();
            try
            {
                var hotel = _hotelService.FindHotel(id);
                if (hotel == null) return Json(new { success = false, message = "Отель не найден" });

                hotel.Name = name ?? hotel.Name;
                hotel.Stars = stars;
                hotel.Rating = rating ?? hotel.Rating;
                hotel.City = city ?? hotel.City;
                hotel.Country = country ?? hotel.Country;
                hotel.Description = description ?? hotel.Description;
                hotel.About_Hotel = about_Hotel ?? hotel.About_Hotel;
                hotel.Card = card ?? hotel.Card;
                hotel.Meal = JsonSerializer.Deserialize<List<string>>(meal) ?? hotel.Meal;
                hotel.ServicesIds = JsonSerializer.Deserialize<List<int>>(servicesIds) ?? hotel.ServicesIds;

                var deletedImagesList = JsonSerializer.Deserialize<List<string>>(deletedImages) ?? new List<string>();
                var deletedRoomsList = JsonSerializer.Deserialize<List<int>>(deletedRooms) ?? new List<int>();
                var hotelPhotoFiles = JsonSerializer.Deserialize<List<PhotoFile>>(photoFiles, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<PhotoFile>();

                var roomsListDtos = JsonParser.ParseRooms(rooms) ?? new List<RoomDTO>();

                var (existingRooms, newRooms) = _hotelService.ProcessRooms(roomsListDtos, hotel);
                hotel.Hotel_Rooms = existingRooms;


                _hotelService.UpdateHotel(hotel, hotelPhotoFiles, deletedImagesList, deletedRoomsList, newRooms);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateHotel error: {ex}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("hotels")]
        public IHttpResult CreateHotel(
            string? name,
            int stars = 0,
            string? rating = "Нету",
            string? city = "Нету",
            string? country = "Нету",
            string? description = "Нету",
            string? about_Hotel = "Нету",
            string? card = null,
            string? meal = "[]",
            string? servicesIds = "[]",
            string? photoFiles = "[]",
            string? rooms = "[]"
        )
        {
            if (!IsAuthorized()) return Unauthorized();
            try
            {
                var hotel = new Hotel
                {
                    Name = name ?? "Без названия",
                    Stars = stars,
                    Rating = rating ?? "1.0",
                    City = city ?? "Город не указан",
                    Country = country ?? "Страна не указана",
                    Description = description ?? "",
                    About_Hotel = about_Hotel ?? "",
                    Meal = JsonSerializer.Deserialize<List<string>>(meal) ?? new List<string>(),
                    ServicesIds = JsonSerializer.Deserialize<List<int>>(servicesIds) ?? new List<int>(),
                    Image_Names = new List<string>(),
                    Card = card ?? ""
                };
                var hotelPhotoFiles = JsonSerializer.Deserialize<List<PhotoFile>>(photoFiles, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<PhotoFile>();

                var roomsListDtos = JsonParser.ParseRooms(rooms) ?? new List<RoomDTO>();

                _hotelService.CreateHotel(hotel, hotelPhotoFiles, roomsListDtos);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateHotel error: {ex}");
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpDelete("hotels/{id}")]
        public IHttpResult DeleteHotel(int id)
        {
            if (!IsAuthorized()) return Unauthorized();
            _hotelService.DeleteHotel(id);

            return Json(new { success = true });
        }

        private IHttpResult Unauthorized()
        {
            var response = new { error = "Unauthorized" };
            var json = JsonSerializer.Serialize(response);
            return new CustomHttpResponse(401, Encoding.UTF8.GetBytes(json), "application/json");
        }
    }
}