
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor;

namespace GameBox
{

    /// <summary>
    /// AB包体保护器
    /// </summary>
    public class GBundleEncrypter
    {

        private static readonly string secret = "enc_tester";
        
        private static void OpenPath(string path)
        {
            
#if UNITY_EDITOR_OSX
            EditorUtility.RevealInFinder(path);
#else
            Application.OpenURL($"file://{path}");
#endif
        }
        
        public static void EncryptAllBundlesInPath(string secret, string from, string to = "")
        {
            if (string.IsNullOrEmpty(to)) to = from; // 直接覆盖原始的文件
            var dirInfo = new DirectoryInfo(from);
            foreach (var f in dirInfo.GetFiles())
            {
                if (string.IsNullOrEmpty(f.Extension))
                {
                    Encrypter.EncryptBundle(f.FullName, secret, $"{to}/{f.Name}");
                }
            }
            OpenPath(to);
        }
        
        
        public static void DecryptAllBundlesInPath(string secret, string from, string to = "")
        {
            if (string.IsNullOrEmpty(to)) to = from; // 直接覆盖原始的文件
            var dirInfo = new DirectoryInfo(from);
            foreach (var f in dirInfo.GetFiles())
            {
                if (string.IsNullOrEmpty(f.Extension))
                {
                    Encrypter.DecryptBundle(f.FullName, secret, $"{to}/{f.Name}");
                }
            }
            
            OpenPath(to);
            
        }



        #region 单元测试


        [Test]
        public static void TEST__EncryptAllBundles()
        {
            string from = $"{Application.dataPath}/../AssetBundles/Android";
            string to = $"{Application.streamingAssetsPath}/AssetBundles/Android";

            Debug.Log($" in:{from} -> out:{to}");
            EncryptAllBundlesInPath(secret, from, to);
        }
        
        [Test]
        public static void TEST__DecryptAllBundles()
        {
            string from = $"{Application.streamingAssetsPath}/AssetBundles/Android";
            DecryptAllBundlesInPath(secret, from);
        }

        [Test]
        public static void TEST__LoadBundle()
        {
            string path = $"{Application.streamingAssetsPath}/AssetBundles/Android/demo";

            var bundle = Encrypter.LoadEncyptBundle(path, secret);
            if (bundle != null)
            {
                foreach (var asset in bundle.LoadAllAssets())
                {
                    Debug.Log($"--- {asset.name} ({asset.GetType()})");
                }
            }
            else
            {
                Debug.LogError($"--- Load Bundle Fail: {path}");
            }
        }
        

        #endregion
        

    }
}