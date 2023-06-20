using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using static zFramework.Web.Loom;

namespace zFramework.Web
{
    [Serializable]
    public class StaticRouteSetting
    {
        public bool FallbackToIndexHtml = false;
        public string UrlRoot = "/";
        public string StreamingAssetsPath = "";
    }

    public class StaticPageController : IHttpController
    {
        public bool FallbackToIndexHtml { get; }
        public string StreamingAssetsRootPath { get; }
        public string UrlRoot { get; }

        private static readonly string defaultPage = "index.html";

        public StaticPageController(StaticRouteSetting staticRouteSetting)
        {
            FallbackToIndexHtml = staticRouteSetting.FallbackToIndexHtml;
            StreamingAssetsRootPath = staticRouteSetting.StreamingAssetsPath;
            UrlRoot = staticRouteSetting.UrlRoot;
        }

        internal ControllerMethod GetControllerMethod()
        {
            var parameters = new List<Parameter>();

            // set static path
            var urlParams = UrlRoot.Split('/')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            foreach (var urlParam in urlParams)
            {
                parameters.Add(new Parameter()
                {
                    Type = ParameterType.Static,
                    Name = urlParam,
                    IncludeSlash = false,
                });
            }

            parameters.Add(new Parameter()
            {
                ArgumentIndex = 0,
                Name = "path",
                IncludeSlash = true,
                Type = ParameterType.String,
            });


            var methodName = nameof(GetStaticContents);
            var method = GetType().GetTypeInfo().GetDeclaredMethod(methodName);

            var controllerMethod = new ControllerMethod()
            {
                Controller = this,
                MethodInfo = method,
                CheckMethod = null,
                Parameters = parameters
            };
            return controllerMethod;
        }


        public async Task GetStaticContents(string path, HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.HttpMethod == "GET")
            {
                Debug.Log($"{nameof(StaticPageController)}: path = {path}");
                await SendFile(path, request, response);
            }
            else
            {
                response.StatusCode = 405;
            }
        }

        private async Task SendFile(string path, HttpListenerRequest request, HttpListenerResponse response)
        {
            var filePath = GetFilePath(path);
            Debug.Log($"{nameof(StaticPageController)}:  filepath = {filePath}");
            if (!File.Exists(filePath) && FallbackToIndexHtml)
            {
                filePath = GetFilePath("/");
            }

            if (!File.Exists(filePath))
            {
                response.StatusCode = 404;
                return;
            }

            var filename = Path.GetFileName(filePath);
            response.ContentType = MimeMapping.GetMimeMapping(filename);

            try
            {
                var contents = File.ReadAllBytes(filePath);
                response.Headers.Add(HttpRequestHeader.ContentLength, contents.LongLength.ToString());
                response.ContentLength64 = contents.LongLength;
                await ToOtherThread;
                await response.OutputStream.WriteAsync(contents, 0, contents.Length);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        private string GetFilePath(string path)
        {
            var rootPath = StreamingAssetsRootPath.Trim('/');
            if (path == "/")
            {
                path = defaultPage;
            }
            else if (path.EndsWith("/"))
            {
                path += defaultPage;
            }

            return Path.Combine(Application.streamingAssetsPath, rootPath, path);
        }
    }
}