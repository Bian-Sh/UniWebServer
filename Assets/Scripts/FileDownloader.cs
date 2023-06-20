using EasyButtons;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class FileDownloader : MonoBehaviour
{
    public string url;
    public string folder = "For Upload Example/To";
    [Button]
    async void DownLoadAsync()
    {
        var orign = Path.Combine(Application.streamingAssetsPath, "For Upload Example/From/City Of stars  - 王OK.mp4");
        var target = Path.Combine(Application.streamingAssetsPath, "StaticPage/test.mp4");
        if (!File.Exists(target))
        {
            #region UNITY_EDITOR
            UnityEditor.FileUtil.CopyFileOrDirectory(orign, target);
            #endregion
        }
        
        var path = Path.Combine(Application.streamingAssetsPath, folder);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        var file = Path.Combine(path, "test.mp4");
        if (File.Exists(file))
        {
            File.Delete(file);
        }

        using var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
        using var dh = new DownloadHandlerFile(file);
        dh.removeFileOnAbort = true;
        uwr.downloadHandler = dh;
        var progress = new Progress<float>(p => Debug.Log($"download Progress: {p}"));
        var op = uwr.SendWebRequest();
        while (!op.isDone)
        {
            ((IProgress<float>)progress).Report(uwr.downloadProgress);            
            await Task.Delay(200);
        }

        if (!string.IsNullOrEmpty(uwr.error))
        {
            Debug.Log(uwr.error);
        }
        else
        {
            Debug.Log("File successfully downloaded and saved to " + file);
        }
    }
}
