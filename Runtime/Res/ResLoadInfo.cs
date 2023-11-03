using System;

namespace GameBox
{
    
    
    
    /// <summary>
    /// 加载Bundle请求
    /// </summary>
    [Serializable]
    public class ResLoadBundleRequest
    {

        public string name;
        public string url;
        public bool autoCache = true;

        public static ResLoadBundleRequest Build(string bundleName, string url, bool autoCache = true)
        {
            return new ResLoadBundleRequest()
            {
                name = bundleName,
                url = url,
                autoCache = autoCache,
            };
        }

    }
}