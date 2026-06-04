using DAO;
using MiniHttpServer.FrameWork;
using MiniHttpServer.FrameWork.Core;
using MiniHttpServer.FrameWork.Core.Attributes;
using MiniHttpServer.FrameWork.Core.HttpResponse;
using MiniHttpServer.FrameWork.Shared;
using MiniHttpServer.FrameWork.Validators;
using MiniHttpServer.Services;
using ModelsLibrary;
using MyORMLibrary;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Web;


namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    public class ApiEndpoint: EndpointBase
    {
        [HttpGet("placements/{query}")]
        public IHttpResult GetPlacements(string query)
        {
            Console.WriteLine(query);
            var decodedQuery = HttpUtility.UrlDecode(query);

            var searchService = new SearchService(Settings.Instance.Sql);
            var results = searchService.SearchAll(decodedQuery, 9);

            return Json(results);
        }
        [HttpGet("filters")]
        public IHttpResult GetFilters()
        {
            Console.WriteLine("Запрос пришел");
            var servicesDAO = new GenericDAO<Service>(Settings.Instance.Sql);
            var services = servicesDAO.Where(c => c.Is_Active).ToList();
            return Json(services ?? new List<Service>());
        }

        [HttpPost("searchHotel")]
        public IHttpResult GetHotelsCards(string name, string type="Hotel", int duration=1, int tourists = 3, int stars = 1, string meal = "Любой", string service = "0", string detail = "", int startValue = 0, string rating = "0.0")
        {
            var validator = new HotelSearchValidator();
            var hotelFilter = validator.ValidateAndBuildFilter(name, type, duration, tourists, stars, meal, service, detail, rating, startValue);

            var hotelService = new Services.HotelService(Settings.Instance.Sql);
            var hotels = hotelService.SearchHotels(hotelFilter);

            var model = new { Hotels = hotels};
            return Page("Templates/HotelCard.thtml", model);
        }

    }
}
