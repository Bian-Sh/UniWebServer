using HttpMultipartParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using  static zFramework.Web.Loom;

namespace zFramework.Web.Samples
{
    public class FileUploadSample : MonoBehaviour, IHttpController
    {
        [SerializeField]
        private ImageViewer imageViewer;

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
                    var file = parser.Files[0];
                    uploadedFileHtml =
                        $@"<div>
                             <p>FileName: {file.FileName}</p>
                             <p>ContentType: {file.ContentType}</p>
                             <p>FileSize: {file.Data.Length} bytes</p>
                           </div>";



                    async Task SaveFileAsync(FilePart file)
                    {
 
                        //  把获取到的文件保存到 StreamingAssets 文件夹下 （file.Data 类型是 Stream）
                        var path = Application.streamingAssetsPath + "/" + file.FileName;
                        var folder = Path.GetDirectoryName(path);
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            Debug.Log($"{nameof(FileUploadSample)}: 2. Thread ID = {Thread.CurrentThread.ManagedThreadId}");
                            await file.Data.CopyToAsync(fileStream);
                        }
                    }
                    
                     foreach (var item in parser.Files)
                    {
                        _ = SaveFileAsync(item);
                    }
                     
                    await ToMainThread;
                    Debug.Log($"{nameof(FileUploadSample)}:  file content = {file.ContentType}");
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
    }
}