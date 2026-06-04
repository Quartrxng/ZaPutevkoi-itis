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
    internal class LoginEndpoint : EndpointBase
    {
        private readonly EmailService _emailService = new EmailService();

        [HttpPost]
        public IHttpResult Login(string email)
        {
            // Отпарвка Email
            
            _emailService.SendEmail(email, "Авторизация прошла успешно");
            return Json(new { status = "Ok" });
        }
        [HttpGet]
        public void Login()
        {
            Console.WriteLine("Да");
        }
    }
}
