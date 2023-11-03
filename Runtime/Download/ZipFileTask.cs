using System;
using System.Collections;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace GameBox
{
    /// <summary>
    /// 存放在服务器上的文件
    /// </summary>
    public class ZipFileTask : BaseTask<string>
    {
        public ZipFileTask(DownloadInfo info) : base(info)
        {
        }

        public override IEnumerator ReadFromLocal()
        {
            string save_path = Info.LocalPath.Replace(".zip", "");
            if (Directory.Exists(save_path))
            {
                Debug.LogFormat("本地已存在解压文件: {0}", save_path);
                DownloadComplete(save_path);
            }
            else if (File.Exists(Info.LocalPath))
            {
                //解压文件
                Debug.LogFormat("本地已存在压缩文件: {0}", Info.LocalPath);
                if (!UnzipTarget(Info.LocalPath, save_path, () => DownloadComplete(save_path)))
                {
                    Info.State = DownloadState.Completed;
                    Debug.Log($"解压失败");
                }
            }
            else if (File.Exists(Info.StreamingPath))
            {
                Debug.LogFormat("Streaming下存在压缩文件: {0}", Info.StreamingPath);
                if (!UnzipTarget(Info.StreamingPath, save_path, () => DownloadComplete(save_path)))
                {
                    Info.State = DownloadState.Completed;
                    Debug.Log($"解压失败,但不再重新下载");
                }
            }
            else
            {
                Debug.Log($"本地不存在:{Info.ID}");
                Info.FailedAction?.Invoke();
            }
            yield return null;
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
                    byte[] bytes = Request.downloadHandler.data;
                    if (!string.IsNullOrEmpty(Info.LocalPath))
                    {
                        if (File.Exists(Info.LocalPath))
                        {
                            Debug.Log($"存在对应资源:{Info.LocalPath}");
                            File.Delete(Info.LocalPath);
                        }
                        SaveBytes(Info.LocalPath, bytes);
                        string save_path = Info.LocalPath.Replace(".zip", "");
                        if (!UnzipTarget(Info.LocalPath, save_path, () => DownloadComplete(save_path)))
                        {
                            Info.State = DownloadState.Completed;
                            Debug.Log($"解压失败,但不再重新下载");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 解压缩文件流
        /// </summary>
        /// <param name="zip_path">zip文件路径</param>
        /// <param name="unzipPath">解压缩到地址</param>
        /// <param name="onUnzipSuccess">解压缩成功回调</param>
        /// <returns>是否解压缩成功</returns>
        private static bool UnzipTarget(string zip_path, string unzipPath, Action onUnzipSuccess)
        {
            Debug.Log($"开始解压文件到:{unzipPath}");
            ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = System.Text.Encoding.UTF8.CodePage;
            bool success = false;
            if (File.Exists(zip_path))
            {
                Unzip(zip_path, unzipPath);
                onUnzipSuccess?.Invoke();
                success = true;
            }

            if (success)
            {
                Debug.Log("文件导入完毕");
                File.Delete(zip_path);
            }

            return success;
        }

        /// <summary>
        /// 解压缩Zip文件
        /// </summary>
        /// <param name="zipFile">zip名称</param>
        /// <param name="outPath">输出地址</param>
        public static void Unzip(string zipFile, string outPath)
        {
            if (!File.Exists(zipFile))
            {
                Debug.Log("Zip文件不存在");
                return;
            }

            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            ZipEntry zip;
            var zipStream = new ZipInputStream(File.OpenRead(zipFile));
            bool isError = false;
            while ((zip = zipStream.GetNextEntry()) != null)
            {
                bool error = UnzipFile(zip, zipStream, outPath);
                if (error)
                {
                    isError = true;
                    Debug.LogError("解压错误");
                    break;
                }
            }
            try
            {
                zipStream.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError("UnZip Error");
                throw ex;
            }
            if (!isError)
                Debug.Log("解压完成：" + outPath);
        }

        private static bool UnzipFile(ZipEntry zip, ZipInputStream zipStream, string outPath)
        {
            try
            {
                //文件名不为空  
                if (!string.IsNullOrEmpty(zip.Name))
                {
                    string filePath = $"{outPath}/{zip.Name}";

                    filePath = filePath.Replace("//", "/");
                    //是文件夹则不需要处理
                    if (IsDirectory(filePath))
                    {
                        //没有则夹创建文件夹
                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);
                    }
                    else
                    {
                        //找到文件夹
                        string directory = GetDirectory(filePath);
                        //检测是否有文件夹
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            //没有则夹创建文件夹
                            if (!Directory.Exists(directory))
                                Directory.CreateDirectory(directory);
                        }
                        FileStream fs = null;
                        //当前文件夹下有该文件  删掉  重新创建  
                        if (File.Exists(filePath))
                            File.Delete(filePath);

                        fs = File.Create(filePath);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        //每次读取2MB  直到把这个内容读完  
                        while (true)
                        {
                            size = zipStream.Read(data, 0, data.Length);
                            //小于0， 也就读完了当前的流  
                            if (size > 0) fs.Write(data, 0, size);
                            else break;
                        }
                        fs.Close();
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return true;
            }
        }

        /// <summary>  
        /// 是否是目录文件夹  
        /// </summary>  
        /// <param name="path"></param>  
        /// <returns></returns>  
        static bool IsDirectory(string path) => path[path.Length - 1] == '/';

        /// <summary>  
        /// 目录文件  
        /// </summary>  
        /// <param name="path"></param>  
        /// <returns></returns>  
        static string GetDirectory(string path)
        {
            for (int i = path.Length - 1; i >= 0; --i)
            {
                if (path[i] == '/')
                {
                    string stringPath = new string(path.ToCharArray(), 0, i);
                    return stringPath.ToString();
                }
            }
            return null;
        }
    }
}
