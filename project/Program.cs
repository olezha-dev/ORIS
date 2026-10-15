using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;


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
                await ProcessRequestAsync(context);
            }
            catch
            {
                Console.WriteLine($"ошибка при получении запроса");

            }
            
        }
    }

    private async static Task ProcessRequestAsync(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        string pageName = request.QueryString["./pages"];
        if (string.IsNullOrWhiteSpace(pageName))
        {
            pageName = "search";
        }

        string filePath = Path.Combine(FolderName, $"{pageName}.html");

        string path = request.RawUrl;

        if ((string.IsNullOrEmpty(path) || path == "/") ==false )
        {
            pageName = path;
            filePath = Path.Combine(FolderName, path.Substring(1), $"{path.Substring(1)}.html");
            FileInfo finfo = new FileInfo(path);
            string Fullp = Path.GetFullPath(finfo.FullName);
            Console.WriteLine($"поиск файла: {finfo}, полный путь: {Fullp}");
        }

        if (!File.Exists(filePath))
        {
            await SendResponseAsync(response, $"<h1>Страница '{pageName}.html' не найдена!</h1>", HttpStatusCode.NotFound);
            return;
        }
        string htmlContent = await File.ReadAllTextAsync(filePath);


        await SendResponseAsync(response, htmlContent, HttpStatusCode.OK);
    }
    static async Task SendResponseAsync(HttpListenerResponse response, string content, HttpStatusCode statusCode)
    {
        response.StatusCode = (int)statusCode;

    byte[] buffer = Encoding.UTF8.GetBytes(content);

        response.ContentType = "text/css";
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;

    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);

    response.OutputStream.Close();
    }
}
