using DAO.Interfaces;
using DAO.Repositories;
using ModelsLibrary;
using MyORMLibrary;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DAO
{
    public class HotelDAO : GenericDAO<Hotel>, IHotelDAO
    {
        private readonly IHotelMapper _mapper;
        private readonly IHotelFilterBuilder _filterBuilder;

        public HotelDAO(string connectionString, IHotelMapper mapper, IHotelFilterBuilder filterBuilder)
            : base(connectionString)
        {
            _mapper = mapper;
            _filterBuilder = filterBuilder;
        }

        public List<Hotel> GetHotelsWithRooms(string query = null)
        {
            var hotelsDTOs = GetHotelsDTOsWithRooms(query);
            if (hotelsDTOs == null) { return null; }
            var hotelIds = hotelsDTOs.Select(h => h.Id).ToList();
            var allRooms = GetRoomsByHotelIds(hotelIds);
            var roomsByHotel = allRooms.GroupBy(r => r.Hotel_Id)
                .ToDictionary(g => g.Key, g => g.ToList());
            var fullHotels = new List<Hotel>();
            foreach (var hotelDTO in hotelsDTOs)
            {
                var rooms = roomsByHotel.ContainsKey(hotelDTO.Id) ? roomsByHotel[hotelDTO.Id] : new List<Hotel_room>();
                fullHotels.Add(_mapper.MapToFullHotel(hotelDTO, rooms));
            }
            return fullHotels;

        }

        private List<HotelDTO> GetHotelsDTOsWithRooms(string query)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                string sql = @"
                    SELECT ""id"", ""name"", ""description"", ""city"", ""country"", ""stars"", ""rating"", 
                           ""about_hotel"", ""meal"", ""note"", ""image_names"", ""card""
                    FROM ""Hotels"" 
                    WHERE ""name"" ILIKE @query || '%'";


                var parameters = new[] { new NpgsqlParameter("@query", query) };
                return _context.ExecuteQuery<HotelDTO>(sql, parameters);
            }
            else
            {
                string sql = @"
                    SELECT ""id"", ""name"", ""description"", ""city"", ""country"", ""stars"", ""rating"", 
                           ""about_hotel"", ""meal"", ""note"", ""image_names"", ""card""
                    FROM ""Hotels""";


                return _context.ExecuteQuery<HotelDTO>(sql);
            }
        }

        public Hotel GetHotelWithRooms(int hotelId)
        {
            var hotelDTO = GetHotelDTOById(hotelId);
            if (hotelDTO == null) return null;
            var rooms = GetRoomsByHotelIds(new List<int> { hotelId });
            return _mapper.MapToFullHotel(hotelDTO, rooms);
        }

        public IEnumerable<Hotel> GetHotelsStartingWith(string query, int limit = 0)
        {
            var hotelDTOs = GetHotelDTOsStartingWith(query, limit).ToList();
            if (!hotelDTOs.Any()) return new List<Hotel>();
            var hotelIds = hotelDTOs.Select(h => h.Id).ToList();
            var allRooms = GetRoomsByHotelIds(hotelIds);
            var roomsByHotel = allRooms.GroupBy(r => r.Hotel_Id)
                .ToDictionary(g => g.Key, g => g.ToList());
            var fullHotels = new List<Hotel>();
            foreach (var hotelDTO in hotelDTOs)
            {
                var rooms = roomsByHotel.ContainsKey(hotelDTO.Id) ? roomsByHotel[hotelDTO.Id] : new List<Hotel_room>();
                fullHotels.Add(_mapper.MapToFullHotel(hotelDTO, rooms));
            }
            return fullHotels;
        }

        public IEnumerable<Hotel> GetHotelsByFilter(HotelFilter filter)
        {
            var (sql, parameters) = _filterBuilder.BuildFilterQuery(filter);
            var hotelDTOs = _context.ExecuteQuery<HotelDTO>(sql, parameters.ToArray()).ToList();
            if (!hotelDTOs.Any()) return new List<Hotel>();

            var hotelIds = hotelDTOs.Select(h => h.Id).ToList();
            var allRooms = GetRoomsByHotelIds(hotelIds);

            var roomsByHotel = allRooms
                .GroupBy(r => r.Hotel_Id)
                .ToDictionary(g => g.Key, g => g.ToList());

            var fullHotels = new List<Hotel>();
            foreach (var hotelDTO in hotelDTOs)
            {
                var rooms = roomsByHotel.ContainsKey(hotelDTO.Id)
                    ? roomsByHotel[hotelDTO.Id]
                    : new List<Hotel_room>();

                var hotel = _mapper.MapToFullHotel(hotelDTO, rooms);
                fullHotels.Add(hotel);
            }

            IEnumerable<Hotel> query = fullHotels;

            if (filter.TouristsCount > 0)
                query = query.Where(h => h.Max_Size_Room >= filter.TouristsCount);

            return query.ToList();
        }

        public Hotel GetFullHotel(int hotelId)
        {
            var hotelDTO = GetHotelDTOById(hotelId);
            if (hotelDTO == null) return null;
            var rooms = GetRoomsByHotelIds(new List<int> { hotelId });
            return _mapper.MapToFullHotel(hotelDTO, rooms);
        }
        public IEnumerable<Hotel> GetFullHotelsStartingWith(string query, int limit = 0)
        {
            return GetHotelsStartingWith(query, limit);
        }
        private HotelDTO GetHotelDTOById(int hotelId)
        {
            string sql = @"
                SELECT ""id"", ""name"", ""description"", ""city"", ""country"", ""stars"", ""rating"", 
                        ""about_hotel"", ""meal"", ""note"", ""image_names"", ""card""
                FROM ""Hotels"" 
                WHERE ""id"" = @hotelId";

            var parameters = new[] { new NpgsqlParameter("@hotelId", hotelId) };
            return _context.ExecuteQuery<HotelDTO>(sql, parameters).FirstOrDefault();
        }
        public IEnumerable<HotelDTO> GetHotelDTOsStartingWith(string query, int limit = 0)
        {
            string limitClause = limit == 0 ? "" : $"LIMIT {limit}";
            string sql = $@"
                SELECT ""id"", ""name"", ""description"", ""city"", ""country"", ""stars"", ""rating"", 
                       ""about_hotel"", ""meal"", ""note"", ""image_names"", ""card""
                FROM ""Hotels"" 
                WHERE ""name"" ILIKE @query || '%' 
                {limitClause}".Trim();


            var parameters = new[] { new NpgsqlParameter("@query", query) };
            return _context.ExecuteQuery<HotelDTO>(sql, parameters);
        }
        private List<Hotel_room> GetRoomsByHotelIds(List<int> hotelIds)
        {
            if (!hotelIds.Any())
                return new List<Hotel_room>();
            var parameters = new List<NpgsqlParameter>();
            var placeholders = new List<string>();
            for (int i = 0; i < hotelIds.Count; i++)
            {
                placeholders.Add($"@id{i}");
                parameters.Add(new NpgsqlParameter($"@id{i}", hotelIds[i]));
            }
            string sql = $@"
                SELECT * FROM ""Hotel_rooms"" 
                WHERE ""hotel_id"" IN ({string.Join(", ", placeholders)})";
            return _context.ExecuteQuery<Hotel_room>(sql, parameters.ToArray());
        }

        public void UpdateHotel(Hotel hotel)
        {
            double rating = 1.0;
            if (!double.TryParse(hotel.Rating, NumberStyles.Any, CultureInfo.InvariantCulture, out rating))
            {
                rating = 1.0;
            }

            string sql = @"
                UPDATE ""Hotels"" 
                SET 
                    ""name"" = @Name,
                    ""description"" = @Description,
                    ""city"" = @City,
                    ""country"" = @Country,
                    ""stars"" = @Stars,
                    ""rating"" = @Rating,
                    ""max_size_room"" = @Max_Size_Room,
                    ""about_hotel"" = @About_Hotel,
                    ""meal"" = @Meal,
                    ""note"" = @Note,
                    ""image_names"" = @Image_Names,
                    ""card"" = @Card
                WHERE ""id"" = @Id";

                    var parameters = new[]
                    {
                new NpgsqlParameter("@Name", hotel.Name ?? (object)DBNull.Value),
                new NpgsqlParameter("@Description", hotel.Description ?? (object)DBNull.Value),
                new NpgsqlParameter("@City", hotel.City ?? (object)DBNull.Value),
                new NpgsqlParameter("@Country", hotel.Country ?? (object)DBNull.Value),
                new NpgsqlParameter("@Stars", hotel.Stars),
                new NpgsqlParameter("@Rating", rating),
                new NpgsqlParameter("@Max_Size_Room", hotel.Max_Size_Room),
                new NpgsqlParameter("@About_Hotel", hotel.About_Hotel ?? (object)DBNull.Value),
                new NpgsqlParameter("@Meal", hotel.Meal != null ? string.Join(", ", hotel.Meal) : DBNull.Value),
                new NpgsqlParameter("@Note", hotel.Note ?? (object)DBNull.Value),
                new NpgsqlParameter("@Image_Names", hotel.Image_Names != null ? string.Join(", ", hotel.Image_Names): DBNull.Value),
                new NpgsqlParameter("@Card", hotel.Card ?? (object)DBNull.Value),
                new NpgsqlParameter("@Id", hotel.Id)
            };

            _context.ExecuteNonQuery(sql, parameters);
        }

        public void UpdateRooms(List<Hotel_room> hotel_Rooms, List<int> deletedRooms)
        {
            foreach(Hotel_room room in hotel_Rooms)
            {
                _context.Update(room.Id, room);
            }
            foreach(var id in deletedRooms)
            {
                _context.Delete<Hotel_room>(id);
            }
        }
        public void CreateRooms(List<Hotel_room> rooms, int hotelId)
        {
            foreach (var room in rooms)
            {
                                string sql = @"
                        INSERT INTO ""Hotel_rooms"" 
                            (""name"", ""price"", ""description"", ""photo"", ""peoplecount"", ""hotel_id"")
                        VALUES 
                            (@Name, @Price, @Description, @Photo, @PeopleCount, @HotelId)";

                                var parameters = new[]
                                {
                        new NpgsqlParameter("@Name", room.Name ?? (object)DBNull.Value),
                        new NpgsqlParameter("@Price", room.Price),
                        new NpgsqlParameter("@Description", room.Description ?? (object)DBNull.Value),
                        new NpgsqlParameter("@Photo", room.Photo ?? (object)DBNull.Value),
                        new NpgsqlParameter("@PeopleCount", room.PeopleCount),
                        new NpgsqlParameter("@HotelId", hotelId)
                    };

                _context.ExecuteNonQuery(sql, parameters);
            }
        }
        public int CreateHotel(Hotel hotel)
        {
            double rating = 1.0;
            if (!double.TryParse(hotel.Rating, NumberStyles.Any, CultureInfo.InvariantCulture, out rating))
            {
                rating = 1.0;
            }

            string sql = @"
                INSERT INTO ""Hotels"" 
                    (""name"", ""description"", ""city"", ""country"", ""stars"", ""rating"", 
                     ""max_size_room"", ""about_hotel"", ""meal"", ""note"", ""image_names"", ""card"")
                VALUES 
                    (@Name, @Description, @City, @Country, @Stars, @Rating, @Max_Size_Room, @About_Hotel, @Meal, @Note, @Image_Names, @Card)
                RETURNING ""id"";";

                    var parameters = new[]
                    {
                new NpgsqlParameter("@Name", hotel.Name ?? (object)DBNull.Value),
                new NpgsqlParameter("@Description", hotel.Description ?? (object)DBNull.Value),
                new NpgsqlParameter("@City", hotel.City ?? (object)DBNull.Value),
                new NpgsqlParameter("@Country", hotel.Country ?? (object)DBNull.Value),
                new NpgsqlParameter("@Stars", hotel.Stars),
                new NpgsqlParameter("@Rating", rating),
                new NpgsqlParameter("@Max_Size_Room", hotel.Max_Size_Room),
                new NpgsqlParameter("@About_Hotel", hotel.About_Hotel ?? (object)DBNull.Value),
                new NpgsqlParameter("@Meal", hotel.Meal != null ? string.Join(", ", hotel.Meal) : DBNull.Value),
                new NpgsqlParameter("@Note", hotel.Note ?? (object)DBNull.Value),
                new NpgsqlParameter("@Image_Names", hotel.Image_Names != null ? (object)hotel.Image_Names : DBNull.Value),
                new NpgsqlParameter("@Card", hotel.Card ?? (object)DBNull.Value)
            };

            int hotelId = _context.ExecuteScalar<int>(sql, parameters);
            return hotelId;
        }

        public Hotel DeleteHotel(int id)
        {
            var result = GetFullHotel(id);
            _context.Delete<Hotel>(id);
            DeleteHotelRoomsByHotelId(id);
            return result;
        }
        public void DeleteHotelRoomsByHotelId(int hotelId)
        {
            string sql = @"DELETE FROM ""Hotel_rooms"" WHERE ""hotel_id"" = @HotelId";

            var parameters = new[]
            {
        new NpgsqlParameter("@HotelId", hotelId)
        };

            _context.ExecuteNonQuery(sql, parameters);
        }
        public Hotel_room GetHotel_RoomById(int id)
        {
            return _context.ReadById<Hotel_room>(id);
        }
        public Hotel CreateFromDTO(HotelDTO dto)
        {
            var hotel = _mapper.MapToEntity(dto);
            return Create(hotel);
        }
        public void UpdateFromDTO(int id, HotelDTO dto)
        {
            var hotel = _mapper.MapToEntity(dto);
            Update(id, hotel);
        }
        public int CalculateMaxSizeRoom(Hotel hotel)
        {
            return _mapper.CalculateMaxSizeRoom(hotel);
        }
        public int CalculateMinPrice(Hotel hotel)
        {
            return _mapper.CalculateMinPrice(hotel);
        }
    }
}