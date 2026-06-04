using DAO.Repositories;
using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DAO
{
    public class HotelMapper : IHotelMapper
    {
        public HotelDTO MapToDTO(Hotel hotel)
        {
            if (hotel == null) return null;

            return new HotelDTO
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Description = hotel.Description,
                City = hotel.City,
                Country = hotel.Country,
                Stars = hotel.Stars,
                Rating = double.TryParse(hotel.Rating, NumberStyles.Any, CultureInfo.InvariantCulture, out double temp) ? temp : 0,
                About_Hotel = hotel.About_Hotel,
                Meal = hotel.Meal != null ? string.Join(", ", hotel.Meal) : string.Empty,
                Note = hotel.Note,
                Image_Names = hotel.Image_Names != null ? string.Join(",", hotel.Image_Names) : string.Empty,
                Card = hotel.Card
            };
        }

        public Hotel MapToEntity(HotelDTO dto)
        {
            if (dto == null) return null;

            return new Hotel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                City = dto.City,
                Country = dto.Country,
                Stars = dto.Stars,
                Rating = dto.Rating.ToString("0.0", CultureInfo.InvariantCulture),
                About_Hotel = dto.About_Hotel,
                Meal = !string.IsNullOrEmpty(dto.Meal)
                    ? dto.Meal.Split(',').Select(m => m.Trim()).Where(m => !string.IsNullOrWhiteSpace(m)).ToList()
                    : new List<string>(),
                Note = dto.Note,
                Image_Names = !string.IsNullOrEmpty(dto.Image_Names)
                    ? dto.Image_Names.Split(',').Select(i => i.Trim()).Where(i => !string.IsNullOrWhiteSpace(i)).ToList()
                    : new List<string>(),
                Hotel_Rooms = new List<Hotel_room>(),
                Max_Size_Room = 0,
                Price = 0,
                RatingTag = dto.Rating < 1 ? "verybad" :
                            dto.Rating < 2 ? "bad" :
                            dto.Rating < 3 ? "normal" :
                            dto.Rating < 4 ? "good" : "verygood",
                Card = dto.Card,
            };
        }

        public Hotel MapToFullHotel(HotelDTO dto, List<Hotel_room> rooms)
        {
            if (dto == null) return null;

            var hotel = MapToEntity(dto);
            hotel.Hotel_Rooms = rooms ?? new List<Hotel_room>();
            hotel.Max_Size_Room = CalculateMaxSizeRoom(hotel);
            hotel.Price = CalculateMinPrice(hotel);

            return hotel;
        }
        public int CalculateMaxSizeRoom(Hotel hotel)
        {
            return hotel.Hotel_Rooms?.Any() == true
                ? hotel.Hotel_Rooms.Max(r => r.PeopleCount)
                : 0;
        }

        public int CalculateMinPrice(Hotel hotel)
        {
            return hotel.Hotel_Rooms?.Any() == true
                ? hotel.Hotel_Rooms.Min(r => r.Price)
                : 0;
        }
    }
}
