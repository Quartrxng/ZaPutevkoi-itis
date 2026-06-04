using DAO.Interfaces;
using ModelsLibrary;
using MyORMLibrary;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO
{
    public class ServiceDAO : IServiceDAO
    {
        private ORMContext context;
        public ServiceDAO(string connectionString)
        {
            context = new ORMContext(connectionString);
        }

        public IEnumerable<string> GetServiceNamesByHotelId(int hotelId)
        {
            var hotelServices = context.Where<Hotel_Service>(hs => hs.Hotel_Id == hotelId);
            var serviceIds = hotelServices.Select(hs => hs.Service_Id).ToList();
            var serviceNames = new List<string>();
            foreach (var serviceId in serviceIds)
            {
                var service = context.ReadById<Service>(serviceId);
                if (service != null && service.Is_Active)
                {
                    serviceNames.Add(service.Service_Name);
                }
            }

            return serviceNames;
        }
        public IEnumerable<int> GetServiceIdsByHotelId(int hotelId)
        {
            var hotelServices = context.Where<Hotel_Service>(hs => hs.Hotel_Id == hotelId);
            var serviceIds = hotelServices.Select(hs => hs.Service_Id).ToList();

            return serviceIds;
        }

        public void CreateServiceByHotelId(int hotelId, int id)
        {
            string sql = @"
                INSERT INTO ""Hotel_Services"" (""hotel_id"", ""service_id"")
                VALUES (@HotelId, @ServiceId)";

                    var parameters = new[]
                    {
                new NpgsqlParameter("@HotelId", hotelId),
                new NpgsqlParameter("@ServiceId", id)
            };

            context.ExecuteNonQuery(sql, parameters);
        }
        public void DeleteServiceForHotel(int hotelId, int id)
        {
                    string sql = @"
                DELETE FROM ""Hotel_Services"" 
                WHERE ""hotel_id"" = @HotelId AND ""service_id"" id";

                    var parameters = new[]
                    {
                new NpgsqlParameter("@HotelId", hotelId),
                new NpgsqlParameter("@ServiceIds", id) 
            };

            context.ExecuteNonQuery(sql, parameters);
        }
        public void DeleteServicesByHotelId(int hotelId)
        {
            string sql = @"
                DELETE FROM ""Hotel_Services"" 
                WHERE ""hotel_id"" = @HotelId";

            var parameters = new[]
            {
                new NpgsqlParameter("@HotelId", hotelId),
            };

            context.ExecuteNonQuery(sql, parameters);
        }
    }
}
