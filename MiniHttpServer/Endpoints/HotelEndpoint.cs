using MiniHttpServer.FrameWork;
using MiniHttpServer.FrameWork.Core;
using MiniHttpServer.FrameWork.Core.Attributes;
using MiniHttpServer.FrameWork.Core.HttpResponse;
using MiniHttpServer.Services;
using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    public class HotelEndpoint : EndpointBase
    {
        [HttpGet("/{id}")]
        public IHttpResult GetPlacements(int id)
        {
            Console.WriteLine(id);

            var service = new Services.HotelService(Settings.Instance.Sql);
            var hotel = service.FindHotel(id);
            hotel.Service_Names = service.GetServices(id).ToList();
            var hotels = new List<Hotel> { hotel };

            var dataModel = new { Hotels = hotels };
            return Page("Templates/Hotel.thtml", dataModel);
        }
    }
}
