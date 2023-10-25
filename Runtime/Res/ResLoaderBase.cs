using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace GameBox
{
    public class ResLoaderBase: IResLoader
    {
        private static readonly string Tag = "[Res]";

        
        private bool _showLog = false;
        public bool ShowLog
        {
            get => _showLog;
            set => _showLog = value;
        }
        
        /// <summary>
        /// 平台参数
        /// </summary>
        private static string Platform
        {
            get
            {
#if UNITY_IOS
                return "ios";
#else
                return "android";
#endif 
            }
        }

        /// <summary>
        /// Bundle存放路径 (全小写)
        /// </summary>
        public static string BundleDirPath => $"assetbundles/{Platform}";


        public static string BundleStreamingPath(string bundleName)
            => $"{Application.streamingAssetsPath}/${BundleDirPath}/{bundleName}";
        
        public static string BundleCachingPath(string bundleName)
            => $"{Application.persistentDataPath}/${BundleDirPath}/{bundleName}";
        

        private string _bundleSecret = ""; // 加密秘钥

        public string BundleSecret
        {
            get => _bundleSecret;
            set => _bundleSecret = value;
        }

        #region 加载接口
        
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="bundleName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Load<T>(string asset, string bundleName = "") where T : Object
        {

            if (string.IsNullOrEmpty(bundleName))
            {
                // Resources 加载
                return Resources.Load<T>(asset);
            }
            else
            {
                // Bundle 加载
                var ab = LoadBundle(bundleName);
                if (ab != null)
                {
                    return ab.LoadAsset<T>(asset);
                }
                else
                {
                    LogD($">>> LoadBundle fail: {bundleName}");
                }
            }


            return null;
        }
        

        /// <summary>
        /// 加载包内Bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        protected virtual AssetBundle LoadStreamingBundle(string bundleName, string secret = "")
        {
            string filePath = $"{Application.streamingAssetsPath}/{BundleDirPath}/{bundleName}";
            LogD($"Load streaming bundles: {filePath}");
            return TryLoadBundleFromPath(filePath, secret);
        }

        /// <summary>
        /// 加载缓存的Bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        protected virtual AssetBundle LoadSavedBundle(string bundleName, string secret = "")
        {
            string filePath = $"{Application.persistentDataPath}/{BundleDirPath}/{bundleName}";
            if (!File.Exists(filePath)) return null;
            LogD($"Load saved bundles: {filePath}");
            return TryLoadBundleFromPath(filePath, secret);
        }

        /// <summary>
        /// 加载Bundle接口
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="forceStreaming">必须使用包内的Bundle</param>
        /// <returns></returns>
        public virtual AssetBundle LoadBundle(string bundleName, bool forceStreaming = false)
        {
            AssetBundle ab = null;
            if(!forceStreaming) ab = LoadSavedBundle(bundleName, _bundleSecret); // 优先加载下载好的Bundle
            if (null == ab) ab = LoadStreamingBundle(bundleName, _bundleSecret); // 再加载包内缓存的Bundle
            return ab;
        }


        /// <summary>
        /// 尝试加载Bundle;
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bundleName"></param>
        /// <param name="secret">bundle加密秘钥</param>
        /// <returns></returns>
        protected virtual AssetBundle TryLoadBundleFromPath(string filePath, string secret = "")
        {
            if(string.IsNullOrEmpty(filePath)) return null;
            if (string.IsNullOrEmpty(secret)) return AssetBundle.LoadFromFile(filePath); // 无加密则直接加载Bundle
            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                return Encrypter.DecryptBundle(data, secret); //  加载加密的 bundle 
            }
            catch (Exception e)
            {
                Debug.LogError($"-- [LoaderBase] Load EncyptBundle fail: {filePath} :: {e}");
            }
            return null;
        }



        #endregion

        #region 异步加载

        /// <summary>
        /// 异步加载Bundle
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <param name="autoCache">自动缓存bundle</param>
        public void LoadBundleAsync(string url, Action<AssetBundle, string> callback, bool autoCache = true)
        {
            UnityWebRequest w = UnityWebRequest.Get(url);
            w.downloadHandler = new DownloadHandlerBuffer();
            w.SendWebRequest().completed += ao =>
            {
                if (w.result == UnityWebRequest.Result.Success)
                {
                    AssetBundle bundle = null;
                    byte[] data = w.downloadHandler.data;
                    if (data != null)
                    {
                        Debug.Log($"Data is loaded: {url}");
                        try
                        {
                            if (string.IsNullOrEmpty(_bundleSecret))
                            {
                                bundle = AssetBundle.LoadFromMemory(data);
                            }
                            else
                            {
                                Debug.Log($"start load enc bundle: {_bundleSecret}");
                                bundle = Encrypter.DecryptBundle(data, _bundleSecret);
                            }
                            
                            if (autoCache)
                            {
                                SaveBundleToCache(data, bundle.name);
                            }
                            
                            callback?.Invoke(bundle, url);

                            return;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Creat Bundle error: {e}");
                        }
                    }
                }
                
                Debug.LogError($"Load Bundle error: {url}");
                Debug.LogError($"{w.error}");
                callback?.Invoke(null, url);
            };
        }

        /// <summary>
        /// 将 Bundle 缓存至本地目录
        /// </summary>
        /// <param name="bundle"></param>
        public void SaveBundleToCache(byte[] data, string bundleName)
        {
            var file = BundleCachingPath(bundleName);
            File.WriteAllBytes(file, data);
        }

        #endregion

        #region 资源释放
        
        
        /// <summary>
        /// 释放所有资源
        /// </summary>
        public virtual void Dispose()
        {
        }


        #endregion

        #region 日志输出

        public void LogD(object data)
        {
            if (ShowLog)
            {
                Debug.Log($"{Tag} {data}");
            }
        }

        public void LogE(object data)
        {
            if (ShowLog)
            {
                Debug.Log($"<color=red>{Tag} {data}</color>");
            }
        }

        #endregion

        #region 编辑器模拟加载
        /// <summary>
        /// 编辑器模拟加载bundle资源
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="bundleName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T SimulationLoadAsset<T>(string asset, string bundleName) where T : Object
        {
#if UNITY_EDITOR
            var manifest = GBundleManifest.Load();
            if (manifest != null)
            {
                var resPath = manifest.GetAssetResPath(asset, bundleName);
                if (!string.IsNullOrEmpty(resPath))
                {
                    LogD($">>> Simulation load: {asset} from {resPath}");
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(resPath);
                }
            }
#endif
            LogD($">>> Simulation can't running without editor: {bundleName}/{asset}");
            return null;
        }


        #endregion
    }
}