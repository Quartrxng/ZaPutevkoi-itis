using DAO;
using DAO.Interfaces;
using DAO.Repositories;
using ModelsLibrary;
using MyORMLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Services
{
    public class SearchService
    {
        private readonly GenericDAO<Land> _landDAO;
        private readonly GenericDAO<Resort> _resortDAO;
        private readonly IHotelDAO _hotelDAO;

        public SearchService(string connectionString)
        {
            _landDAO = new GenericDAO<Land>(connectionString);
            _resortDAO = new GenericDAO<Resort>(connectionString);

            IHotelMapper mapper = new HotelMapper();
            IHotelFilterBuilder filterBuilder = new HotelFilterBuilder();
            _hotelDAO = new HotelDAO(connectionString, mapper, filterBuilder);
        }

        public List<Placements> SearchAll(string query, int limit = 9)
        {
            var lands = _landDAO.Where(l => l.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase), limit)
                .Select(l => new Placements { Name = l.Name, Type = l.Type, Image = l.Image })
                .ToList();

            var resorts = _resortDAO.Where(r => r.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase), limit)
                .Select(r => new Placements { Name = r.Name, Type = r.Type, Country = r.Country })
                .ToList();

            var hotels = _hotelDAO.GetHotelsStartingWith(query, limit)
                .Select(h => new Placements { Name = $"{h.Name} {h.Stars}*", Country = $"{h.City}, {h.Country}", Type = "hotel" })
                .ToList();

            var allResults = new List<Placements>();
            allResults.AddRange(lands);
            allResults.AddRange(resorts);
            allResults.AddRange(hotels);

            return allResults.Take(limit).ToList();
        }

        public List<Placements> SearchLands(string query, int limit = 9)
        {
            return _landDAO.Where(l => l.Name.Contains(query, StringComparison.OrdinalIgnoreCase), limit)
                .Select(l => new Placements { Name = l.Name, Type = l.Type, Image = l.Image })
                .ToList();
        }

        public List<Placements> SearchResorts(string query, int limit = 9)
        {
            return _resortDAO.Where(r => r.Name.Contains(query, StringComparison.OrdinalIgnoreCase), limit)
                .Select(r => new Placements { Name = r.Name, Type = r.Type, Country = r.Country })
                .ToList();
        }

        public List<Placements> SearchHotels(string query, int limit = 9)
        {
            return _hotelDAO.GetHotelsStartingWith(query, limit)
                .Select(h => new Placements { Name = $"{h.Name} {h.Stars}*", Country = $"{h.City}, {h.Country}", Type = "hotel" })
                .ToList();
        }
    }
}
