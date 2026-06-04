using MiniHttpServer;
using System.ComponentModel;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;
using MiniHttpServer.FrameWork;
using MyORMLibrary;




Settings settings = Settings.Instance;
string fs = null;

bool _keepRunning = true;
bool _keepAlive = false;

Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
{
    e.Cancel = true;
    _keepRunning = false;
};


HttpServer server = new HttpServer();
Task.Run(() =>
{
    server.Start(ref _keepAlive);
});
new ORMContext(Settings.Instance.Sql);
AdminSeeder.CreateDefaultAdmin(Settings.Instance.Sql);
while (_keepRunning) 
{
    var a = Console.ReadLine();
    if (a == "/start" && !_keepAlive)
    {
        server.Start(ref _keepAlive);
    }

    else if (a == "/start" && _keepAlive)
    {
        Console.WriteLine("Сервер уже запущен");
    }

    else if (a == "/restart")
    {
        server.Stop(ref _keepAlive);
        Thread.Sleep(500);
        server.Start(ref _keepAlive);
    }

    else if (a == "/stop")
    {
        server.Stop(ref _keepRunning);
        break;
    }

    else if (a == "/off" && _keepAlive)
    {
        server.Stop(ref _keepAlive);
    }


    else if (a == "/start" && !_keepAlive)
    {
        Console.WriteLine("Сервер уже запущен");
    }

    else if (a == "/off" && !_keepAlive)
    {
        Console.WriteLine("Сервер уже выключен");
    }

    else if (a == "/help")
    {
        Console.WriteLine("Доступные комманды: \n /start - запустить сервер \n /restart - перезапустить сервер \n /stop - остановить сервер \n /off - выключить сервер");
    }
}
