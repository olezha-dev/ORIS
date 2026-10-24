using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace project
{
    internal class ContentTypes
    {

        public static readonly Dictionary<string, string> Types = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".html", "text/html;charset=utf-8" },
                { ".htm", "text/html;charset=utf-8" },

                { ".css", "text/css" },

                { ".js", "text/javascript" },

                { ".json", "application/json" },

                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".gif", "image/gif" },
                { ".svg", "image/svg+xml" },
                { ".webp", "image/webp" },
                { ".ico", "image/x-icon" },

                { ".txt", "text/plain" },

                { ".pdf", "application/pdf" },

                { ".mp4", "video/mp4" },
                { ".webm", "video/webm" },

                { ".woff", "font/woff" },
                { ".woff2", "font/woff2" },
                { ".ttf", "font/ttf" },
                { ".otf", "font/otf" }
        };
    }
}
