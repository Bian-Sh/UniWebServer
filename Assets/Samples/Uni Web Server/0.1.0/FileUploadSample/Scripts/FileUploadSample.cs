using HttpMultipartParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static zFramework.Web.Loom;

namespace zFramework.Web.Samples
{
    public class FileUploadSample : MonoBehaviour, IHttpController
    {
        [SerializeField]
        private ImageViewer imageViewer;
        public string savePath = "For Upload Example/To";

        [Route("/")]
        public async Task<string> FileUploadPage(HttpListenerRequest request)
        {
            var uploadedFileHtml = "";
            if (request.HttpMethod == "POST")
            {
                Debug.Log($"{nameof(FileUploadSample)}: 0. Thread ID = {Thread.CurrentThread.ManagedThreadId}");
                await ToOtherThread;// 使用其他线程做文件解析
                Debug.Log($"{nameof(FileUploadSample)}: 1. Thread ID = {Thread.CurrentThread.ManagedThreadId}");
                var parser = await MultipartFormDataParser.ParseAsync(request.InputStream);

                if (parser.Files.Count >= 1)
                {
                    //  把获取到的文件保存到 StreamingAssets 文件夹下 
                    async Task SaveFileAsync(FilePart file)
                    {
                        var filename = Uri.UnescapeDataString(file.FileName);
                        var path = Path.Combine(Application.streamingAssetsPath, savePath, filename);
                        var folder = Path.GetDirectoryName(path);
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            Debug.Log($"{nameof(FileUploadSample)}: 2. Thread ID = {Thread.CurrentThread.ManagedThreadId}");
                            await ToOtherThread;
                            await file.Data.CopyToAsync(fileStream);
                        }
                    }

                    foreach (var item in parser.Files)
                    {
                        uploadedFileHtml +=
                            $@"<div>
                             <p>FileName: {item.FileName}</p>
                             <p>ContentType: {item.ContentType}</p>
                             <p>FileSize: {item.Data.Length} bytes</p>
                           </div>" + Environment.NewLine;
                        _ = SaveFileAsync(item);
                    }

                    await ToMainThread;
                    var file = parser.Files[0];
                    if (file.ContentType.StartsWith("image/"))
                    {
                        _ = imageViewer.ShowImageAsync(file.Data);
                    }
                }
            }

            var html =
                $@"<html>
                     <body>
                       {uploadedFileHtml}
                       <form method = ""post"" enctype=""multipart/form-data"">
                         <input type=""file"" name=""file""/>
                         <br/>
                         <br/>
                         <input type=""submit"" value=""send""/>
                       </form>
                    </body>
                  </html>";
            return html;
        }

        [Route("/upload")]
        public async Task<string> FileUploadPageV2(HttpListenerRequest request)
        {
            Debug.Log($"{nameof(FileUploadSample)}: file upload v2 is called {request.HttpMethod}");
            var uploadedFileHtml = "";
            if (request.HttpMethod == "POST")
            {
                Debug.Log($"{nameof(FileUploadSample)}: 0. Thread ID = {Thread.CurrentThread.ManagedThreadId}");
                await ToOtherThread;// 使用其他线程做文件解析
                Debug.Log($"{nameof(FileUploadSample)}: 1. Thread ID = {Thread.CurrentThread.ManagedThreadId}");
                var parser = await MultipartFormDataParser.ParseAsync(request.InputStream);

                if (parser.Files.Count >= 1)
                {
                    //  把获取到的文件保存到 StreamingAssets 文件夹下 
                    async Task SaveFileAsync(FilePart file)
                    {
                        var filename = Uri.UnescapeDataString(file.FileName);
                        var path = Path.Combine(Application.streamingAssetsPath, savePath, filename);
                        var folder = Path.GetDirectoryName(path);
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            Debug.Log($"{nameof(FileUploadSample)}: 2. Thread ID = {Thread.CurrentThread.ManagedThreadId}");
                            await ToOtherThread;
                            await file.Data.CopyToAsync(fileStream);
                        }
                    }

                    foreach (var item in parser.Files)
                    {
                        uploadedFileHtml +=
                            $@"<div>
                             <p>FileName: {item.FileName}</p>
                             <p>ContentType: {item.ContentType}</p>
                             <p>FileSize: {item.Data.Length} bytes</p>
                           </div>" + Environment.NewLine;
                        _ = SaveFileAsync(item);
                    }

                    await ToMainThread;
                    var file = parser.Files[0];
                    if (file.ContentType.StartsWith("image/"))
                    {
                        _ = imageViewer.ShowImageAsync(file.Data);
                    }
                }
            }

            var html =
                $@"<html>
                     <body>
                       {uploadedFileHtml}
                       <form method = ""post"" enctype=""multipart/form-data"">
                         <input type=""file"" name=""file""/>
                         <input type=""file"" name=""file""/>
                         <input type=""file"" name=""file""/>
                         <br/>
                         <br/>
                         <input type=""submit"" value=""send""/>
                       </form>
                    </body>
                  </html>";
            return html;
        }
    }
}