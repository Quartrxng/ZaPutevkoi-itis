using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MiniHttpServer;
using MiniTemplateEngine;


namespace MiniHttpServer.FrameWork.Core.HttpResponse
{
    public class PageResult : IHttpResult
    {
        private readonly string PathTemplate;
        private readonly object Data;
        public PageResult(string pathTemplate, object data) 
        {
            PathTemplate = pathTemplate;
            Data = data;
        }
        public string Execute(HttpListenerContext context)
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.StatusCode = 200;
            var templateRenderer = new HtmlTemplateRenderer();

            return templateRenderer.RenderFromFile(PathTemplate, Data);

            // TODO: доработать логику в EndpointHandler
            // TODO: вызов методов шаблонизатора
            // TODO: реализовать JsonResult
            // Создать проект с тестами для  MiniHttpServer.Framework.UnitTests
            // покрыть тестами класс HttpServer
        }
    }
}
