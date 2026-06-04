using DAO;
using DAO.Repositories;
using MiniHttpServer.FrameWork.Shared;
using ModelsLibrary;
using MyORMLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Services
{
    public class HotelService
    {
        private readonly HotelDAO _hotelDAO;
        private readonly ServiceDAO _serviceDAO;

        public HotelService(string connectionString)
        {
            IHotelMapper mapper = new HotelMapper();
            IHotelFilterBuilder filterBuilder = new HotelFilterBuilder();
            _hotelDAO = new HotelDAO(connectionString, mapper, filterBuilder);
            _serviceDAO = new ServiceDAO(connectionString);
        }

        public IEnumerable<Hotel> GetHotels(string query = null)
        {
            return _hotelDAO.GetHotelsWithRooms(query);
        }

        public IEnumerable<Hotel> SearchHotels(HotelFilter filter)
        {
            return _hotelDAO.GetHotelsByFilter(filter);
        }

        public Hotel FindHotel(int id)
        {
            return _hotelDAO.GetHotelWithRooms(id);
        }
        
        public IEnumerable<string> GetServices(int id)
        {
            return _serviceDAO.GetServiceNamesByHotelId(id);
        }
        public IEnumerable<int> GetIdsServices(int id)
        {
            return _serviceDAO.GetServiceIdsByHotelId(id);
        }
        public void CreateHotelServices(int hotel_id, List<int> service_ids)
        {
            var hotel_services = _serviceDAO.GetServiceIdsByHotelId(hotel_id)?.ToList() ?? new List<int>();
            foreach (int service_id in service_ids)
            {
                if (hotel_services.Count == 0 || hotel_services.Contains(service_id))
                    _serviceDAO.CreateServiceByHotelId(hotel_id, service_id);
            }
        }
        public void DeleteHotelServicesById(int hotel_id, List<int> service_ids)
        {
            var hotel_services = _serviceDAO.GetServiceIdsByHotelId(hotel_id)?.ToList() ?? new List<int>();
            foreach (int service_id in service_ids)
            {
                if (hotel_services.Count == 0 || hotel_services.Contains(service_id))
                    _serviceDAO.DeleteServiceForHotel(hotel_id, service_id);
            }
        }
        public void DeleteAllHotelServicesById(int hotel_id)
        {
                _serviceDAO.DeleteServicesByHotelId(hotel_id);
        }
        public void CreateHotel(Hotel hotel, List<PhotoFile> photoFiles, List<RoomDTO> roomsListDtos)
        {
            var id = _hotelDAO.CreateHotel(hotel);
            hotel.Id = id;
            string basePath = Directory.GetCurrentDirectory();
            string folderPath = Path.Combine(basePath, "public", "hotels", "data", hotel.Country, hotel.City, hotel.Id.ToString(), "Rooms");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            AddPhotos(photoFiles, hotel);
            _hotelDAO.UpdateHotel(hotel);
            var newRoomsLists = new List<Hotel_room>();
            foreach (var room in roomsListDtos)
            {
                var hotelRoom = new Hotel_room();
                hotelRoom.Name = room.Name;

                hotelRoom.Hotel_Id = hotel.Id;
                hotelRoom.Description = room.Description;
                hotelRoom.Price = room.Price;
                hotelRoom.PeopleCount = room.PeopleCount;
                if (room.PhotoFile == null)
                {
                    hotelRoom.Photo = room.Photo;
                }
                else
                {
                    AddPhotos(new List<PhotoFile>() { room.PhotoFile }, hotel, hotelRoom, true);
                }
                newRoomsLists.Add(hotelRoom);

            }
            _hotelDAO.CreateRooms(newRoomsLists, hotel.Id);
            DeleteAllHotelServicesById(hotel.Id);
            CreateHotelServices(hotel.Id, hotel.ServicesIds);
        }
        public void DeleteHotel(int id)
        {
            var hotel = _hotelDAO.DeleteHotel(id);
            _serviceDAO.DeleteServicesByHotelId(id);
            string basePath = Directory.GetCurrentDirectory();
            string folderPath = Path.Combine(basePath, "public", "hotels", "data", hotel.Country, hotel.City, hotel.Id.ToString());
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }
        public void UpdateHotel(Hotel hotel, List<PhotoFile> hotelPhotoFiles,List<string> deletedImagesList, List<int> deletedRoomsList, List<Hotel_room> newRoomsLists)
        {
            foreach (var roomId in deletedRoomsList)
            {
                var deletedRoomPhoto = _hotelDAO.GetHotel_RoomById(roomId).Photo;
                if (!string.IsNullOrEmpty(deletedRoomPhoto))
                {
                    deletedImagesList.Add(deletedRoomPhoto);
                }
            }

            AddPhotos(hotelPhotoFiles, hotel);

            DeletePhotos(deletedImagesList, hotel);

            _hotelDAO.UpdateHotel(hotel);

            _hotelDAO.CreateRooms(newRoomsLists, hotel.Id);

            _hotelDAO.UpdateRooms(hotel.Hotel_Rooms, deletedRoomsList);

            DeleteAllHotelServicesById(hotel.Id);
            CreateHotelServices(hotel.Id, hotel.ServicesIds);
        }
        public (List<Hotel_room> existingRooms, List<Hotel_room> newRooms) ProcessRooms(List<RoomDTO> roomsListDtos, Hotel hotel)
        {
            
            var roomList = new List<Hotel_room>();
            var newRoomsLists = new List<Hotel_room>();

            foreach (var room in roomsListDtos)
            {
                var hotelRoom = new Hotel_room();

                if (room.Id != null)
                {
                    // Существующая комната
                    hotelRoom.Id = room.Id.Value;
                    hotelRoom.Name = room.Name;
                    hotelRoom.Hotel_Id = hotel.Id;
                    hotelRoom.Description = room.Description;
                    hotelRoom.Price = room.Price;
                    hotelRoom.PeopleCount = room.PeopleCount;

                    if (room.PhotoFile == null)
                    {
                        hotelRoom.Photo = room.Photo;
                    }
                    else
                    {
                        AddPhotos(new List<PhotoFile>() { room.PhotoFile }, hotel, hotelRoom, true);
                    }
                    roomList.Add(hotelRoom);
                }
                else
                {
                    // Новая комната
                    hotelRoom.Name = room.Name;
                    hotelRoom.Hotel_Id = hotel.Id;
                    hotelRoom.Description = room.Description;
                    hotelRoom.Price = room.Price;
                    hotelRoom.PeopleCount = room.PeopleCount;

                    if (room.PhotoFile == null)
                    {
                        hotelRoom.Photo = room.Photo;
                    }
                    else
                    {
                        AddPhotos(new List<PhotoFile>() { room.PhotoFile }, hotel, hotelRoom, true);
                    }
                    newRoomsLists.Add(hotelRoom);
                }
            }

            return (roomList, newRoomsLists);
        }

        public void DeletePhotos(List<string> photo, Hotel hotel, Hotel_room room = null, bool isRoom = false)
        {
            if (isRoom)
            {
                foreach (string photoFile in photo)
                {
                    string basePath = Directory.GetCurrentDirectory();
                    string filePath = Path.Combine(basePath, "public", "hotels", "data", hotel.Country, hotel.City, hotel.Id.ToString(), "Rooms", photoFile);
                    var flag = PhotoDeleter.DeleteHotelImage(filePath);
                }
            }
            else
            {
                foreach (string photoFile in photo)
                {
                    string basePath = Directory.GetCurrentDirectory();
                    string filePath = Path.Combine(basePath, "public", "hotels", "data", hotel.Country, hotel.City, hotel.Id.ToString(), photoFile);
                    var flag = PhotoDeleter.DeleteHotelImage(filePath);
                    hotel.Image_Names.Remove(photoFile);
                }
            }
        }
        public void AddPhotos(List<PhotoFile> photo, Hotel hotel, Hotel_room room = null, bool isRoom = false)
        {
            if (isRoom)
            {
                foreach (PhotoFile photoFile in photo)
                {
                    string basePath = Directory.GetCurrentDirectory();
                    var fileName = Guid.NewGuid().ToString() + ".jpg";
                    string filePath = Path.Combine(basePath, "public", "hotels", "data", hotel.Country, hotel.City, hotel.Id.ToString(),"Rooms", fileName);
                    string folderPath = Path.Combine(basePath, "public", "hotels", "data", hotel.Country, hotel.City, hotel.Id.ToString(), "Rooms");
                    Console.WriteLine(folderPath);
                    Directory.CreateDirectory(folderPath);
                    var flag = DataUrlConverter.TryConvertDataUrlToJpg(photoFile.DataUrl, filePath);
                    if (flag)
                        room.Photo = fileName;
                }
            }
            else
            {
                foreach (PhotoFile photoFile in photo)
                {
                    var fileName = Guid.NewGuid().ToString() + ".jpg";
                    string basePath = Directory.GetCurrentDirectory();
                    string filePath = Path.Combine(basePath, "public", "hotels", "data", hotel.Country, hotel.City, hotel.Id.ToString() ,fileName);
                    string folderPath = Path.Combine(basePath, "public", "hotels", "data", hotel.Country, hotel.City, hotel.Id.ToString());
                    Console.WriteLine(folderPath);
                    Directory.CreateDirectory(folderPath);
                    var flag = DataUrlConverter.TryConvertDataUrlToJpg(photoFile.DataUrl, filePath);
                    if (flag)
                        hotel.Image_Names.Add(fileName);
                }
            }
        }
    }
}
