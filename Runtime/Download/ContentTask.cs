using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace GameBox
{
    /// <summary>
    /// 一般指存放在服务器上的小文本文件,方便更改
    /// </summary>
    public class ContentTask : BaseTask<string>
    {
        public ContentTask(DownloadInfo info) : base(info)
        {
        }

        public override IEnumerator ReadFromLocal()
        {
            yield return null;
            string content = null;
            if (File.Exists(Info.LocalPath))
            {
                Debug.LogFormat("本地读取: {0}", Info.LocalPath);
                content = File.ReadAllText(Info.LocalPath);
            }
            else if (File.Exists(Info.StreamingPath))
            {
                Debug.LogFormat("Streaming下读取: {0}", Info.StreamingPath);
                content = File.ReadAllText(Info.StreamingPath);
            }
            if (!string.IsNullOrEmpty(content))
                DownloadComplete(content);
            else
            {
                Debug.Log($"本地不存在:{Info.ID}");
                Info.FailedAction?.Invoke();
            }
        }

        public override IEnumerator DownloadFromAzure()
        {
            Debug.LogFormat("连接服务器下载:{0}", Info.ID);
            ///下载会跟服务器要真实地址
            var Request = UnityWebRequest.Get(Info.NetPath);
            yield return Request.SendWebRequest();
            if (Request.isDone)
            {
                if (Request.result == UnityWebRequest.Result.ConnectionError || Request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogFormat("下载失败: {0}", Request.error);
                    Info.State = DownloadState.Wait;
                    yield return GetWaitForSeconds();
                    Info.State = DownloadState.Running;
                    Info.FailedAction?.Invoke();
                }
                else
                {
                    Debug.LogFormat("下载成功: {0}", Info.ID);
                    if (!string.IsNullOrEmpty(Info.LocalPath))
                    {
                        //保存数据
                        try
                        {
                            if (File.Exists(Info.LocalPath))
                            {
                                Debug.Log($"存在对应资源:{Info.LocalPath}");
                                File.Delete(Info.LocalPath);
                            }
                            CreateText(Info.LocalPath, Request.downloadHandler.text);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.Log($"存储文本资源失败:{ex.Message}");
                        }
                    }
                    DownloadComplete(Request.downloadHandler.text);
                }
            }
        }

        /// <summary>
        /// 创建文本
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        private void CreateText(string path, string content)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            if (File.Exists(path)) File.Delete(path);
            File.AppendAllText(path, content, System.Text.Encoding.UTF8);
        }
    }
}
