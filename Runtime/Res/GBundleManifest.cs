using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameBox.LitJson;
using UnityEngine;

namespace GameBox
{

    #region 路径常量

    public static class GBundleConst
    {
        public const string MANIFEST_FILE_NAME = "manifest.gamebox.json";
        public static string ProjectSettingsPath => Path.GetFullPath($"{Application.dataPath}/../ProjectSettings");
        public static string ManifestPath => Path.GetFullPath($"{ProjectSettingsPath}/{MANIFEST_FILE_NAME}");
        public const string TargetResName = "ResourceG";
    }

    
    #endregion
    
    #region 构建数据

    /// <summary>
    /// Bundle 构建的 Manifest 文件
    /// </summary>
    [Serializable]
    public class GBundleManifest
    {
       

        private string _savePath;

        public string version = "0.0.1";
        public string target_name;
        public DateTime create_time;
        public List<RefBundleInfo> bundles;
        public string build_target;
        
        public GBundleManifest()
        {
        }

        public GBundleManifest(string target_name)
        {
            create_time = DateTime.Now.ToLocalTime();
            bundles = new List<RefBundleInfo>(10);
        }


        public void AddBundle(RefBundleInfo bundle)
        {
            var exists = bundles.FirstOrDefault(c => c.name == bundle.name);
            if (exists != null)
            {
                exists.assets.AddRange(bundle.assets);
            }
            else
            {
                bundles.Add(bundle);
            }
        }
        
        public RefBundleInfo GetBundleInfo(string bundleName) => bundles.FirstOrDefault(c => c.name == bundleName);

        public bool Save()
        {
            if (string.IsNullOrEmpty(_savePath)) return false;

            
            return true;
        }

        public void SaveTo(string path)
        {
            _savePath = path;
            EnsurePathDir(path);
            Save();
        }
        
        private void EnsurePathDir(string filePath)
        {
            var dir = Directory.GetParent(filePath);
            if(dir != null && !dir.Exists) dir.Create();
            File.WriteAllText(filePath, JsonMapper.ToJson(this));
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }


        public string GetBundleResPath(string bundleName)
        {
            var info = GetBundleInfo(bundleName);
            return info?.resPath ?? null;
        }

        public string GetAssetResPath(string asset, string bundleName)
        {
            var info = GetBundleInfo(bundleName);
            if (null != info)
            {
                var aInfo = info.GetAssetInfo(asset);
                return aInfo?.resPath ?? null;
            }

            return null;
        }
        
        
        public static GBundleManifest Load(string path = "")
        {
            if (string.IsNullOrEmpty(path)) path = GBundleConst.ManifestPath;
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonMapper.ToObject<GBundleManifest>(json);
            }
            return null;
        }

    }

    [Serializable]
    public class RefBundleInfo
    {
        public string name;
        public string hash;
        public string resPath;
        public List<RefAssetInfo> assets = new List<RefAssetInfo>();


        public RefBundleInfo()
        {
        }

        public static RefBundleInfo CreateFromDirInfo(string bundleName, string res)
        {
            RefBundleInfo info = new RefBundleInfo()
            {
                name = bundleName,
                resPath = res,
            };
            return info;
        }

        public RefAssetInfo GetAssetInfo(string asset) 
            => assets.FirstOrDefault(c => c.name == asset);
    }
    
    /// <summary>
    /// 引用资产信息
    /// </summary>
    [Serializable]
    public class RefAssetInfo
    {
        public string name;
        public string group;
        public string resPath;
        
        public static RefAssetInfo Create(string name, string group, string res)
        {
            RefAssetInfo info = new RefAssetInfo()
            {
                name = name,
                group = group,
                resPath = res,
            };
            return info;
        }
    }



    #endregion
}