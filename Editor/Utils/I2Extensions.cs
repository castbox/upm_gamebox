using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using I2.Loc;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameBox
{

    // public static class I2Extensions
    // {
    //     
    // }

    

    /// <summary>
    /// I2 语言扩展功能
    /// </summary>
    public class GuruI2EditorAPI
    {
        private const char DEFAULT_SEPARATOR = ',';
        private const char DEFAULT_LINE_CHANGE = '\n';
        private const string GURU_LOCALIZATION_FILENAME = "guru_localization.csv";

        private static LanguageSourceAsset _asset;

        private static LanguageSourceData GetSourceData()
        {
            _asset = Resources.Load<LanguageSourceAsset>("I2Languages");
            return _asset?.mSource ?? null;
        }

        /// <summary>
        /// Guru 翻译文件的路径
        /// </summary>
        /// <returns></returns>
        public static string GuruLocalizationFilePath()
        {
            return Path.GetFullPath($"{Application.dataPath}/../{GURU_LOCALIZATION_FILENAME}");
        }


        #region 文件导出
        
        
        /// <summary>
        /// 导出为 CSV 字符串
        /// </summary>
        /// <returns></returns>
        public static string ExportGuruCSVString()
        {
            var source = GetSourceData();
            if (null != source)
            {
                
                var csv = source.Export_CSV(null, DEFAULT_SEPARATOR);
                
                var lines = csv.Split(DEFAULT_LINE_CHANGE).ToList();
                var names = lines[0]
                    .Replace($"Key{DEFAULT_SEPARATOR}Type{DEFAULT_SEPARATOR}Desc{DEFAULT_SEPARATOR}", "")
                    .Split(DEFAULT_SEPARATOR);

                // 获取语言Code值
                List<string> codes = new List<string>(40); 
                foreach (var name in names)
                {
                    var code = GetLanguageCode(source, name);
                    codes.Add(code.Replace("-", "_")); // 转化为 "_" 的形式
                }
                
                string header = $"Code{DEFAULT_SEPARATOR}--{DEFAULT_SEPARATOR}--{DEFAULT_SEPARATOR}";
                header = $"{header}{string.Join(DEFAULT_SEPARATOR.ToString(), codes)}";
                lines.Insert(1, header);
                
                return string.Join(DEFAULT_LINE_CHANGE.ToString(), lines);
            }

            return "";
        }

        /// <summary>
        /// 获取语言Code
        /// </summary>
        /// <param name="data"></param>
        /// <param name="lanName"></param>
        /// <returns></returns>
        private static string GetLanguageCode(LanguageSourceData data, string lanName)
        {
            var ld = data.mLanguages.FirstOrDefault(c => c.Name == lanName);
            if (ld != null) return ld.Code;
            return "";
        }

        #endregion


        #region 文件导入


        /// <summary>
        /// 导入CSV文件
        /// </summary>
        /// <param name="filePath"></param>
        private static bool ImportGuruCsvFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                return ImportGuruCSVString(File.ReadAllText(filePath));
            }
            return false;
        }



        public static bool ImportGuruCSVString(string csv)
        {
            if (string.IsNullOrEmpty(csv))
            {
                return false;
            }

            var lines = csv.Split(DEFAULT_LINE_CHANGE).ToList();
            if (!lines[0].StartsWith("Key"))
            {
                return false;
            }

            if (lines[1].StartsWith("Code"))
            {
                lines.RemoveAt(1);
            }
            
            var source = GetSourceData();
            if (source != null)
            {
                source.Import_CSV("", 
                    string.Join(DEFAULT_LINE_CHANGE.ToString(), lines), 
                    eSpreadsheetUpdateMode.Replace,
                    DEFAULT_SEPARATOR);
                AssetDatabase.Refresh();
                
                return true;
            }
            
            return false;
        }

        #endregion
        

        #region 系统菜单
    
        [MenuItem("Guru/I2 Localization/导出 [CSV]")]
        private static void EditorExportToFile()
        {
            var csv = ExportGuruCSVString();
            if (string.IsNullOrEmpty(csv))
            {
                EditorUtility.DisplayDialog("导出Guru多语言配置", "I2 Guru定制CSV文件导出失败...", "OK");
                return;
            }

            // Debug.Log(csv);

            string path = GuruLocalizationFilePath();
            File.WriteAllText(path, csv);
#if UNITY_EDITOR_OSX
            EditorUtility.RevealInFinder(path);
#endif
        }
        
        [MenuItem("Guru/I2 Localization/导入 [CSV]")]
        private static void EditortImportFromFile()
        {
            string path = GuruLocalizationFilePath();
            if (!File.Exists(path))
            {
                path = EditorUtility.OpenFilePanelWithFilters("导入Guru多语言配置", $"{Application.dataPath}/../",
                    new string[] { "csv,txt" });
            }

            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("导入Guru多语言配置", $"{path}\n选定文件不存在", "OK");
                return;
            }

            
            bool res = ImportGuruCsvFromFile(path);
            if (res)
            {
                EditorUtility.DisplayDialog("导入Guru多语言配置", $"配置导入成功", "OK");
                if (null != _asset) Selection.activeObject = _asset;
            }
            else
            {
                
                EditorUtility.DisplayDialog("导入Guru多语言配置", $"配置导入失败", "OK");
            }
        }
        #endregion
        

    }
    
    
    

    
    
}