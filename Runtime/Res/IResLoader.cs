namespace GameBox
{
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;
    public interface IResLoader
    {
        
        
        T Load<T>(string asset, string bundleName = "") where T : Object;


    }
}