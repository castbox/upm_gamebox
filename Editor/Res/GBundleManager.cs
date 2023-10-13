using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameBox
{
    public class GBundleManager
    {

        #region 通用方法

        private static void OpenPath(string path)
        {
            
#if UNITY_EDITOR_OSX
            EditorUtility.RevealInFinder(path);
#else
            Application.OpenURL($"file://{path}");
#endif
        }

        #endregion
        

        #region 正常构建所有包体

        
        [MenuItem("GameBox/Bundle/Build All Bundles")]
        public static void BuildAllBundles()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
            
            GAssetBundleAPI.BuildBundlesInTargetPath(GBundleConst.TargetResName, true);
        }


        private static string ResSimulateFlagPath => Path.GetFullPath($"{Application.dataPath}/../Library/res_simulate_mode");
        
        private static bool SimulationMode
        {
            get => File.Exists(ResSimulateFlagPath);
            set
            {
                if (value)
                {
                    File.WriteAllText(ResSimulateFlagPath, "");
                }
                else
                {
                    if (File.Exists(ResSimulateFlagPath))
                    {
                        File.Delete(ResSimulateFlagPath);
                    }
                }
                
                Debug.Log($"Set Bundle Simulation: {SimulationMode}");
            }
        }

        private const string TITLE_SIMULATION_ON = "GameBox/Bundle/Simulation ✅";
        private const string TITLE_SIMULATION_OFF = "GameBox/Bundle/Simulation ❎";

        [MenuItem(TITLE_SIMULATION_ON, true)]
        public static bool CheckSimulateModeOn() => SimulationMode == false;
        
        [MenuItem(TITLE_SIMULATION_OFF, true)]
        public static bool CheckSimulateModeOff() => SimulationMode == true;


        [MenuItem(TITLE_SIMULATION_ON)]
        public static void SetSimulateModeOn() => SimulationMode = true;
        
        [MenuItem(TITLE_SIMULATION_OFF)]
        public static void SetSimulateModeOff() => SimulationMode = false;
        
        #endregion


        #region 加密接口
        
        /// <summary>
        /// 加密所有BUndle
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void EncryptAllBundlesInPath(string secret, string from, string to = "", bool isOffset = true)
        {
            if (string.IsNullOrEmpty(to)) to = from; // 直接覆盖原始的文件
            var dirInfo = new DirectoryInfo(from);
            byte[] data = null;
            foreach (var f in dirInfo.GetFiles())
            {
                if (string.IsNullOrEmpty(f.Extension))
                {
                    if (isOffset)
                    {
                        Encrypter.EncryptOffset(f.FullName, secret, $"{to}/{f.Name}");
                    }
                    else
                    {
                        Encrypter.EncryptXOR(f.FullName, secret, $"{to}/{f.Name}");
                    }

                    
                }
            }
            OpenPath(to);
        }
        
        /// <summary>
        /// 还原所有Bundle
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void DecryptAllBundlesInPath(string secret, string from, string to = "", bool isOffset = true)
        {
            if (string.IsNullOrEmpty(to)) to = from; // 直接覆盖原始的文件
            var dirInfo = new DirectoryInfo(from);
            foreach (var f in dirInfo.GetFiles())
            {
                if (string.IsNullOrEmpty(f.Extension))
                {
                    if (isOffset)
                    {
                        Encrypter.DecryptOffset(f.FullName, secret, $"{to}/{f.Name}");
                    }
                    else
                    {
                        Encrypter.DecryptXOR(f.FullName, secret, $"{to}/{f.Name}");
                    }
                }
            }
            
            OpenPath(to);
            
        }
        
        #endregion

        #region 单元测试

        private static readonly string test_secret = "enc_test_key";
        
        [Test]
        public static void TEST__EncryptAllBundles()
        {
            string from = $"{Application.dataPath}/../AssetBundles/Android";
            string to = $"{Application.streamingAssetsPath}/assetbundles/android";

            Debug.Log($" in:{from} -> out:{to}");
            EncryptAllBundlesInPath(test_secret, from, to);
        }
        
        [Test]
        public static void TEST__DecryptAllBundles()
        {
            string from = $"{Application.streamingAssetsPath}/assetbundles/android";
            DecryptAllBundlesInPath(test_secret, from);
        }

        
        

        #endregion

    }



    /// <summary>
    /// 创建Bundle的API
    /// </summary>
    public class GAssetBundleAPI
    {
        public const string Version = "0.0.1";
        

        /// <summary>
        /// 获取资源路径
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string GetAssetPath(string fullPath)
        {
            return fullPath.Replace(Application.dataPath, "Assets");
        }
        
        private static void EnsurePathExist(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
        

        /// <summary>
        /// 查找所有的目标组
        /// </summary>
        /// <param name="targetDirName"></param>
        /// <returns></returns>
        public static GBundleManifest SearchAllTargetGroups(string targetDirName)
        {
            
            GBundleManifest manifest = new GBundleManifest(targetDirName);
            
            var root = new DirectoryInfo(Application.dataPath);
            var targets = root.GetDirectories(targetDirName, SearchOption.AllDirectories);

            if (targets.Length > 0)
            {
                foreach (var t in targets)
                {
                    List<RefBundleInfo> list = CreateBundleInfo(t);
                    if (list.Count > 0)
                    {
                        foreach (var info in list)
                        {
                            manifest.AddBundle(info);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"--- No Target {targetDirName} has been found" );
            }
            return manifest;
        }
        
        /// <summary>
        /// 创建BundleInfo
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static List<RefBundleInfo> CreateBundleInfo(DirectoryInfo info)
        {

            List<RefBundleInfo> list = new List<RefBundleInfo>();
            var dirs = info.GetDirectories("*", SearchOption.TopDirectoryOnly);
            foreach (var d in dirs)
            {
                var files = d.GetFiles("*", searchOption: SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    string bundleName = d.Name.ToLower();
                    RefBundleInfo bundle = RefBundleInfo.CreateFromDirInfo(bundleName, GetAssetPath(d.FullName));
                    
                    foreach (var file in files)
                    {
                        if (!file.Extension.Contains("meta"))
                        {
                            var resPath = GetAssetPath(file.FullName);
                            var a = AssetDatabase.LoadAssetAtPath<Object>(resPath);
                            if (a != null && AssetDatabase.IsMainAsset(a))
                            {
                                var item = RefAssetInfo.Create(a.name, bundleName, resPath);
                                bundle.assets.Add(item);
                            }
                        } 
                    }
                    list.Add(bundle);
                }       
            }

            return list;
        }
        
        /// <summary>
        /// 应用打包配置
        /// </summary>
        /// <param name="manifest"></param>
        private static void ApplyManifest(GBundleManifest manifest)
        {
            if (manifest.bundles.Count > 0)
            {
                foreach (var bundle in manifest.bundles)
                {
                    foreach (var asset in bundle.assets)
                    {
                        AssetImporter importer = AssetImporter.GetAtPath(asset.resPath);
                        importer.assetBundleName = bundle.name;
                    }
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 更新打包信息
        /// </summary>
        /// <param name="targetDirName"></param>
        public static void BuildBundlesInTargetPath(string targetDirName, bool isLocalBundle = false)
        {

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var gbm = SearchAllTargetGroups(targetDirName);
            gbm.build_target = buildTarget.ToString();
            gbm.target_name = targetDirName;
            gbm.version = Version;
            // manifest.SaveTo($"{Application.streamingAssetsPath}/manifest.json");
            
            ApplyManifest(gbm);
            
            var outputRoot = $"{Application.dataPath}/../assetbundles";
            var buildPath = $"{outputRoot}/{buildTarget.ToString()}";

            EnsurePathExist(buildPath);

            var options = BuildAssetBundleOptions.AssetBundleStripUnityVersion
                          | BuildAssetBundleOptions.ChunkBasedCompression;
            // | BuildAssetBundleOptions.EnableProtection;
            var bundleManifest =  BuildPipeline.BuildAssetBundles(buildPath, options, buildTarget);
            foreach (var bundle in gbm.bundles)
            {
                bundle.hash = bundleManifest.GetAssetBundleHash(bundle.name).ToString();
            }

            // 保存 build 配置
            gbm.SaveTo(GBundleConst.ManifestPath);
            
            // 如果是本地包
            if (isLocalBundle)
            {
                string localDir = $"{Application.streamingAssetsPath}/assetbundles/{buildTarget.ToString()}".ToLower();
                EnsurePathExist(localDir);
                string from, to;
                foreach (var bundle in gbm.bundles)
                {
                    from = $"{buildPath}/{bundle.name}";
                    to = $"{localDir}/{bundle.name}";
                    File.Copy(from, to, true);    
                }
                OpenPath(localDir);
                
            }
            else
            {
                OpenPath(buildPath);
            }
        }


        private static void OpenPath(string path)
        {
#if UNITY_EDITOR_OSX
            EditorUtility.RevealInFinder(path);
#else
            Application.OpenURL($"file://{path}");
#endif
        }
        

    }

    






}