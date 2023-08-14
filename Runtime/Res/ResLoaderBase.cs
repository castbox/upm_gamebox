using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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

        private static string BundleDirPath
        {
            get
            {
#if UNITY_IOS
                return $"AssetBundles/iOS";
#else
                return $"AssetBundles/Android";
#endif
            }
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
        protected virtual AssetBundle LoadStreamingBundle(string bundleName)
        {
            string filePath = $"{Application.streamingAssetsPath}/{BundleDirPath}/{bundleName}";
            LogD($"Load streaming bundles: {filePath}");
            return TryLoadBundle(filePath, bundleName);
        }

        /// <summary>
        /// 加载缓存的Bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        protected virtual AssetBundle LoadSavedBundle(string bundleName)
        {
            string filePath = $"{Application.persistentDataPath}/{BundleDirPath}/{bundleName}";
            if (!File.Exists(filePath)) return null;
            LogD($"Load saved bundles: {filePath}");
            return TryLoadBundle(filePath, bundleName);
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
            if(!forceStreaming) ab = LoadSavedBundle(bundleName); // 优先加载下载好的Bundle
            if (null == ab) ab = LoadStreamingBundle(bundleName); // 再加载包内缓存的Bundle
            return ab;
        }


        /// <summary>
        /// 尝试加载Bundle;
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        protected virtual AssetBundle TryLoadBundle(string filePath, string bundleName = "")
        {
            if(string.IsNullOrEmpty(filePath)) return null;
            return AssetBundle.LoadFromFile(filePath);
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