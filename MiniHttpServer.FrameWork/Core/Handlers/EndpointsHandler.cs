using MiniHttpServer.FrameWork.Core.Abstracts;
using MiniHttpServer.FrameWork.Core.Attributes;
using MiniHttpServer.FrameWork.Core.HttpResponse;
using MiniHttpServer.FrameWork.Shared;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
namespace MiniHttpServer.FrameWork.Core.Handlers
{
    class EndpointsHandler : Handler
    {
        public override async void HandleRequest(HttpListenerContext context)
        {
            try
            {
                if (true)
                {
                    var request = context.Request;

                    var pathSegments = request.Url.AbsolutePath
                                                .Split('/', StringSplitOptions.RemoveEmptyEntries);

                    var assembly = Assembly.GetEntryAssembly();
                    Type? endpoint = null;
                    int endpointIndex = -1;

                    if (pathSegments.Length == 0)
                    {
                        endpoint = assembly.GetTypes()
                       .FirstOrDefault(t => t.GetCustomAttribute<EndpointAttribute>() != null
                                            && t.Name.Equals("IndexEndpoint", StringComparison.OrdinalIgnoreCase));
                        endpointIndex = -1;
                    }
                    else
                    {
                        for (int i = 0; i < pathSegments.Length; i++)
                        {
                            string candidate = pathSegments[i];
                            var type = assembly.GetTypes()
                                .Where(t => t.GetCustomAttribute<EndpointAttribute>() != null)
                                .FirstOrDefault(t => IsCheckedNameEndpoint(t.Name, candidate));

                            if (type != null)
                            {
                                endpoint = type;
                                endpointIndex = i;
                                break;
                            }
                        }
                    }

                    if (endpoint == null) return;

                    string endpointPath = "/" + string.Join('/', pathSegments.Skip(endpointIndex + 1));
                    if (endpoint == null) return;
                    Dictionary<string, string> routeValues = new();
                    var method = endpoint.GetMethods()
                        .Where(m => m.GetCustomAttributes(true)
                            .Any(attr => attr.GetType().Name.StartsWith($"Http{context.Request.HttpMethod}", StringComparison.OrdinalIgnoreCase)))
                        .FirstOrDefault(m => IsMethodForCurrentPath(m, endpointPath, out routeValues));

                    if (method == null)
                    {
                        method = endpoint.GetMethods().Where(t => t.GetCustomAttributes(true)
                            .Any(attr => attr.GetType().Name.Equals($"Http{context.Request.HttpMethod}", StringComparison.OrdinalIgnoreCase)))
                            .FirstOrDefault();
                    }

                    if (method == null) return;

                    var parameters = new object[method.GetParameters().Length];

                    if (parameters.Length > 0)
                    {
                        string body = "";
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8))
                        {
                            body = reader.ReadToEnd();
                        }

                        var bodyParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        bool isJson = request.ContentType?.Contains("application/json") == true;
                        bool isForm = request.ContentType?.Contains("application/x-www-form-urlencoded") == true;

                        if (isJson && !string.IsNullOrWhiteSpace(body))
                        {
                            try
                            {
                                using var doc = JsonDocument.Parse(body);
                                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                                {
                                    foreach (var prop in doc.RootElement.EnumerateObject())
                                    {
                                        bodyParams[prop.Name] = prop.Value.ToString();
                                    }
                                }
                            }
                            catch { }
                        }

                        else if (isForm && !string.IsNullOrWhiteSpace(body))
                        {
                            foreach (var pair in body.Split('&'))
                            {
                                var kv = pair.Split('=', 2);
                                if (kv.Length == 2)
                                {
                                    bodyParams[WebUtility.UrlDecode(kv[0])] = WebUtility.UrlDecode(kv[1]);
                                }
                            }
                        }

                        var methodParams = method.GetParameters();
                        for (int i = 0; i < methodParams.Length; i++)
                        {
                            var param = methodParams[i];
                            object? value = null;

                            if (routeValues.TryGetValue(param.Name, out string routeVal))
                            {
                                value = Converter.ConvertValue(param.ParameterType, routeVal);
                            }
                            else if (bodyParams.TryGetValue(param.Name, out string bodyVal))
                            {
                                if (param.ParameterType == typeof(string))
                                {
                                    value = bodyVal;
                                }
                                else
                                {
                                    try
                                    {
                                        value = JsonSerializer.Deserialize(bodyVal, param.ParameterType);
                                    }
                                    catch
                                    {
                                        value = Converter.ConvertValue(param.ParameterType, bodyVal);
                                    }
                                }
                            }

                            else
                            {
                                value = param.ParameterType.IsValueType
                                    ? Activator.CreateInstance(param.ParameterType)
                                    : null;
                            }

                            parameters[i] = value;
                        }
                    }

                    bool isBaseEndpoint = endpoint.Assembly.GetTypes()
                                           .Any(t => typeof(EndpointBase)
                                           .IsAssignableFrom(t) && !t.IsAbstract);

                    var instanceEndpoint = Activator.CreateInstance(endpoint);

                    if (isBaseEndpoint)
                    {
                        (instanceEndpoint as EndpointBase).SetContext(context);
                    }

                    var ret = method.Invoke(instanceEndpoint, parameters);
                    if (ret is CustomHttpResponse httpResponse)
                    {
                        httpResponse.Execute(context);
                        return;
                    }
                    else if (ret is IHttpResult httpResult)
                    {
                        if (context.Response.OutputStream == null)
                        {
                            return;
                        }

                        string resultContent = httpResult.Execute(context);

                        if (context.Response.OutputStream == null)
                        {
                            return;
                        }

                        await WriteResponseAsync(context.Response, resultContent);
                    }

                    else if (ret is string stringResult)
                    {
                        await WriteResponseAsync(context.Response, stringResult);
                    }
                    else if (ret != null)
                    {
                        await WriteResponseAsync(context.Response, ret.ToString());
                    }
                    else
                    {
                        context.Response.StatusCode = 200;
                        await WriteResponseAsync(context.Response, "OK");
                    }
                }
                else if (Successor != null)
                {
                    Successor.HandleRequest(context);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    context.Response.StatusCode = 500;
                    await WriteResponseAsync(context.Response, $"Internal Server Error\n{ex.Message}");
                }
                catch {}
            }
        }
        private bool IsCheckedNameEndpoint(string endpointName, string className)=>
            endpointName.Equals(className, StringComparison.OrdinalIgnoreCase) 
            || endpointName.Equals($"{className}EndPoint", StringComparison.OrdinalIgnoreCase);

        private bool IsMethodForCurrentPath(MethodInfo method, string requestPath, out Dictionary<string, string> routeValues)
        {
            routeValues = new Dictionary<string, string>();

            var attr = method.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name.StartsWith("Http"));
            string route = attr?.GetType().GetProperty("Route")?.GetValue(attr) as string
                           ?? method.Name.ToLower().Replace("get", "").Replace("post", "");

            var template = route.Trim('/').Split('/');
            var path = requestPath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (template.Length != path.Length)
                return false;

            for (int i = 0; i < template.Length; i++)
            {
                if (template[i].StartsWith("{") && template[i].EndsWith("}"))
                {
                    routeValues[template[i].Trim('{', '}')] = path[i];
                }
                else if (!template[i].Equals(path[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static async Task WriteResponseAsync(HttpListenerResponse response, string content) // object
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            // получаем поток ответа и пишем в него ответ
            response.ContentLength64 = buffer.Length;
            using Stream output = response.OutputStream;
            // отправляем данные
            await output.WriteAsync(buffer);
            await output.FlushAsync();
        }
    }
}
