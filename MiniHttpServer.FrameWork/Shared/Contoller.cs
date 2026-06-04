using System.Net;

namespace MiniHttpServer.FrameWork.Shared
{
    public class GetResponse
    {
        public static byte[] GetBytes(string path)
        {
                byte[] answer = File.ReadAllBytes(path);
                return answer;
        }
    }

    public class GetPath
    {
        public static string Path(HttpListenerRequest? request, string publicDirectoryPath)
        {
            var decodedPath = Uri.UnescapeDataString(request.Url.AbsolutePath);
            var path = decodedPath[^1] == '/' ? decodedPath + "index.html" : decodedPath;
            return publicDirectoryPath + path;
        }
    }
}
