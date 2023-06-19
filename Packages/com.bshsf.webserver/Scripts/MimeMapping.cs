using System.Collections.Generic;
using System.IO;
using System;
namespace zFramework.Web
{
    public static class MimeMapping
    {
        private static readonly Dictionary<string, string> _mappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
          {".bmp", "image/bmp"},
          {".gif", "image/gif"},
          {".jpeg", "image/jpeg"},
          {".jpg", "image/jpeg"},
          {".png", "image/png"},
          {".tif", "image/tiff"},
          {".tiff", "image/tiff"},
          {".ico", "image/x-icon"},
          {".svg", "image/svg+xml"},
          {".js", "application/javascript"},
          {".json", "application/json"},
          {".pdf", "application/pdf"},
          {".xml", "application/xml"},
          {".zip", "application/zip"},
          {".doc", "application/msword"},
          {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
          {".xls", "application/vnd.ms-excel" },
          {".xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
          {".mp3","audio/mpeg" },
          {".wav","audio/wav" },
          {".wma","audio/x-ms-wma" },
          {".mid","audio/midi" },
          {".midi","audio/midi" },
          {".mp4","video/mp4" },
          {".avi","video/x-msvideo" },
          {".wmv","video/x-ms-wmv" },
          {".flv","video/x-flv" },
          {".mkv","video/x-matroska" },
          { ".m3u8","application/x-mpegURL"},
          {".html", "text/html"},
          {".htm", "text/html"},
          {".asp", "text/asp"},
          {".aspx", "text/asp"},
          {".php", "text/php"},
          {".jsp", "text/jsp"}};

        public static string GetMimeMapping(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return _mappings.TryGetValue(extension, out var mime) ? mime : "application/octet-stream";
        }
    }
}