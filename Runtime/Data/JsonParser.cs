

namespace GameBox
{
    using System;
    using GameBox.LitJson;
    
    /// <summary>
    /// JSON 格式转化器
    /// </summary>
    public class JsonParser
    {
        
        public static bool IsValidJson(string json)
        {
            char[] trims = new char[]{' ', '\n'};
            string str = json.TrimStart(trims).TrimEnd(trims);
            if( str.StartsWith("{") && str.EndsWith("}")) return true;
            if( str.StartsWith("[") && str.EndsWith("]")) return true;
            
            Log($"Json string is not vaild: {json}");
            return false;
        }

        public static T Parse<T>(string json)
        {
            if(!IsValidJson(json)) return default(T);
            
            try
            {
                return JsonMapper.ToObject<T>(json);
            }
            catch (Exception e)
            {
                Log("Parse error: " + e.Message);
            }
            
            return default(T);
        }


        /// <summary>
        /// 转化为JSON格式
        /// </summary>
        /// <param name="obj">序列化对象</param>
        /// <param name="pretty">应用格式</param>
        /// <returns></returns>
        public static string ToJson(object obj, bool pretty = false)
        {
            if (pretty)
            {
                var writer = new JsonWriter() { PrettyPrint = true };
                JsonMapper.ToJson(obj, writer);
                return writer.TextWriter.ToString();
            }
            return JsonMapper.ToJson(obj);
        }
        
        /// <summary>
        /// 打印信息
        /// </summary>
        /// <param name="msg"></param>
        private static void Log(string msg)
        {
            Console.WriteLine(msg);
        }

    }
}