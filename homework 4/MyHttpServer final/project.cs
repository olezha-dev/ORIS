using System.Text.Json;
using MyHttpServer;

string settingsJson = File.ReadAllText("settings.json");

Settings settings = JsonSerializer.Deserialize<Settings>(settingsJson) 
    ?? throw new Exception("Не удалось прочитать settings.json");

HttpServer server = new HttpServer(
    settings.Server.Host,
    settings.Server.Port,
    settings.Server.Path,
    settings.Pages
);

Task serverTask = server.Start();

while (true)
{
    string? command = Console.ReadLine();

    if (command == "stop")
    {
        server.Stop();
        break;
    }
}