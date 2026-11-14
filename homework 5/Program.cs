using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using project;


class program
{
    private const string FolderName = "./pages";

    static async Task Main(string[] args)
    {
        var listener = new HttpListener();

        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("запуск сервера с хостом \"http://localhost:8080/\"");

        while (true)
        {
            try
            {
                FileInfo finfo = new FileInfo(FolderName);
                string Fullp = Path.GetFullPath(finfo.FullName);
                Console.WriteLine($"поиск файла: {finfo}, полный путь: {Fullp}");
                HttpListenerContext context = await listener.GetContextAsync();
                await ProcessRequestAsync(context, listener);
            }
            catch
            {
                Console.WriteLine($"ошибка при получении запроса");
            }
            
        }
    }
    private static readonly Controller controller = new Controller();
    private async static Task ProcessRequestAsync(HttpListenerContext context, HttpListener listener)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        string path = context.Request.Url.AbsolutePath;

        string filePath;
        bool steam = false;

        if (path == "/steam")
        {

            steam = true;
            string name = path.Trim('/');
            string directory = Path.Combine(FolderName, name);
            string[] files = Directory.GetFiles(
                directory,
                $"{name}.*"
            );

            if (context.Request.HttpMethod == "POST" &&
                context.Request.Url.AbsolutePath == "/login")
            {
                Controller controller = new Controller();

                await controller.ControlAs(context);

                return;
            }
        }

        if (path == "/")
        {
            filePath = Path.Combine(FolderName, "search.html");
            Console.WriteLine(path);
        }
        else
        {
            string name = path.Trim('/');
            string directory = Path.Combine(FolderName, name);
            string[] files = Directory.GetFiles(
                directory,
                $"{name}.*"
            );

            if (files.Length == 0)
            {
                await SendResponseAsync(
                    response,
                    $"<h1>Страница '{name}' не найдена!</h1>",
                    HttpStatusCode.NotFound
                );

                return;
            }
            filePath = files[0];
        }

        if (!File.Exists(filePath))
        {
            await SendResponseAsync(response, $"<h1>Страница '{path}.{Path.GetExtension(path)}' не найдена!</h1>", HttpStatusCode.NotFound);
            return;
        }
        string Content = await File.ReadAllTextAsync(filePath);



        await SendResponseAsync(response, filePath, HttpStatusCode.OK);
    }
    static async Task SendResponseAsync(HttpListenerResponse response, string filePath, HttpStatusCode statusCode)
    {
        string extension = Path.GetExtension(filePath);

        response.StatusCode = (int)statusCode;

        if (!ContentTypes.Types.TryGetValue(
            extension,
            out string contentType))
        {
            contentType = "application/octet-stream";
        }

        response.ContentType = contentType;

        byte[] buffer = await File.ReadAllBytesAsync(filePath);

        response.ContentLength64 = buffer.Length;

        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);

    response.OutputStream.Close();
    }
}
