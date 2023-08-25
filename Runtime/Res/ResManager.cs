using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public void SetBundleSecret(string secret) => _loader.BundleSecret = secret;


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
        public AssetBundle PreLoadBundle(string bundleName, string secret = "")
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
        public bool HasBundle(string bundleName)
        {
            return Bundles.ContainsKey(bundleName);
        }

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
                Debug.Log($"Can't load bundle: {bundleName}...");
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
        public T LoadAsset<T>(string assetName, string bundleName = "", string secret = "") where T : Object
        {
            // Debug.Log($"<color=cyan> SimulationMode: {SimulationMode}  </color>");
            
            if (SimulationMode) return SimulationLoadAsset<T>(assetName, bundleName);
            
            if (string.IsNullOrEmpty(bundleName))
            {
                return _loader.Load<T>(assetName); // 资源加载
            }

            var ab = GetBundle(bundleName);
            bool isNew = false;
            if (null == ab)
            {
                ab = _loader.LoadBundle(bundleName);
                isNew = true;
            }

            if (ab != null)
            {
                if(isNew) Bundles[bundleName] = ab;
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