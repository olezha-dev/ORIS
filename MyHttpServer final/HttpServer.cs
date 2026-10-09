using System.Net;

namespace MyHttpServer
{
    public class HttpServer
    {
        private HttpListener server;
        private string urlPrefix;
        private string serverPath;
        private string rootPath;
        private string staticPath;
        private Dictionary<string, string> pages;


        public HttpServer(string host, string port, string path, Dictionary<string, string> pages)
        {
            server = new HttpListener();
            this.pages = pages;
            serverPath = path.Trim('/');
            urlPrefix = $"http://{host}:{port}/{serverPath}/";
            server.Prefixes.Add(urlPrefix);
            rootPath = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            staticPath = Path.Combine(rootPath, "Static");
            Console.WriteLine("Корневая папка: " + rootPath);
        }



        public Task Start()
        {
            server.Start();

            Console.WriteLine("IsListening: " + server.IsListening);

            foreach (string prefix in server.Prefixes)
            {
                Console.WriteLine("Prefix: " + prefix);
            }

            return StartAsync();
        }


        public void Stop()
        {
            server.Stop();

            Console.WriteLine("Сервер остановлен");
        }

        private async Task StartAsync()
        {
            try
            {
                while (true)
                {
                    var context = await server.GetContextAsync();
                    Console.WriteLine("Запрос получен");

                    var request = context.Request;
                    var response = context.Response;

                    string filePath = request.Url?.AbsolutePath.TrimStart('/') ?? "";

                    if (filePath.StartsWith(serverPath + "/"))
                    {
                        filePath = filePath.Substring(serverPath.Length + 1);
                    }

                    string[] parts = filePath.Split('/', 2);

                    if (pages.ContainsKey(filePath))
                    {
                        filePath = pages[filePath];
                    }
                    else if (parts.Length == 2 && pages.ContainsKey(parts[0]))
                    {
                        string page = pages[parts[0]];

                        string pageFolder = Path.GetDirectoryName(page) ?? "";

                        filePath = Path.Combine(pageFolder, parts[1]);
                    }

                    string fullPath = Path.Combine(staticPath, filePath);

                    Console.WriteLine("Full" + fullPath,
                        $"Получен запрос: {request.HttpMethod} {request.Url?.AbsolutePath}"
                    );

                    if (string.IsNullOrEmpty(filePath))
                    {
                        filePath = "404.html";
                        fullPath = Path.Combine(rootPath, filePath);
                        response.StatusCode = 404;
                    }

                    if (!File.Exists(fullPath))
                    {
                        Console.WriteLine(
                            $"Файл не найден"
                        );
                    }
                    else
                    {
                        response.StatusCode = 200;
                    }

                    string extension =
                        Path.GetExtension(filePath).ToLower();

                    if (ContentTypes.Types.ContainsKey(extension))
                    {
                        response.ContentType =
                            ContentTypes.Types[extension];
                    }
                    else
                    {
                        response.ContentType =
                            "application/octet-stream";
                    }

                    byte[] buffer =
                        await File.ReadAllBytesAsync(fullPath);

                    response.ContentLength64 = buffer.Length;

                    using Stream output = response.OutputStream;

                    await output.WriteAsync(buffer);
                    await output.FlushAsync();

                    Console.WriteLine(
                        $"Отправлен файл: {filePath}"
                    );
                }
            }
            catch (HttpListenerException)
            {
                Console.WriteLine("Сервер завершил работу");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Ошибка сервера: {ex.Message}"
                );
            }
        }
    }
}
