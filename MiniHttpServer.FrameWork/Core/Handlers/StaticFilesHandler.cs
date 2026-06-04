using MiniHttpServer.FrameWork.Core.Abstracts;
using MiniHttpServer.FrameWork.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.FrameWork.Core.Handlers
{
    class StaticFilesHandler : Handler
    {
        public async override void HandleRequest(HttpListenerContext context)
        {
            // некоторая обработка запроса
            var request = context.Request;
            var isGetMethod = request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase);
            var isStaticFile = request.Url.AbsolutePath.Split('/').Any(x => x.Contains("."));
            if (isGetMethod && isStaticFile)
            {

                var response = context.Response;


                string path = GetPath.Path(request, HttpServer.Settings.PublicDirectoryPath);

                byte[] buffer = null;
                try
                {
                    buffer = GetResponse.GetBytes(path);
                    response.ContentType = Shared.ContentType.GetContentType(path);
                    response.ContentLength64 = buffer.Length;

                    using Stream output = response.OutputStream;
                    // отправляем данные
                    await output.WriteAsync(buffer);
                    await output.FlushAsync();
                    response.Close();

                    Console.WriteLine($"Запрос обработан {path}");
                }

                catch (FileNotFoundException)
                {
                    Console.WriteLine($"Файл не найден {path}");
                    response.StatusCode = 404;
                    response.ContentLength64 = 0;
                    using Stream output = response.OutputStream;
                    await output.FlushAsync();
                }

                catch (DirectoryNotFoundException ex)
                {
                    Console.WriteLine($"Директория не найдена: {ex.Message}");
                    response.StatusCode = 404;
                    response.ContentLength64 = 0;
                    using Stream output = response.OutputStream;
                    await output.FlushAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    response.StatusCode = 500;
                    response.ContentLength64 = 0;
                    using Stream output = response.OutputStream;
                    await output.FlushAsync();
                }
                return;
            }
            // передача запроса дальше по цепи при наличии в ней обработчиков
            else if (Successor != null)
            {
                Successor.HandleRequest(context);
            }
        }
    }
}
