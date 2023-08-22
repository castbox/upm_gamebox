namespace GameBox
{
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;
    public interface IResLoader
    {
        
        /// <summary>
        /// 加载资源, bundle名称
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="bundleName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Load<T>(string asset, string bundleName = "") where T : Object;


    }
}