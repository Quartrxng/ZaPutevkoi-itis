using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace MiniHttpServer.FrameWork.Shared
{
    public class ModelBinder
    {
        public static Stream Body { get; set; }
        public virtual bool CanBind(HttpListenerContext context, ParameterInfo parameter)
        {
            return context.Request.ContentLength64 > 0 &&
                   context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true;
        }

        public virtual object? Bind(HttpListenerContext context, Type type)
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                var body = reader.ReadToEnd();

                if (string.IsNullOrWhiteSpace(body))
                    return GetDefault(type);

                var result = JsonSerializer.Deserialize(body, type, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? GetDefault(type);
            }
            catch
            {
                return GetDefault(type);
            }
        }

        private static object? GetDefault(Type type)
            => type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}