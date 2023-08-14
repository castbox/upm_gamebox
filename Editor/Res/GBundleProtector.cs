
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;

namespace GameBox
{
    
    /// <summary>
    /// AB包体保护器
    /// </summary>
    public class GBundleProtector
    {

        /// <summary>
        /// 请妥善保存加密秘钥, 前
        /// </summary>
        private const string EncryptKey = "UnityFS    5.x.x 0.0.0      ���  �  /  C      \n$MicroSoft 1980 228 5.x.x 0.0.0\n$#9C 8 \nGuru INC COPY RIGHT9C\n  \n /#9c0\n 9C \n� 6\n 8 $\n BC#�9�\nK K�\nUnityFS    3.x.x UnityFS    4.x.x ";
        
        
        /// <summary>
        /// 加密Bundle
        /// </summary>
        /// <param name="bundlePath"></param>
        public static void EncryptBundle(string bundlePath)
        {
            if (File.Exists(bundlePath))
            {
                byte[] data = File.ReadAllBytes(bundlePath);
                byte[] encHeader = Encoding.UTF8.GetBytes(EncryptKey);
                List<byte> st = new List<byte>(encHeader);
                st.AddRange(data);
                UnityEngine.Windows.File.WriteAllBytes(bundlePath, st.ToArray());
                st = null;
                Debug.Log($"--- 成功加密: {bundlePath}");
            }
        }


        public static void DecryptBundle(string bundlePath)
        {
            if (File.Exists(bundlePath))
            {
                byte[] encHeader = Encoding.UTF8.GetBytes(EncryptKey);
                byte[] raw = File.ReadAllBytes(bundlePath);
                int offset = encHeader.Length;
                int count = raw.Length - encHeader.Length;
                var data = raw.Skip(offset).ToArray();
                File.WriteAllBytes(bundlePath, data);
                Debug.Log($"--- 成功解密: {bundlePath}");
            }
        }



        /// <summary>
        /// 解密bundle
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <param name="crc"></param>
        /// <returns></returns>
        public static AssetBundle LoadBundle(string bundlePath, uint crc = 0)
        {
            byte[] encHeader = Encoding.UTF8.GetBytes(EncryptKey);
            return AssetBundle.LoadFromFile(bundlePath, crc, (ulong)encHeader.LongLength);
        }



        #region 单元测试

        [MenuItem("Test/Bundle/加密Bundle")]
        private static void EditorEncryptBundle()
        {
            var bundleName = "demo";
            var path = $"{Application.streamingAssetsPath}/AssetBundles/Android/{bundleName}";

            EncryptBundle(path);
        }
        
        [MenuItem("Test/Bundle/解密Bundle")]
        private static void EditorDencryptBundle()
        {
            var bundleName = "demo";
            var path = $"{Application.streamingAssetsPath}/AssetBundles/Android/{bundleName}";

            DecryptBundle(path);
        }
        
        [MenuItem("Test/Bundle/加载Bundle")]
        private static void EditorLoadEncryptBundle()
        {
            var bundleName = "demo";
            var path = $"{Application.streamingAssetsPath}/AssetBundles/Android/{bundleName}";
            
            Debug.Log($">>>#1 Test Load Bundle directly");
            try
            {
                var ab = AssetBundle.LoadFromFile(path);
                TestReadBundle(ab);
            }
            catch (Exception e)
            {
                Debug.Log($"<color=orange>---- {e} ----</color>");
            }

            
            Debug.Log($">>>#2 Test Load Bundle by dencrypt");
            try
            {
                AssetBundle bundle = LoadBundle(path);
                TestReadBundle(bundle);
            }
            catch (Exception e)
            {
                Debug.Log($"<color=orange>---- {e} ----</color>");
            }
        }


        private static void TestReadBundle(AssetBundle bundle)
        {
            if (bundle != null)
            {
                var objs = bundle.LoadAllAssets();
                foreach (var o in objs)
                {
                    Debug.Log($"-- {o.name} : {o.GetType().Name}");
                }
                bundle.Unload(true);
                bundle = null;
            }
        }


        #endregion
        

    }
}