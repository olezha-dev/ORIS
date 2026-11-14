using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.XPath;
using Project;


class program
{
    private const string FolderName = "./pages";
    static List<TaskItem> Tasks = new List<TaskItem>();
    static int NextTaskId = 1;
    private static readonly HttpListener listener = new();

    private static readonly TaskService taskService = new();



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
                HttpListenerContext context =
                await listener.GetContextAsync();

                _ = ProcessRequestAsync(context, listener);
            }
            catch
            {
                Console.WriteLine($"ошибка при получении запроса");
            }

        }

    }
    private async static Task ProcessRequestAsync(HttpListenerContext context, HttpListener listener)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        FileInfo finfo = new FileInfo(FolderName);
        string Fullp = Path.GetFullPath(finfo.FullName);
        Console.WriteLine($"поиск файла: {finfo}, полный путь: {Fullp}");
        string path = context.Request.Url.AbsolutePath;
        string method = request.HttpMethod;

        string filePath;
        bool steam = false;
        if (method == "GET" && path == "/")
        {
            filePath = Path.Combine(FolderName, "search.html");
            Console.WriteLine(path);
        }
        else if (method == "GET" && path == "/tasks")
        {
            await GetTasks(request, response);
        }
        else if (method == "GET" && path == "/tasks/details")
        {
            await GetTaskDetails(request, response);
        }
        else if (method == "POST" && path == "/tasks/create")
        {
            await CreateTask(request, response);
        }
        else if (method == "PUT" && path == "/tasks/update")
        {
            await UpdateTask(request, response);
        }
        else if (method == "GET" && path == "/tasks/create.html")
        {
            await SendStaticFileAsync(
                response,
                "tasks/create.html"
            );
        }
        else
        {
            string relativePath = path.TrimStart('/');

            await SendStaticFileAsync(
                response,
                relativePath
            );
        }
        static async Task GetTasks(
            HttpListenerRequest request,
            HttpListenerResponse response)
        {
            string? priority = request.QueryString["priority"];

            if (!string.IsNullOrWhiteSpace(priority)
                && !TaskService.IsValidPriority(priority.ToLowerInvariant()))
            {
                await SendErrorResponseAsync(
                    response,
                    "Некорректный приоритет",
                    HttpStatusCode.BadRequest
                );

                return;
            }

            List<TaskItem> tasks = taskService.GetTasks(priority);

            StringBuilder html = new();

            html.Append("""
            <!DOCTYPE html>
            <html lang="ru">
            <head>
                <meta charset="UTF-8">
                <title>Список задач</title>
                <link rel="stylesheet" href="/css/style.css">
            </head>
            <body>
                <h1>Список задач</h1>

                <form method="GET" action="/tasks">
                    <label>Фильтр по приоритету:</label>
                    <select name="priority">
                        <option value="">Все</option>
                        <option value="low">Низкий</option>
                        <option value="medium">Средний</option>
                        <option value="high">Высокий</option>
                    </select>
                    <button type="submit">Фильтровать</button>
                </form>

                <a href="/tasks/create.html">Создать задачу</a>

                <ul>
            """);

            foreach (TaskItem task in tasks)
            {
                string safeTitle = WebUtility.HtmlEncode(task.Title);

                html.Append($"""
                <li>
                    <a href="/tasks/details?id={task.Id}">
                        {safeTitle}
                    </a>
                    — {task.Priority}
                    — {task.Status}
                </li>
                """);
            }

            html.Append("""
                </ul>
                <a href="/">На главную</a>
            </body>
            </html>
            """);

            await SendHtmlAsync(
                response,
                html.ToString(),
                HttpStatusCode.OK
            );
        }
        static async Task GetTaskDetails(
            HttpListenerRequest request,
            HttpListenerResponse response)
        {
            string? idString = request.QueryString["id"];

            if (!int.TryParse(idString, out int id))
            {
                await SendErrorResponseAsync(
                    response,
                    "Некорректный ID",
                    HttpStatusCode.BadRequest
                );

                return;
            }

            TaskItem? task = taskService.GetTask(id);

            if (task == null)
            {
                await SendErrorResponseAsync(
                    response,
                    "Задача не найдена",
                    HttpStatusCode.NotFound
                );

                return;
            }

            string safeTitle = WebUtility.HtmlEncode(task.Title);

            string html = $"""
            <!DOCTYPE html>
            <html lang="ru">
            <head>
                <meta charset="UTF-8">
                <title>Задача №{task.Id}</title>
                <link rel="stylesheet" href="/css/style.css">
            </head>
            <body>
                <h1>Задача №{task.Id}</h1>

                <h2>{safeTitle}</h2>

                <p>Приоритет: {task.Priority}</p>
                <p>Статус: {task.Status}</p>

                <a href="/tasks">Назад к списку</a>
            </body>
            </html>
            """;

            await SendHtmlAsync(
                response,
                html,
                HttpStatusCode.OK
            );
        }
        static async Task CreateTask(
            HttpListenerRequest request,
            HttpListenerResponse response)
        {
            string body = await ReadRequestBody(request);

            Dictionary<string, string> formData =
                ParseFormData(body);

            if (!formData.TryGetValue("title", out string? title)
                || !formData.TryGetValue("priority", out string? priority))
            {
                await SendErrorResponseAsync(
                    response,
                    "Не заполнены обязательные поля",
                    HttpStatusCode.BadRequest
                );

                return;
            }

            title = title.Trim();
            priority = priority.ToLowerInvariant();

            if (title.Length < 3 || title.Length > 80
                || !TaskService.IsValidPriority(priority))
            {
                await SendErrorResponseAsync(
                    response,
                    "Некорректные данные задачи",
                    HttpStatusCode.BadRequest
                );

                return;
            }

            TaskItem task = taskService.CreateTask(title, priority);

            response.StatusCode = (int)HttpStatusCode.SeeOther;

            response.RedirectLocation =
                $"/tasks/details?id={task.Id}";

            response.Close();
        }
        static async Task UpdateTask(
            HttpListenerRequest request,
            HttpListenerResponse response)
        {
            string? idString = request.QueryString["id"];

            if (!int.TryParse(idString, out int id))
            {
                await SendErrorResponseAsync(
                    response,
                    "Некорректный ID",
                    HttpStatusCode.BadRequest
                );

                return;
            }

            string body = await ReadRequestBody(request);

            Dictionary<string, string> formData =
                ParseFormData(body);

            if (!formData.TryGetValue("title", out string? title)
                || !formData.TryGetValue("priority", out string? priority)
                || !formData.TryGetValue("status", out string? status))
            {
                await SendErrorResponseAsync(
                    response,
                    "Не заполнены все поля",
                    HttpStatusCode.BadRequest
                );

                return;
            }

            title = title.Trim();
            priority = priority.ToLowerInvariant();
            status = status.ToLowerInvariant();

            if (title.Length < 3 || title.Length > 80
                || !TaskService.IsValidPriority(priority)
                || !TaskService.IsValidStatus(status))
            {
                await SendErrorResponseAsync(
                    response,
                    "Некорректные данные",
                    HttpStatusCode.BadRequest
                );

                return;
            }

            bool updated = taskService.UpdateTask(
                id,
                title,
                priority,
                status
            );

            if (!updated)
            {
                await SendErrorResponseAsync(
                    response,
                    "Задача не найдена",
                    HttpStatusCode.NotFound
                );

                return;
            }

            response.StatusCode = (int)HttpStatusCode.NoContent;
            response.Close();
        }

        static async Task<string> ReadRequestBody(
            HttpListenerRequest request)
        {
            using StreamReader reader = new(
                request.InputStream,
                request.ContentEncoding
            );

            return await reader.ReadToEndAsync();
        }

        static Dictionary<string, string> ParseFormData(string body)
        {
            Dictionary<string, string> result = new();

            string[] pairs = body.Split('&');

            foreach (string pair in pairs)
            {
                string[] parts = pair.Split('=', 2);

                if (parts.Length != 2)
                    continue;

                string key = Uri.UnescapeDataString(
                    parts[0].Replace("+", " ")
                );

                string value = Uri.UnescapeDataString(
                    parts[1].Replace("+", " ")
                );

                result[key] = value;
            }

            return result;
        }

        static async Task SendStaticFileAsync(
            HttpListenerResponse response,
            string relativePath)
        {
            string fullPath = Path.GetFullPath(
                Path.Combine(FolderName, relativePath)
            );

            string pagesRoot = Path.GetFullPath(FolderName)
                + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(
                pagesRoot,
                StringComparison.OrdinalIgnoreCase))
            {
                await SendErrorResponseAsync(
                    response,
                    "Недопустимый путь",
                    HttpStatusCode.BadRequest
                );

                return;
            }

            if (!File.Exists(fullPath))
            {
                await SendErrorResponseAsync(
                    response,
                    "Страница не найдена",
                    HttpStatusCode.NotFound
                );

                return;
            }

            string content = await File.ReadAllTextAsync(fullPath);

            string extension = Path.GetExtension(fullPath);

            response.ContentType = extension switch
            {
                ".html" => "text/html; charset=utf-8",
                ".css" => "text/css; charset=utf-8",
                ".js" => "application/javascript; charset=utf-8",
                _ => "application/octet-stream"
            };

            byte[] buffer = Encoding.UTF8.GetBytes(content);

            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer);

            response.Close();
        }

        static async Task SendHtmlAsync(
            HttpListenerResponse response,
            string content,
            HttpStatusCode statusCode)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);

            response.StatusCode = (int)statusCode;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer);

            response.Close();
        }

        static async Task SendErrorResponseAsync(
            HttpListenerResponse response,
            string message,
            HttpStatusCode statusCode)
        {
            string safeMessage = WebUtility.HtmlEncode(message);

            string html = $"""
            <!DOCTYPE html>
            <html lang="ru">
            <head>
                <meta charset="UTF-8">
                <title>Ошибка</title>
            </head>
            <body>
                <h1>Ошибка {(int)statusCode}</h1>
                <p>{safeMessage}</p>
                <a href="/tasks">Вернуться к задачам</a>
            </body>
            </html>
            """;

            await SendHtmlAsync(response, html, statusCode);
        }
    }
}