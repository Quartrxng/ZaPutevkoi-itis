using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO.Interfaces
{
    public interface IServiceDAO
    {
        IEnumerable<string> GetServiceNamesByHotelId(int hotelId);
        IEnumerable<int> GetServiceIdsByHotelId(int hotelId);
    }
}
