using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyHttpServer;
{
    internal class HttpServer()
    {
        private readonly HttpListener _listener;
        private readonly string _htmlFile;

        public HttpServer(string url, string htmlFile)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(url);

            _htmlFile = htmlFile;
        }
        public void Start()
        {
            _listener.Start();

            Console.WriteLine("Сервер запущен!");

            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();

                    ProcessRequest(context);
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }
        private void ProcessRequest(HttpListenerContext context)
        {
            string html = File.ReadAllText(_htmlFile, Encoding.UTF8);

            byte[] buffer = Encoding.UTF8.GetBytes(html);

            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.StatusCode = 200;

            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }
        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
            Console.WriteLine("Сервер остановлен.");
        }
    }
}
