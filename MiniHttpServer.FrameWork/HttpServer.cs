using MiniHttpServer.FrameWork.Core.Abstracts;
using MiniHttpServer.FrameWork.Core.Handlers;
using MiniHttpServer.FrameWork.Shared;
using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace MiniHttpServer.FrameWork
{
    public class HttpServer
    {
        public static Settings Settings;

        private HttpListener _listener = new ();

        private CancellationTokenSource _cts;

        public HttpServer()
        {
            Settings = Settings.Instance;
        }

        public void Start(ref bool keepAlive)
        {
            _cts = new CancellationTokenSource();
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://" + Settings.Domain + ":" + Settings.Port + "/");
            keepAlive = true;
            _listener.Start();
            Console.WriteLine($"http://{Settings.Domain}:{Settings.Port}/index.html - чтобы зайти на index.html");
            Console.WriteLine($"http://{Settings.Domain}:{Settings.Port}/Loginform/index.html - чтобы зайти на LoginForm");
            Console.WriteLine($"http://{Settings.Domain}:{Settings.Port}/Delta/index.html - чтобы зайти на Delta");
            Console.WriteLine("Сервер запущен");

            _ = ListenAsync(_cts.Token);
        }

        public void Stop(ref bool keepRunning)
        {
            _cts?.Cancel();
            _listener?.Stop();
            _listener?.Close();
            Console.WriteLine("Сервер остановлен");
            keepRunning = false;
        }

        private async Task ListenAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync();
                _ = ListenerCallback(context, token);
            }
        }

        private async Task ListenerCallback(HttpListenerContext context, CancellationToken token)
        {
            try
            {
                Handler staticFilesHandler = new StaticFilesHandler();
                Handler endPointsHandler = new EndpointsHandler();
                staticFilesHandler.Successor = endPointsHandler;

                staticFilesHandler.HandleRequest(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке запроса: {ex.Message}");
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
        }
    }
}