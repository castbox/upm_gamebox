using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameBox
{
    /// <summary>
    /// UIBinder 功能扩展
    /// </summary>
    [CustomEditor(typeof(UIBinder))]
    public class UIBinderInspector: UnityEditor.Editor
    {
        public enum ImportScope
        {
            Protected,
            Private,
            Public,
        }
        
        
        private UIBinder _binder;
        private Object _bindTarget = null;
        private bool _isDirty;
        private bool _badAction;
        private ImportScope _scope;
        
        private void OnEnable()
        {
            _isDirty = false;
            
            _binder = (UIBinder)target;
            
            if (!string.IsNullOrEmpty(_binder.bindScriptPath))
            {
                _bindTarget = AssetDatabase.LoadAssetAtPath<TextAsset>(_binder.bindScriptPath);
            }
        }

        private void OpenBindScriptPath()
        {
            string openPath = $"file://{Path.GetFullPath(_binder.bindScriptPath)}";
#if UNITY_EDITOR_WIN
            openPath =  openPath.Replace("/", "\\");
#endif
            Debug.Log($"open script: \n<color=cyan>{openPath}</color>");
            Application.OpenURL(openPath);
        }
        private void SyncAssets()
        {
            if (_isDirty)
            {
                _isDirty = false;
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        #region GUI

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(10);
            GUI_BindScriptField();
        }
        
        void GUI_BindScriptField()
        {
            var c = GUI.color;
            GUILayout.BeginHorizontal("box");
            if (string.IsNullOrEmpty(_binder.bindScriptPath))
            {
                GUI.color = Color.yellow;
                EditorGUILayout.SelectableLabel($"尚未绑定对应的脚本");
                GUI.color = c;
            }
            else
            {
                
                GUI.color = Color.green;
                GUILayout.Label("脚本已绑定");
                if (GUILayout.Button("打开", GUILayout.Width(50)))
                {
                    OpenBindScriptPath();
                }
                GUI.color = Color.red;
                if (GUILayout.Button("清空", GUILayout.Width(50)))
                {
                    _binder.bindScriptPath = "";
                    _bindTarget = null;
                    _isDirty = true;
                }
                // EditorGUILayout.TextField(_binder.bindScriptPath);
            }
            GUILayout.EndHorizontal();
            GUI.color = c;
            
            GUILayout.Space(4);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("拖入绑定脚本:");
            _bindTarget = EditorGUILayout.ObjectField( _bindTarget, typeof(TextAsset), false);
            GUILayout.Label("作用域:");
            GUI.color = Color.yellow;
            _scope = (ImportScope) EditorGUILayout.EnumPopup(_scope);
            GUI.color = c;
            GUILayout.EndHorizontal();

            if (GUILayout.Button("注入"))
            {
                string path = AssetDatabase.GetAssetPath(_bindTarget);
                Debug.Log($"脚本路径: <color=orange>{path}</color>");
                _binder.bindScriptPath = path;
                InjectBinderData();
            }
            
            
            
            SyncAssets();
        }

        #endregion

        #region 脚本注入

        private void InjectBinderData()
        {
            if(string.IsNullOrEmpty(_binder.bindScriptPath))
            {
                EditorUtility.DisplayDialog("脚本绑定缺失", $"无法找到绑定到{_binder.name}上的执行脚本, 请重新绑定", "OK");
                return;
            }

            string scriptPath = Path.GetFullPath(_binder.bindScriptPath);
            BinderDataInjectior.Run(scriptPath, _binder, _scope.ToString().ToLower());
            OpenBindScriptPath();
        }
        

        #endregion
        
    }

    
    class BinderDataInjectior
    {
        private const string K_SPLIT_FORMAT = "//********** {0} **********//";
        private const string K_UI_DEFINE_HEAD = "[CODE GEN] PROPERTY DEFINE HEAD";
        private const string K_UI_DEFINE_END = "[CODE GEN] PROPERTY DEFINE END";
        private const string K_UI_STAGEMENT_HEAD = "[CODE GEN] PROPERTY STATEMENT HEAD";
        private const string K_UI_STAGEMENT_END = "[CODE GEN] PROPERTY STATEMENT END";
        private const string K_ONCREATE_HEAD = "void OnCreateOver()";
        private const string K_USING_UNITY_UI = "UnityEngine.UI";
        private const string K_USING_GAMEBOX = "GameBox";
        private const string K_USING_UNITY = "UnityEngine";
        private const string K_USING_TMP = "TMPro";
        private const string K_MANUAL_FIX_WARNING = "//TODO: 合并代码错误, 请手动删除以下属性, 下次操作请勿删除 [CODE GEN] 相关标签!!";
        
        private static string C_DEFINE_HEAD = $"\t\t{string.Format(K_SPLIT_FORMAT, K_UI_DEFINE_HEAD)}";
        private static string C_DEFINE_END = $"\t\t{string.Format(K_SPLIT_FORMAT, K_UI_DEFINE_END)}";
        private static string C_STAGEMENT_HEAD = $"\t\t\t{string.Format(K_SPLIT_FORMAT, K_UI_STAGEMENT_HEAD)}";
        private static string C_STAGEMENT_END = $"\t\t\t{string.Format(K_SPLIT_FORMAT, K_UI_STAGEMENT_END)}";
        private static string C_ON_CREATE_LINE = "\t\tprotected override void OnCreateOver()";
        private static string C_CALL_BASE_ON_CREATE= "\t\t\tbase.OnCreateOver(); // 一定要调用基类的方法, 会有一些基础组件的初始化";

        private string _filePath;
        private List<string> _lines;
        private bool _badAction = false;
        private string _scopeName = "";

        private string[] requireUsingHeads = new[]
        {
            K_USING_UNITY,
            K_USING_UNITY_UI,
            K_USING_GAMEBOX,
            K_USING_TMP,
        };
        
        /// <summary>
        /// 注入属性
        /// </summary>
        /// <param name="scriptPath"></param>
        /// <param name="binder"></param>
        public static void Run(string scriptPath, UIBinder binder, string scope)
        {
            BinderDataInjectior runner = new BinderDataInjectior();
            runner.Read(scriptPath)
                .SetVariables(binder.variables, scope)
                .Save();
        }

        public BinderDataInjectior Read(string path)
        {
            _filePath = path;
            _lines = File.ReadLines(path).ToList();
            return this;
        }
        
        public BinderDataInjectior Save()
        {
            if (!_badAction)
            {
                File.WriteAllLines(_filePath, _lines.ToArray());
            }
            return this;
        }
        
        public BinderDataInjectior SetVariables(VariableArray variables, string scope)
        {
            _badAction = false;
            _scopeName = scope;
            if (string.IsNullOrEmpty(_scopeName)) _scopeName = "private";
            
            // 声明区
            int defineHead = -1;
            int defineEnd = -1;
            // 赋值区
            int stateHead = -1;
            int stateEnd = -1;
            // 类定义
            int classDefine = -1;
            // 方法行
            int onCreateCall = -1;
            List<string> usings = new List<string>();


            string line;
            for (int i = 0; i < _lines.Count; i++)
            {
                line = _lines[i];
                if (line.Contains("using "))
                {
                    usings.Add(line.Replace("using","").Replace(" ", "").Replace(";", ""));
                }
                else if (line.Contains("class") && line.Contains(":"))
                {
                    classDefine = i;
                }
                else if (line.Contains(K_UI_DEFINE_HEAD))
                {
                    defineHead = i;
                }
                else if(line.Contains(K_UI_DEFINE_END))
                {
                    defineEnd = i;
                }
                else if (line.Contains(K_UI_STAGEMENT_HEAD))
                {
                    stateHead = i;
                }
                else if (line.Contains(K_UI_STAGEMENT_END))
                {
                    stateEnd = i;
                }
                else if (line.Contains(K_ONCREATE_HEAD))
                {
                    onCreateCall = i;
                }
            }

            if (classDefine < 0)
            {
                if(EditorUtility.DisplayDialog("文件校验出错", "类声明校验报错! 请检查该类是否为 public 声明", "好的, 我知道了"))
                {
                    Debug.LogError($"找不到类定义! 请检查该类是否为 public 声明");
                    _badAction = true;
                }
                return this;
            }

            if (classDefine > -1)
            {
                if (_lines[classDefine].Contains("{"))
                {
                    defineHead = classDefine +1;
                }
                else
                {
                    int j = classDefine + 1;
                    while (j < _lines.Count)
                    {
                        if (_lines[j].Contains("{"))
                        {
                            classDefine = j +1;
                            break;
                        }
                        j++;
                    }
                }
            }

            
            // 先注入Statement
            List<string> states = new List<string>();
            List<string> defines = new List<string>();
            string typeName = "";
            foreach (var v in   variables.Variables)
            {
                Debug.Log($"value : Type{v.VariableType}  TypeName: {v.ValueType.ToString()}   Type.Name: {v.ValueType.Name}");
                if (!string.IsNullOrEmpty(v.Name))
                {
                    if (v.VariableType == VariableType.Component)
                    {
                        states.Add(GetComponentStatementLine(v));
                    }
                    else
                    {
                        states.Add(GetObjectStatementLine(v));
                    }
                    
                    defines.Add(GetDefineLine(v, _scopeName));
                }
            }
            states.Insert(0, C_CALL_BASE_ON_CREATE);
            states.Insert(0, C_STAGEMENT_HEAD);
            states.Add(C_STAGEMENT_END);
            defines.Insert(0, C_DEFINE_HEAD);
            defines.Add(C_DEFINE_END);

            
            // #1. 向OnCreate方法内注入属性定义
            if (stateHead > -1) _lines.RemoveAt(stateHead);
            if (stateEnd > -1) _lines.RemoveAt(stateEnd-1);
        
            if (onCreateCall < 0)
            {
                states.Insert(0, C_ON_CREATE_LINE);
                states.Insert(1, "\t\t{");
                states.Add("\t\t}");
                _lines.InsertRange(classDefine, states);
            }
            else
            {
                // 修正指针方向
                if (_lines[onCreateCall].Contains("{"))
                {
                    onCreateCall = onCreateCall+1;
                }
                else
                {
                    int j = onCreateCall + 1;
                    while (j < _lines.Count)
                    {
                        if (_lines[j].Contains("{"))
                        {
                            onCreateCall = j + 1;
                            break;
                        }
                        j++;
                    }
                }
                
                int count = 0;
                int start = onCreateCall;
                for (int i = start; i < _lines.Count; i++)
                {
                    if (_lines[i].Contains("}"))
                    {
                        count = i - start ;
                        break;
                    }
                }

                if (count > 0)
                {
                    _lines.RemoveRange(onCreateCall, count);
                }
                else
                {
                    if(EditorUtility.DisplayDialog("插入代码报错[2]", "[2] 属性定义标签只有开头, 无法进行去重操作, 请手动删除重复行!", "好的, 我知道了", "不, 取消注入"))
                    {
                        _lines.Insert(start, K_MANUAL_FIX_WARNING);
                        Debug.LogError($"插入代码错误, 请手动删除重复行! At line: {start}");
                    }
                    else
                    {
                        _badAction = false;
                    }
                }
                _lines.InsertRange(onCreateCall, states);
            }

            
            // #2. 向Class定义的下一行注入属性声明
            if (defineHead > -1 && defineEnd > -1 && defineEnd > defineHead)
            {
                _lines.RemoveRange(defineHead, defineEnd - defineHead + 1);
            }
            else if (defineHead > -1)
            {
                _lines.RemoveAt(defineHead);
                Debug.Log($"<color=orange>属性声明标签只有开头, 无法进行去重操作, 请手动删除重复行</color>");
                if(EditorUtility.DisplayDialog("插入代码报错[1]", "[1] 属性声明标签只有开头, 无法进行去重操作, 请手动删除重复行!", "好的, 我知道了", "不, 取消注入"))
                {
                    _lines.Insert(defineHead, K_MANUAL_FIX_WARNING);
                    Debug.LogError($"插入代码错误, 请手动删除重复行! At line: {defineHead}");
                }
                else
                {
                    _badAction = false;
                }
            }
            
            _lines.InsertRange(classDefine, defines);
            
            // #3. 整理Using
            foreach (var u in requireUsingHeads)
            {
                if (!usings.Contains(u))
                {
                    _lines.Insert(0, $"using {u};");
                }
            }
            
            return this;
        }

        private string ToDefineName(string assetName)
        {
            string buff = "";
            string[] words = assetName.Split('_');
            for (int i = 0; i < words.Length; i++)
            {
                var s = words[i];
                if (i == 0)
                {
                    buff += s.ToLower();
                }
                else
                {
                    buff += s.Substring(0, 1).ToUpper() + s.Substring(1);
                }
            }
            return buff;
        }

        private string GetDefineLine(Variable variable, string scope = "protected" )
        {
            return $"\t\t{scope} {GetTypeName(variable)} {ToDefineName(variable.Name)};";
        }

        private string GetComponentStatementLine(Variable variable)
        {
            return $"\t\t\t{ToDefineName(variable.Name)} = FindUI<{GetTypeName(variable)}>(\"{variable.Name}\");";
        }
        
        private string GetObjectStatementLine(Variable variable, string typeName = "")
        {
            return $"\t\t\t{ToDefineName(variable.Name)} = FindObj<{GetTypeName(variable)}>(\"{variable.Name}\");";
        }
        
        /// <summary>
        /// 获取类型名称
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        private string GetTypeName(Variable variable)
        {
            switch (variable.VariableType)
            {
                case VariableType.Integer:  return "int";
                case VariableType.String: return "string";
                case VariableType.Float: return  "float";
                case VariableType.Boolean: return "bool";
                default: return variable.ValueType.Name;
            }
        }
        
    }






}