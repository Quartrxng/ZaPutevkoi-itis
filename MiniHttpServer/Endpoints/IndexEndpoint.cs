using MiniHttpServer.FrameWork.Core;
using MiniHttpServer.FrameWork.Core.Attributes;
using MiniHttpServer.FrameWork.Core.HttpResponse;
using MiniHttpServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class IndexEndpoint : EndpointBase
    {
        [HttpGet]
        public IHttpResult GetSearchPage()
        {
            return Page("public/Search.html", new { });
        }
    }
}
