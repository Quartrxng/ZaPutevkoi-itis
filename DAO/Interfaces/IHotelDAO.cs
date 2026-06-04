using DAO.Interfaces;
using ModelsLibrary;

public interface IHotelDAO : IGenericDAO<Hotel>
{
    Hotel GetHotelWithRooms(int hotelId);
    IEnumerable<Hotel> GetHotelsStartingWith(string query, int limit = 0);
    IEnumerable<Hotel> GetFullHotelsStartingWith(string query, int limit = 0);
    IEnumerable<Hotel> GetHotelsByFilter(HotelFilter filter);
    List<Hotel> GetHotelsWithRooms(string query = null);
    Hotel GetFullHotel(int hotelId);

    Hotel CreateFromDTO(HotelDTO dto);
    void UpdateFromDTO(int id, HotelDTO dto);

    int CalculateMaxSizeRoom(Hotel hotel);
    int CalculateMinPrice(Hotel hotel);
}