using System;
using UnityEngine;

namespace GameBox
{
    /// <summary>
    /// 单利模式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GSingleton<T> where T : class, new()
    {

        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                    (_instance as GSingleton<T>)?.OnSingletonInit();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void OnSingletonInit()
        {
            
        }

        public virtual void Dispose()
        {
            
        }

    }


    /// <summary>
    /// 单利 MonoBehaviour对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GMonoSingleton<T>: MonoBehaviour where T : Component
    {
        private static T _instance;
        /// <summary>
        /// 单利引用
        /// </summary>
        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    var _name = typeof(T).Name; 
                    GameObject go = GameObject.Find(_name);
                    if (null == go)
                    {
                        go = new GameObject(_name);
                        _instance = go.AddComponent<T>();
                    }
                    else
                    {
                        _instance = go.GetComponent<T>();
                    }
                }
                return _instance;
            }
        }


        public static bool IsInstanceExisted => _instance != null && _instance.gameObject != null;
        
        void Awake()
        {
            Init();
        }

        protected virtual void Init()
        {
            
        }

        public virtual void Dispose()
        {
            
        }


        public static void ReleaseInstance()
        {
            if (IsInstanceExisted)
            {
                (Instance as GMonoSingleton<T>)?.Dispose();
                Destroy(Instance.gameObject);
                _instance = null;
            }
        }

    }


}