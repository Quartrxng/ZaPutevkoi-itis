using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniHttpServer.FrameWork.Core.HttpResponse
{
    public class JsonResult : IHttpResult
    {
        private readonly object _data;

        public JsonResult(object data)
        {
            _data = data;
        }

        public string Execute(HttpListenerContext context)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = 200;

            var options = new JsonSerializerOptions
            {
                WriteIndented = false, // В строку
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // позволяет кириллицу
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(_data, options);
        }
    }
}
