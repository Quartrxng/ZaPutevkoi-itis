using System;
using System.Text;
using System.Text.Json;
using MiniHttpServer.FrameWork;
using MiniHttpServer.FrameWork.Core;
using MiniHttpServer.FrameWork.Core.Attributes;
using MiniHttpServer.FrameWork.Core.HttpResponse;
using MiniHttpServer.FrameWork.Shared;
using MiniHttpServer.Services;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    public class AdminEndpoint : EndpointBase
    {
        private readonly AdminAuthService _authService;

        public AdminEndpoint()
        {
            _authService = new AdminAuthService(Settings.Instance.Sql);
        }

        [HttpGet("/login")]
        public IHttpResult GetLoginPage()
        {
            Console.WriteLine("Запрос получение логин page");
            return Page("public/admin/Login.html", new object { });
        }

        [HttpGet("/admin")]
        public IHttpResult GetAdminPage()
        {
            try
            {
                // Логируем весь заголовок Cookie
                string cookieHeader = Context.Request.Headers.Get("Cookie");
                Console.WriteLine($"[GetAdminPage] Full Cookie header: '{cookieHeader}'");

                string tokenCookie = Cookie.GetCookie("admin_session", Context.Request);
                Console.WriteLine($"[GetAdminPage] Extracted token cookie: '{tokenCookie}'");

                if (string.IsNullOrEmpty(tokenCookie))
                {
                    Console.WriteLine("[GetAdminPage] No cookie found -> Redirect to Login");
                    return RedirectToLogin();
                }

                byte[] token;
                try
                {
                    token = Convert.FromBase64String(tokenCookie);
                    Console.WriteLine($"[GetAdminPage] Decoded token length: {token.Length} bytes");

                    // Логируем первые несколько байт токена для отладки
                    if (token.Length > 0)
                    {
                        string tokenHex = BitConverter.ToString(token).Replace("-", "");
                        Console.WriteLine($"[GetAdminPage] Token hex (first 16): {tokenHex.Substring(0, Math.Min(16, tokenHex.Length))}...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GetAdminPage] Failed to decode token: {ex.Message}");
                    return RedirectToLogin();
                }

                bool isValid = false;
                try
                {
                    isValid = _authService.IsTokenValid(token);
                    Console.WriteLine($"[GetAdminPage] Token validation completed: {isValid}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GetAdminPage] Error during token validation: {ex.Message}");
                    Console.WriteLine($"[GetAdminPage] Stack trace: {ex.StackTrace}");
                    return RedirectToLogin();
                }

                if (!isValid)
                {
                    Console.WriteLine("[GetAdminPage] Token not valid -> Redirect to Login");
                    return RedirectToLogin();
                }

                Console.WriteLine("[GetAdminPage] Token valid -> Admin page");
                return Page("public/admin/AdminMainPage.html", new object { });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAdminPage] General error: {ex.Message}");
                Console.WriteLine($"[GetAdminPage] Stack trace: {ex.StackTrace}");
                return RedirectToLogin();
            }
        }

        [HttpPost("login")]
        public IHttpResult Login(string username, string password)
        {
            try
            {
                var (success, message, token) = _authService.Login(username, password);

                if (!success)
                {
                    Console.WriteLine($"[Login] Failed: {message}");
                    var errorResponse = new { success = false, message = message };
                    var json = JsonSerializer.Serialize(errorResponse);
                    return new CustomHttpResponse(401, Encoding.UTF8.GetBytes(json), "application/json");
                }

                var tokenBase64 = Convert.ToBase64String(token);
                var cookie = $"admin_session={tokenBase64}; HttpOnly; SameSite=Strict; Path=/";
                Console.WriteLine($"[Login] Setting cookie: {cookie}");

                var response = new { success = true, message = message };

                var jsonResult = JsonSerializer.Serialize(response);
                var httpResponse = new CustomHttpResponse(200, Encoding.UTF8.GetBytes(jsonResult), "application/json");
                httpResponse.Headers.Add("Set-Cookie", cookie);
                return httpResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                var errorResponse = new { success = false, message = "Login failed" };
                var json = JsonSerializer.Serialize(errorResponse);
                return new CustomHttpResponse(500, Encoding.UTF8.GetBytes(json), "application/json");
            }
        }

        [HttpGet("check-auth")]
        public IHttpResult CheckAuth()
        {
            try
            {
                string tokenCookie = Cookie.GetCookie("admin_session", Context.Request);
                if (string.IsNullOrEmpty(tokenCookie))
                {
                    var response = new { authenticated = false };
                    var json = JsonSerializer.Serialize(response);
                    return new CustomHttpResponse(401, Encoding.UTF8.GetBytes(json), "application/json");
                }

                byte[] token;
                try
                {
                    token = Convert.FromBase64String(tokenCookie);
                }
                catch
                {
                    var response = new { authenticated = false };
                    var json = JsonSerializer.Serialize(response);
                    return new CustomHttpResponse(401, Encoding.UTF8.GetBytes(json), "application/json");
                }

                bool isValid = _authService.IsTokenValid(token);
                if (!isValid)
                {
                    var response = new { authenticated = false };
                    var json = JsonSerializer.Serialize(response);
                    return new CustomHttpResponse(401, Encoding.UTF8.GetBytes(json), "application/json");
                }

                var successResponse = new { authenticated = true };
                var jsonResult = JsonSerializer.Serialize(successResponse);
                return new CustomHttpResponse(200, Encoding.UTF8.GetBytes(jsonResult), "application/json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Check auth error: {ex.Message}");
                var errorResponse = new { authenticated = false };
                var json = JsonSerializer.Serialize(errorResponse);
                return new CustomHttpResponse(500, Encoding.UTF8.GetBytes(json), "application/json");
            }
        }

        [HttpPost("/logout")]
        public IHttpResult Logout()
        {
            try
            {
                string tokenCookie = Cookie.GetCookie("admin_session", Context.Request);
                if (!string.IsNullOrEmpty(tokenCookie))
                {
                    byte[] token = null;
                    try
                    {
                        token = Convert.FromBase64String(tokenCookie);
                        _authService.Logout(token);
                    }
                    catch
                    {
                    }
                }

                var deleteCookie = "admin_session=; HttpOnly; Secure; SameSite=Strict; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT";

                var response = new { success = true, message = "Logged out successfully" };

                var jsonResult = JsonSerializer.Serialize(response);
                var httpResponse = new CustomHttpResponse(200, Encoding.UTF8.GetBytes(jsonResult), "application/json");
                httpResponse.Headers.Add("Set-Cookie", deleteCookie);
                return httpResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout error: {ex.Message}");
                var errorResponse = new { success = false, message = "Logout failed" };
                var json = JsonSerializer.Serialize(errorResponse);
                return new CustomHttpResponse(500, Encoding.UTF8.GetBytes(json), "application/json");
            }
        }

        private IHttpResult RedirectToLogin()
        {
            return GetLoginPage();
        }
    }
}