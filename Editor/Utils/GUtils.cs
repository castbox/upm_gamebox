
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameBox
{
    public static class GUtils
    {
        
        #region Value Extends

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }
        
        public static bool IsNotNullOrEmpty(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        #endregion

        #region Editor Utils

        /// <summary>
        /// 从某个场景开始运行游戏
        /// </summary>
        /// <param name="scenePath"></param>
        public static void RunGameFrom(string scenePath)
        {
            SceneAsset launcher = AssetDatabase.LoadAssetAtPath<SceneAsset>($"Assets/{scenePath}.unity");
            // SceneAsset curScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorSceneManager.GetActiveScene().path);
            
            if (launcher != null)
            {
                SetLaunchScene(launcher);

                System.Action<PlayModeStateChange> handler = null;
                handler = (mode) =>
                {
                    EditorApplication.playModeStateChanged -= handler;
                    if (mode == PlayModeStateChange.ExitingPlayMode)
                    {
                        EditorSceneManager.playModeStartScene = null;
                    }
                };
                EditorApplication.playModeStateChanged += handler;
                EditorApplication.isPlaying = true;

            }
           
        }

        public static void SetLaunchScene(SceneAsset scene = null)
        {
            EditorSceneManager.playModeStartScene = scene;
        }
        

        #endregion

        #region Files

        public static bool CopyFile(string from, string to)
        {
            if (from.IsNullOrEmpty() || to.IsNullOrEmpty()) return false;
            if(File.Exists(to)) File.Delete(to);
            string dir = Directory.GetParent(to)?.FullName;
            if (dir.IsNotNullOrEmpty() && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (File.Exists(from))
            {
                File.Copy(from , to);
                // Debug.Log($"<color=#88ff00>Copy {from} to </color>\n{to}");
                return true;
            }
            else
            {
                Debug.Log($"<color=#ff8800>File not exists: {from}</color>");
            }

            return false;
        }
        
        //文件夹copy
        public static void CopyDirectory(string srcDir, string tgtDir, string pattern = "*")
        {
            DirectoryInfo source = new DirectoryInfo(srcDir);
            DirectoryInfo target = new DirectoryInfo(tgtDir);

            if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("父目录不能拷贝到子目录！");
            }

            if (!source.Exists)
            {
                return;
            }

            if (!target.Exists)
            {
                target.Create();
            }
            else
            {
                CleanDirectory(tgtDir);
            }

            FileInfo[] files = source.GetFiles(pattern);
            DirectoryInfo[] dirs = source.GetDirectories();
            if(files.Length==0 && dirs.Length==0)
            {
                Debug.Log("当前项目中文件夹为空");
                return;
            }
            for (int i = 0; i < files.Length; i++)
            {
                File.Copy(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
            }
            for (int j = 0; j < dirs.Length; j++)
            {
                CopyDirectory(dirs[j].FullName, Path.Combine(target.FullName, dirs[j].Name));
            }
        }

        //删除目标文件夹下面所有文件
        public static void CleanDirectory( string dir)
        {
            foreach (string subFile in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
            {
                File.Delete(subFile);
            }
            
            foreach (string subdir in Directory.GetDirectories(dir, "*", SearchOption.AllDirectories))
            {
                Directory.Delete(subdir, true);
            }
        }

        public static void Open(string path)
        {
            Application.OpenURL($"file://{path}");
        }

        #endregion
        
        
        
        
    }
}