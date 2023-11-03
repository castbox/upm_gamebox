using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace GameBox
{
    public class AssetBundleTask : BaseTask<AssetBundle>
    {
        public AssetBundleTask(DownloadInfo info) : base(info)
        {

        }

        /// <summary>
        /// 从本地读取
        /// </summary>
        /// <param name="unityAction"></param>
        /// <returns></returns>
        public override IEnumerator ReadFromLocal()
        {
            AssetBundle assetBundle = AssetBundle.GetAllLoadedAssetBundles().Where(e => IsSameFile(e, Info.LocalPath)).First();
            if (assetBundle != null)
            {
                DownloadComplete(assetBundle);
                yield return new WaitForEndOfFrame();
            }
            else
            {
                AssetBundleCreateRequest Request = null;
#if DEVELOPER_MODE
				Request = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, Info.ID));
#else
                var path = Info.LocalPath.Replace(@"\", "/");
                if (File.Exists(path))
                    Request = AssetBundle.LoadFromFileAsync(path);
#endif
                if (Request != null)
                {
                    yield return Request;
                    if (Request.isDone)
                    {
                        assetBundle = Request.assetBundle;
                        if (assetBundle != null)
                        {
                            Debug.LogFormat("本地加载成功: {0}", Info.ID);
                            DownloadComplete(assetBundle);
                        }
                        else
                        {
                            Debug.LogFormat("本地加载完成,但解压失败: {0}", Info.ID);
                            Info.FailedAction?.Invoke();
                        }
                    }
                }
                else
                {
                    Debug.LogError($"未找到:{Info.LocalPath}");
                    Info.FailedAction?.Invoke();
                }
            }
        }

        /// <summary>
        /// 从服务器下载
        /// </summary>
        /// <param name="unityAction"></param>
        /// <returns></returns>
        public override IEnumerator DownloadFromAzure()
        {
            //下载会跟服务器要真实地址
            UnityWebRequest Request = UnityWebRequest.Get(Info.NetPath);
            yield return Request.SendWebRequest();
            if (Request.isDone)
            {
                if (Request.result == UnityWebRequest.Result.ConnectionError || Request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log($"下载失败: {Request.error}");
                    Info.State = DownloadState.Wait;
                    yield return GetWaitForSeconds();
                    Info.State = DownloadState.Running;
                    Info.FailedAction?.Invoke();
                }
                else
                {
                    AssetBundle assetBundle = AssetBundle.GetAllLoadedAssetBundles().Where(bundle => IsSameFile(bundle, Info.ID)).First();
                    if (assetBundle != null)
                    {
                        Debug.Log($"卸载旧资源:{Info.ID}");
                        assetBundle.Unload(true);
                    };
                    var bundleRequest = AssetBundle.LoadFromMemoryAsync(Request.downloadHandler.data);
                    yield return bundleRequest;
                    assetBundle = bundleRequest.assetBundle;
                    if (assetBundle != null)
                    {
                        Debug.LogFormat("下载成功: {0}", Info.ID);
                        var path = Path.Combine(Application.persistentDataPath, Info.ID);
                        SaveBytes(path, Request.downloadHandler.data);
                        DownloadComplete(assetBundle);
                    }
                    else
                    {
                        Debug.LogFormat("下载完成,但解压失败: {0}", Info.ID);
                        Info.State = DownloadState.Wait;
                        yield return GetWaitForSeconds();
                        Info.State = DownloadState.Running;
                        Info.FailedAction?.Invoke();
                    }
                }
            }
        }

        /// <summary>
        /// 是否是同一个文件{针对默认bundle的名字为空的问题}
        /// </summary>
        /// <param name="assetBundle"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool IsSameFile(AssetBundle assetBundle, string fileName)
        {
            if (assetBundle == null) return false;
            return IsSameFile(assetBundle.name, fileName);
        }

        /// <summary>
        /// 是同一个文件
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        private bool IsSameFile(string path1, string path2)
        {
            if (path1.Equals(path2)) return true;
            path1 = path1.Replace("\\", "/").Replace("//", "/").ToLower();
            path2 = path2.Replace("\\", "/").Replace("//", "/").ToLower();
            if (path1.EndsWith(string.Format("/{0}", path2))) return true;
            if (path2.EndsWith(string.Format("/{0}", path1))) return true;
            return false;
        }
    }
}
