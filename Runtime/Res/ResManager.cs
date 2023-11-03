using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace GameBox
{
    /// <summary>
    /// 简易加载器
    /// </summary>
    public class ResManager: GMonoSingleton<ResManager>
    {

        public const string Tag = "[Res]";
        
        /// <summary>
        /// 资源加载器
        /// </summary>
        private ResLoaderBase _loader;
        
        
        private static bool SimulationMode
        {
            get
            {
#if UNITY_EDITOR
                var path = $"{Application.dataPath}/../Library/res_simulate_mode";
                return System.IO.File.Exists(path);
#endif
                return false;
            }
        }
        
        /// <summary>
        /// 平台参数
        /// </summary>
        private static string Platform
        {
            get
            {
#if UNITY_IOS
                return "iOS";
#else
                return "Android";
#endif 
            }
        }
        
        public const string K_ASSET_BUNLDES = "AssetBundles";
        
        /// <summary>
        /// Bundle存放路径 (全小写)
        /// </summary>
        public static string BundleDirPath => $"{K_ASSET_BUNLDES}/{Platform}";
        
        public static string BundleStreamingPath(string bundleName)
            => $"{Application.streamingAssetsPath}/{BundleDirPath.ToLower()}/{bundleName}";
        
        public static string BundleCachingPath(string bundleName)
            => $"{Application.persistentDataPath}/{BundleDirPath.ToLower()}/{bundleName}";
        
        
        
        #region 资源缓存


        /// <summary>
        /// 已缓存的Bundle数据
        /// </summary>
        private Dictionary<string, AssetBundle> _bundles;
        protected Dictionary<string, AssetBundle> Bundles
        {
            get
            {
                if(_bundles == null) _bundles = new Dictionary<string, AssetBundle>();
                return _bundles;
            }
        }

        #endregion


        #region 初始化

        protected override void Init()
        {
            _loader = new ResLoaderBase();
        }

        /// <summary>
        /// 设置加载器秘钥
        /// </summary>
        /// <param name="secret"></param>
        public void SetBundleSecret(string secret, bool useOffset = true)
        {
            _loader.BundleSecret = secret;
            _loader.EncryptedOffset = useOffset;
        }

        #endregion
        
        #region 预加载资源

        /// <summary>
        /// 预加载Bundles
        /// </summary>
        /// <param name="bundles"></param>
        public void PreloaderBundles(IList<string> bundles)
        {
            for (int i = 0; i < bundles.Count(); i++)
            {
                PreLoadBundle(bundles[i]); // 预加载所有的Bundles
            }
        }

        /// <summary>
        /// 实时加载Bundle
        /// </summary>
        /// <param name="bundleName"></param>
        public AssetBundle PreLoadBundle(string bundleName)
        {
            var ab = _loader.LoadBundle(bundleName);
            if (null != ab) Bundles[bundleName] = ab;
            return ab;
        }
        
        /// <summary>
        /// 是否存在Bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public bool IsBundleExsited(string bundleName)
        {
            return Bundles.ContainsKey(bundleName);
        }


     




        #endregion

        #region 资源预加载

        /// <summary>
        /// 预加载嵌入式的Bundle
        /// </summary>
        /// <param name="bundleNames"></param>
        /// <param name="onComplete"></param>
        /// <param name="onProgress"></param>
        public void PreloadEmbedBundles(IList<string> bundleNames, Action onComplete, Action<float> onProgress = null)
            => LoadBundlesInternal(bundleNames, onComplete, onProgress);

        #endregion
        
        
        
        #region Bundle 加载队列

        private Action _bundleLoadCompleteHandle = () => { };
        private Action<float> _bundleLoadProgressHandle = p => { };
        private int _bundleLoadCount;
        private int _bundleLoadIdx;
        private int _retryTimes;
        private List<ResLoadBundleRequest> _loadBundleQuests;

        /// <summary>
        /// 异步加载所有的Bundle
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="onComplete"></param>
        /// <param name="onProgress"></param>
        private void LoadBundlesAsync(IList<ResLoadBundleRequest> requests, Action onComplete, Action<float> onProgress = null)
        {
            _bundleLoadCompleteHandle += onComplete;
            _bundleLoadProgressHandle += onProgress;
            
            if (_loadBundleQuests == null)
            {
                _loadBundleQuests = new List<ResLoadBundleRequest>(10);
            }
            _loadBundleQuests.AddRange(requests);
            
            LoadNextBundle(); // 加载剩余的Bundles
        }
        
        
        /// <summary>
        /// 加载下一个Bundle
        /// </summary>
        private void LoadNextBundle()
        {

            _bundleLoadCount = _loadBundleQuests.Count; // 更新加载计数器
            if (_bundleLoadIdx >= _bundleLoadCount)
            {
                // 全部加载完毕
                _bundleLoadCompleteHandle?.Invoke();
                _bundleLoadIdx = 0;
                _bundleLoadCompleteHandle = () => { };
                _bundleLoadProgressHandle = p => { };
                _bundleLoadCount = 0;
                _loadBundleQuests.Clear();
                return;
            }
            
            string bundleName = _loadBundleQuests[_bundleLoadIdx].name;
            string url = _loadBundleQuests[_bundleLoadIdx].url;
            LoadBundleAsync(url, (bundle, s) =>
            {
                if (bundle != null)
                {
                    Bundles[bundleName] = bundle;
                    _bundleLoadIdx++;
                    _retryTimes = 0;
                    _bundleLoadProgressHandle?.Invoke((float)_bundleLoadIdx/_bundleLoadCount); // 上报进度
                }
                else
                {
                    if (_retryTimes < 3)
                    {
                        _retryTimes++;
                    }
                    else
                    {
                        _retryTimes = 0;
                        _bundleLoadIdx++;
                    }

                    if (!string.IsNullOrEmpty(s))
                    {
                        Debug.LogError($"{Tag} Load bundle failed: {s}");
                    }
                }
                LoadNextBundle();
            });
        }



        /// <summary>
        /// 从URL加载Bundle
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="onComplete"></param>
        /// <param name="onProgress"></param>
        public void LoadBundlesFromUrl(IList<string> urls, Action onComplete, Action<float> onProgress = null)
        {
            List<ResLoadBundleRequest> list = new List<ResLoadBundleRequest>(urls.Count);

            string name = "";
            string url;
            for (int i = 0; i < urls.Count; i++)
            {
                url = urls[i].Replace("\\", "/");
                if (url.Contains("/"))
                {
                    try
                    {
                        name = url.Split('/').Last();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
         
                    if (!string.IsNullOrEmpty(name))
                    {
                        list.Add(ResLoadBundleRequest.Build(name, url)); 
                    }
                }
            }

            if (list.Count == 0) return;

            // 调用接口
            LoadBundlesAsync(list, onComplete, onProgress);
        }

        
        /// <summary>
        /// 从本地缓存中加载Bundles
        /// </summary>
        /// <param name="names"></param>
        /// <param name="onComplete"></param>
        /// <param name="onProgress"></param>
        public void LoadBundlesFromCache(IList<string> names, Action onComplete, Action<float> onProgress = null)
        {
            List<ResLoadBundleRequest> list = new List<ResLoadBundleRequest>(names.Count);
            
            string name = "";
            string url = "";
            for (int i = 0; i < names.Count; i++)
            {
                name = names[i];
                url = BundleCachingPath(name);
#if UNITY_EDITOR || UNITY_IOS
                url = $"file://{url}"; // iOS 和 Editor 需要添加 file 前缀路径
#endif
                list.Add(ResLoadBundleRequest.Build(name, url));
            }

            if (list.Count == 0) return;

            // 调用接口
            LoadBundlesAsync(list, onComplete, onProgress);
        }

        /// <summary>
        /// 从App内部加载Bundles
        /// </summary>
        /// <param name="names"></param>
        /// <param name="onComplete"></param>
        /// <param name="onProgress"></param>
        public void LoadBundlesInternal(IList<string> names, Action onComplete, Action<float> onProgress = null)
        {
            List<ResLoadBundleRequest> list = new List<ResLoadBundleRequest>(names.Count);
            
            string name = "";
            string url = "";
            for (int i = 0; i < names.Count; i++)
            {
                name = names[i];
                
                url = BundleCachingPath(name);
                if (!File.Exists(url))
                {
                    url = BundleStreamingPath(name);
                    Debug.Log($">>> Load Bundle from Streaming: {url}");
                }
                else
                {
                    Debug.Log($">>> Load Bundle from Cache: {url}");
                }
                
#if UNITY_EDITOR || UNITY_IOS
                    url = $"file://{url}"; // iOS 和 Editor 需要添加 file 前缀路径
#endif
                
                list.Add(ResLoadBundleRequest.Build(name, url));
            }

            if (list.Count == 0) return;

            // 调用接口
            LoadBundlesAsync(list, onComplete, onProgress);
        }

        
        /// <summary>
        /// 异步加载单个Bundle
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <param name="autoCache"></param>
        public void LoadBundleAsync(string url, Action<AssetBundle, string> callback, bool autoCache = true)
        {
            _loader.LoadBundleAsync(url, (bundle, error) =>
            {
                if (bundle != null)
                {
                    Bundles[bundle.name] = bundle;
                }
                
                callback?.Invoke(bundle, error);
            }, autoCache);
        }

        /// <summary>
        /// 同步加载单个Bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="forceStreaming"></param>
        /// <returns></returns>
        public AssetBundle LoadBundle(string bundleName, bool forceStreaming = false) 
            => _loader.LoadBundle(bundleName, forceStreaming);
        

        #endregion
        
        

        #region 资源管理


        public AssetBundle GetBundle(string bundleName)
        {
            AssetBundle ab = null;
            if (Bundles.TryGetValue(bundleName, out ab))
            {
                Debug.Log($"Get cached bundle: {bundleName} : {ab}");   
            }
            else
            {
                Debug.Log($"Bundle: {bundleName} not in cache...");
            }
            return ab;
        }


        /// <summary>
        /// 释放Bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="unloadAll"></param>
        public void ReleaseBundle(string bundleName, bool unloadAll = true)
        {
            if (Bundles.TryGetValue(bundleName, out var ab))
            {
                Bundles.Remove(bundleName);
                ab.Unload(unloadAll);
            }
        }

        public void Dispose()
        {
            _loader.Dispose();
            _loader = null;

            foreach (var ab in Bundles.Values)
            {
                if(ab != null) ab.Unload(true);
            }
            
            Bundles.Clear();
            _bundles = null;
        }

        #endregion
        
        #region 资源引用
        
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="bundleName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LoadAsset<T>(string assetName, string bundleName = "") where T : Object
        {
            // Debug.Log($"<color=cyan> SimulationMode: {SimulationMode}  </color>");
            
            if (SimulationMode) return SimulationLoadAsset<T>(assetName, bundleName);
            
            if (string.IsNullOrEmpty(bundleName))
            {
                Debug.Log($"{Tag} ---- Bundle load from resources {bundleName}...");
                return _loader.Load<T>(assetName); // 资源加载
            }

            // var ab = GetBundle(bundleName);
            // bool isNew = false;
            // if (null == ab)
            // {
            //     Debug.Log($"{Tag} ---- #1 Bundle not in Cache, load new bundle {bundleName}...");
            //     ab = _loader.LoadBundle(bundleName);
            //     isNew = true;
            // }
            //
            // if (ab != null)
            // {
            //     Debug.Log($"{Tag} ---- #2 bundle loaded: {bundleName}...");
            //     if(isNew) Bundles[bundleName] = ab;
            //     return ab.LoadAsset<T>(assetName);
            // }
            
            var ab = GetBundle(bundleName);
            if (null != ab)
            {
                // Debug.Log($"{Tag} bundle is exists: {bundleName}...");
                return ab.LoadAsset<T>(assetName);
            }

            Debug.Log($"{Tag} bundle {bundleName} is not existed!");
            return null;
        }



        /// <summary>
        /// 克隆对象
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="bundleName"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GameObject CloneGameObject(string assetPath, string bundleName = "", Transform parent = null)
        {
            var prefab = LoadAsset<GameObject>(assetPath, bundleName);
            if (prefab != null) return GameObject.Instantiate(prefab, parent);
            return null;
        }


        /// <summary>
        /// 加载图集对象
        /// </summary>
        /// <param name="atlasName"></param>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public SpriteAtlas LoadAtlas(string atlasName, string bundleName = "")
        {
            return LoadAsset<SpriteAtlas>(atlasName, bundleName);
        }

        /// <summary>
        /// 加载包内文本对象
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public string LoadText(string assetPath, string bundleName = "")
        {
            var ta = LoadAsset<TextAsset>(assetPath, bundleName);
            return ta?.text ?? "";
        }

        /// <summary>
        /// 加载Json对象
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="bundleName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LoadJsonObject<T>(string assetPath, string bundleName = "")
        {
            var json = LoadText(assetPath, bundleName);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    return LitJson.JsonMapper.ToObject<T>(json);
                }
                catch (Exception e)
                {
                    Debug.Log($"[Res] load JSON object failed: {json}");
                    Debug.Log(e);
                }
            }
            return default(T);
        }

        #endregion

        #region IO管理

        /// <summary>
        /// 确保文件的父级目录一定存在
        /// </summary>
        /// <param name="filePath"></param>
        public static void EnsureDirectory(string filePath)
        {
            var dir = Directory.GetParent(filePath);
            if(!dir.Exists) dir.Create();
        }

        #endregion

        #region 编辑器功能
        
        /// <summary>
        /// 模拟加载
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="bundleName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T SimulationLoadAsset<T>(string assetName, string bundleName) where T : Object
            => _loader.SimulationLoadAsset<T>(assetName, bundleName);

        #endregion
    }

}