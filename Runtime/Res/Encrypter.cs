using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GameBox
{
    public class Encrypter
    {
        /// <summary>
        /// 请妥善保存加密前缀
        /// </summary>
        private const string EncryptPrefix = "UnityFS    5.x.x 0.0.0      ���  �  9  C";

        public static byte[] GetSalt(string secret) => Encoding.UTF8.GetBytes(EncryptPrefix + secret);
        
        /// <summary>
        /// 加密Bundle
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <param name="secret"></param>
        /// <param name="outputPath"></param>
        public static void EncryptOffset(string bundlePath, string secret = "", string outputPath = "")
        {
            if (!File.Exists(bundlePath))
            {
                Debug.LogError($"bundle 不存在: {bundlePath}");
                return;
            }

            // 确保路径可用
            if (string.IsNullOrEmpty(outputPath)) outputPath = bundlePath;
            var dir = Directory.GetParent(outputPath);
            if (!dir.Exists) dir.Create();
            
            byte[] src = File.ReadAllBytes(bundlePath);
            byte[] salt =  GetSalt(secret); // 秘钥

            byte[] buffer = new byte[src.Length + salt.Length];
            Array.Copy(salt, buffer, salt.Length);
            Array.Copy(src, 0, buffer, salt.Length, src.Length);
            
            File.WriteAllBytes(outputPath, buffer);
            Debug.Log($"+ Bundle加密: {secret} -> {salt.Length}\n<color=#88ff00>{outputPath}</color>");
        }
        
        
        
        public static void EncryptXOR(string bundlePath, string secret = "", string outputPath = "")
        {
            if (!File.Exists(bundlePath))
            {
                Debug.LogError($"bundle 不存在: {bundlePath}");
                return;
            }

            // 确保路径可用
            if (string.IsNullOrEmpty(outputPath)) outputPath = bundlePath;
            var dir = Directory.GetParent(outputPath);
            if (!dir.Exists) dir.Create();
            
            byte[] src = File.ReadAllBytes(bundlePath);
            src = XOR(src, secret);
            
            File.WriteAllBytes(outputPath, src);
            Debug.Log($"+ Bundle加密: {bundlePath} -> <color=#88ff00>{outputPath}</color>");
        }


        public static byte[] XOR(byte[] source, byte[] salt)
        {
            int len = salt.Length;
            for (int i = 0; i < salt.Length; i++)
            {
                source[i] = (byte)(source[i] ^ salt[i % len]);
                // source[i] = (byte)(source[i] ^ 123);
            }

            return source;
        }


        public static byte[] XOR(byte[] source, string secret)
        {
            byte[] salt = GetSalt(secret); // 秘钥
            return XOR(source, salt);
        }


        /// <summary>
        /// 还原bundle
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <param name="secret"></param>
        /// <param name="outputPath"></param>
        public static void DecryptOffset(string bundlePath, string secret = "", string outputPath = "")
        {
            if (!File.Exists(bundlePath))
            {
                Debug.LogError($"bundle 不存在: {bundlePath}");
                return;
            }

            // 确保路径可用
            if (string.IsNullOrEmpty(outputPath)) outputPath = bundlePath;
            var dir = Directory.GetParent(outputPath);
            if (!dir.Exists) dir.Create();

            byte[] salt = GetSalt(secret); // 秘钥
            byte[] src = File.ReadAllBytes(bundlePath);
            var data = src.Skip(salt.Length).ToArray();
            File.WriteAllBytes(outputPath, data);
            Debug.Log($"- Bundle成功还原: {bundlePath} -> <color=#88ff00>{outputPath}</color>");
        }
        
        
        public static void DecryptXOR(string bundlePath, string secret = "", string outputPath = "")
        {
            if (!File.Exists(bundlePath))
            {
                Debug.LogError($"bundle 不存在: {bundlePath}");
                return;
            }

            // 确保路径可用
            if (string.IsNullOrEmpty(outputPath)) outputPath = bundlePath;
            var dir = Directory.GetParent(outputPath);
            if (!dir.Exists) dir.Create();

            byte[] src = File.ReadAllBytes(bundlePath);
            src = XOR(src, secret);
            
            File.WriteAllBytes(outputPath, src);
            Debug.Log($"- Bundle成功还原: {bundlePath} -> <color=#88ff00>{outputPath}</color>");
        }

        
        
        private static AssetBundle LoadBundleOffset(string bundlePath, string secret = "", uint crc = 0)
        {
            var salt = GetSalt(secret);
            Debug.Log($"--- Load Bundle: secret:{secret}  salt: {salt.Length}");
            return AssetBundle.LoadFromFile(bundlePath, crc, (ulong) salt.Length);
        }


        private static byte[] UnOffset(byte[] data, string secret)
        {
            var salt = GetSalt(secret);
            return UnOffset(data, salt);
        }
        
        private static byte[] UnOffset(byte[] data, byte[] salt)
        {
            return data.Skip(salt.Length).ToArray();
        }


        /// <summary>
        /// 解密加载Bundle
        /// </summary>
        /// <param name="data"></param>
        /// <param name="secret"></param>
        /// <param name="isOffset"></param>
        /// <returns></returns>
        public static AssetBundle DecryptBundle(byte[] data, string secret = "", bool isOffset = true)
        {
            try
            {
                var salt = GetSalt(secret);

                if (isOffset)
                {
                    data = UnOffset(data, salt);
                }
                else
                {
                    data = XOR(data, salt);
                }

                return AssetBundle.LoadFromMemory(data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;

        }

    }
    
    


}