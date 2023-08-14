using System;
using System.IO;
using UnityEngine;

namespace GameBox
{
    public partial class GDataIO
    {

        private static readonly string Tag = "[IO]";
        private static readonly string DefaultSaveName = "save_data.json";
        private static readonly string EditorSaveRoot = ".simulation";
        
        /// <summary>
        /// 创建一个IO类
        /// </summary>
        /// <param name="saveName"></param>
        /// <returns></returns>
        public static GDataIO Create(string saveName)
        {
            var io = new GDataIO(saveName);
            return io;
        }



        private string _saveName;
        private float _saveInterval = 2;  // 写保护间隔, 防止频繁读写硬盘

        public float SaveInterval
        {
            get => _saveInterval;
            set => _saveInterval = value;
        }


        public string SaveDir
        {
            get
            {
                var dir = Application.persistentDataPath;
#if UNITY_EDITOR
                dir =  Path.GetFullPath($"{Application.dataPath}/../{EditorSaveRoot}");
#endif
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                return dir;
            }
        }

        /// <summary>
        /// 存档路径
        /// </summary>
        public string SavePath 
        {
            get
            {
                if (string.IsNullOrEmpty(_saveName))
                {
                    _saveName = DefaultSaveName;
                }

                return $"{SaveDir}/{_saveName}";
            }

            set => _saveName = value;
        }

        public GDataIO()
        {

        }

        public GDataIO(string saveName)
        {
            _saveName = saveName;
        }

        /// <summary>
        /// 加载文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Load<T>()
        {
            if (string.IsNullOrEmpty(_saveName))
            {
                Debug.Log($"{Tag} saveName is null");
                return default(T);
            }

            if (!File.Exists(SavePath))
            {
                Debug.Log($"{Tag} file is not exists: {SavePath}");
                return default(T);
            }

            string raw = File.ReadAllText(SavePath);
            if (!string.IsNullOrEmpty(raw)) return JsonParser.Parse<T>(raw);
            return default(T);
        }

        public T LoadOrCreate<T>()
        {
            T data = Load<T>();
            if (data == null) data = Activator.CreateInstance<T>();
            return data;
        }
        
        private DateTime _lastSavedTime = new DateTime(1970, 1, 1);


        private void EnsureDirectory(string filePath)
        {
            var dir = Directory.GetParent(filePath);
            if(!dir.Exists) dir.Create();
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isForce"></param>
        public void Save(object data, bool isForce = false)
        {
            if (string.IsNullOrEmpty(_saveName)) return;

            EnsureDirectory(SavePath);
            
            if (!isForce)
            {
                var span = DateTime.Now - _lastSavedTime;
                if (span.TotalSeconds < SaveInterval) return;
            }
            
            _lastSavedTime = DateTime.Now;
            string json = JsonParser.ToJson(data);
            File.WriteAllText(SavePath, json);
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        public void Delete()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
        }

    }




}