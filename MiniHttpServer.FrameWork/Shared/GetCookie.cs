using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.FrameWork.Shared
{
    public static class Cookie
    {
        public static string GetCookie(string cookieName, HttpListenerRequest request)
        {
            string cookieHeader = request.Headers.Get("Cookie");
            if (string.IsNullOrEmpty(cookieHeader))
            {
                return null;
            }

            var cookies = cookieHeader.Split(';');
            foreach (var cookie in cookies)
            {
                int separatorIndex = cookie.IndexOf('=');
                if (separatorIndex > 0)
                {
                    string name = cookie.Substring(0, separatorIndex).Trim();
                    string value = cookie.Substring(separatorIndex + 1).Trim();

                    if (name == cookieName)
                    {
                        return value;
                    }
                }
            }

            return null;
        }
    }
}
