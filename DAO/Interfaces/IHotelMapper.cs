using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO.Repositories
{
    public interface IHotelMapper
    {
        // Из Hotel в HotelDTO
        HotelDTO MapToDTO(Hotel hotel);

        // Из HotelDTO в Hotel (базовое преобразование)
        Hotel MapToEntity(HotelDTO dto);

        // Из HotelDTO + комнаты в полный Hotel
        Hotel MapToFullHotel(HotelDTO dto, List<Hotel_room> rooms);

        // Вычисляемые поля
        int CalculateMaxSizeRoom(Hotel hotel);
        int CalculateMinPrice(Hotel hotel);
    }
}
