using EasyButtons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using zFramework.Web;

public class FileUploader : MonoBehaviour
{
    public string uploadURL;
    [Header("上传的文件，相对 StreamingAssets")]
    public List<string> files = new List<string>();
    //Assets/StreamingAssets/For Upload Example/From/City Of stars  - 王OK.mp4
    //Assets/StreamingAssets/For Upload Example/From/Cover.png
    //Assets/StreamingAssets/For Upload Example/From/Test.zip

    [Button("使用 UnityWebRequest")]
    async Task UploadFiles()
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        foreach (var filePath in files)
        {
            var fileFullPath = Path.Combine(Application.streamingAssetsPath, filePath);
            byte[] fileData = File.ReadAllBytes(fileFullPath);
            string fileName = Path.GetFileName(fileFullPath);
            fileName = Uri.EscapeDataString(fileName);// 解决中文乱码问题
            formData.Add(new MultipartFormFileSection("files[]", fileData, fileName, "application/octet-stream"));
        }

        using (UnityWebRequest request = UnityWebRequest.Post(uploadURL, formData))
        {
            var progress = new Progress<float>(p => Debug.Log($"Upload Progress: {p}"));
            request.SendWebRequest();

            while (!request.isDone)
            {
                ((IProgress<float>)progress).Report(request.uploadProgress);
                await Task.Delay(100);
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Upload complete!");
            }
            else
            {
                Debug.Log($"Upload failed with error: {request.error}");
            }
        }
    }

    [Button("使用 HttpClient")]

    async Task UploadFilesV2()
    {
        using (var client = new HttpClient())
        using (var content = new MultipartFormDataContent())
        {
            foreach (var filePath in files)
            {
                try
                {
                    var fileFullPath = Path.Combine(Application.streamingAssetsPath, filePath);
                    byte[] fileData = File.ReadAllBytes(fileFullPath);
                    string fileName = Path.GetFileName(fileFullPath);
                    fileName = Uri.EscapeDataString(fileName);

                    var fileContent = new ByteArrayContent(fileData);
                    var contenttype = new System.Net.Http.Headers.MediaTypeHeaderValue(MimeMapping.GetMimeMapping(fileName));
                    fileContent.Headers.ContentType = contenttype;
                    content.Add(fileContent, "files[]", fileName);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{nameof(FileUploader)}: MultipartFormData  错误 {e}");
                }
            }

            //var progress = new Progress<float>(p => Debug.Log($"Upload Progress: {p}"));
            var response = await client.PostAsync(uploadURL, content);

            if (response.IsSuccessStatusCode)
            {
                Debug.Log("Upload complete!");
            }
            else
            {
                Debug.Log($"Upload failed with error: {response.StatusCode}");
            }
        }
    }

    /*
    IEnumerator UploadFiles(List<string> filePaths, string uploadURL)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        foreach (var filePath in filePaths)
        {
            Debug.Log($"{nameof(FileUploader)}: file exist {File.Exists(filePath)}");
            Debug.Log($"{nameof(FileUploader)}: file = {filePath} ");
            byte[] fileData = File.ReadAllBytes(filePath);
            string fileName = Path.GetFileName(filePath);
            formData.Add(new MultipartFormFileSection("files[]", fileData, fileName, "application/octet-stream"));
        }
        Debug.Log($"{nameof(FileUploader)}:  formData count = {formData.Count}");
        
        foreach (var section in formData)
        {
            Debug.Log($"{nameof(FileUploader)}: section out");
            if (section is MultipartFormFileSection fileSection)
            {
                if (fileSection.sectionData == null || fileSection.sectionData.Length < 0)
                {
                    Debug.LogError("Invalid file section data!");
                }
            }
        }
        
        byte[] boundary = UnityWebRequest.GenerateBoundary();
        using (UnityWebRequest request = UnityWebRequest.Post(uploadURL, formData, boundary))
        {
            request.SendWebRequest();
            while (!request.isDone)
            {
                Debug.Log($"Upload Progress: {request.uploadProgress}");
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Upload complete!");
            }
            else
            {
                Debug.Log($"Upload failed with error: {request.error}");
            }
        }
    }
     */
}
