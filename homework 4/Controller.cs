using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace project
{
    public class Controller
    {
        private readonly Handler handler = new Handler();
        public async Task ControlAs(HttpListenerContext context)
        {

            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            StreamReader streamReader = new StreamReader(request.InputStream, request.ContentEncoding);
            string body = await streamReader.ReadToEndAsync();
            handler.Handle("","");

            Dictionary<string, string> data = ParseFormData(body);
            data.TryGetValue("login", out string? login);
            data.TryGetValue("password", out string? password);

            string result = handler.Handle(login, password);

            byte[] buffer = Encoding.UTF8.GetBytes(result);

            response.StatusCode = 200;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer);
            response.Close();

        }

        private static Dictionary<string,string> ParseFormData(string body)
        {
            Dictionary<string, string> result = new();
            foreach (string pair in body.Split('&'))
            {
                string[] parts = pair.Split('=', 2);

                if (parts.Length == 2)
                {
                    string key = Uri.UnescapeDataString(
                        parts[0].Replace("+", " ")
                    );

                    string value = Uri.UnescapeDataString(
                        parts[1].Replace("+", " ")
                    );

                    result[key] = value;
                }
            }
            return result;
        }
    }

}
