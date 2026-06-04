using MiniHttpServer.FrameWork.Core;
using System.Net;

namespace MiniHttpServer.FrameWork.Core.HttpResponse
{
    public class CustomHttpResponse : IHttpResult
    {
        public int StatusCode { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();

        public CustomHttpResponse(int statusCode, byte[] content, string contentType = "text/html")
        {
            StatusCode = statusCode;
            Content = content;
            ContentType = contentType;
        }

        public string Execute(HttpListenerContext context)
        {
            context.Response.StatusCode = StatusCode;
            context.Response.ContentType = ContentType;

            foreach (var header in Headers)
            {
                context.Response.Headers.Add(header.Key, header.Value);
            }

            context.Response.ContentLength64 = Content.Length;
            context.Response.OutputStream.Write(Content, 0, Content.Length);
            context.Response.Close(); // <-- Закрывает поток

            return string.Empty;
        }
    }
}