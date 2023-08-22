using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameBox
{
    public class Encrypter
    {
        /// <summary>
        /// 请妥善保存加密前缀
        /// </summary>
        private const string EncryptPrefix = "UnityFS    5.x.x 1.0.0      ���";

        public static string GetSalt(string secret) => EncryptPrefix + secret;
        
        /// <summary>
        /// 加密Bundle
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <param name="secret"></param>
        /// <param name="outputPath"></param>
        public static void EncryptBundle(string bundlePath, string secret = "", string outputPath = "")
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


            byte[] salt = Encoding.UTF8.GetBytes(GetSalt(secret)); // 秘钥
            byte[] src = File.ReadAllBytes(bundlePath);

            byte[] buffer = new byte[src.Length + salt.Length];
            Array.Copy(salt, buffer, salt.Length); // 考入头部
            Array.Copy(src, 0, buffer, salt.Length, src.Length); // 考入数据

            File.WriteAllBytes(outputPath, buffer);
            Debug.Log($"+ Bundle成功加密: {bundlePath} -> <color=#88ff00>{outputPath}</color>");
        }

        /// <summary>
        /// 还原bundle
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <param name="secret"></param>
        /// <param name="outputPath"></param>
        public static void DecryptBundle(string bundlePath, string secret = "", string outputPath = "")
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

            byte[] salt = Encoding.UTF8.GetBytes(GetSalt(secret)); // 秘钥
            byte[] src = File.ReadAllBytes(bundlePath);
            var data = src.Skip(salt.Length).ToArray();
            File.WriteAllBytes(outputPath, data);
            Debug.Log($"- Bundle成功还原: {bundlePath} -> <color=#88ff00>{outputPath}</color>");
        }



        /// <summary>
        /// 加载加密的Bundle
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <param name="crc"></param>
        /// <returns></returns>
        public static AssetBundle LoadEncyptBundle(string bundlePath, string secret = "", uint crc = 0)
        {
            if (!File.Exists(bundlePath))
            {
                Debug.LogError($"bundle 不存在: {bundlePath}");
                return null;
            }

            byte[] salt = Encoding.UTF8.GetBytes(GetSalt(secret));
            return AssetBundle.LoadFromFile(bundlePath, crc, (ulong)salt.Length);
        }
    }
}