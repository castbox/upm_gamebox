using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameBox
{
    
    /// <summary>
    /// 基于Mono的对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoPool<T> where T : MonoPoolObject
    {
        // 对象池
        protected Queue<MonoPoolObject> _objects;
        
        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init()
        {
            _objects = new Queue<MonoPoolObject>();
        }

        // 创建对象的方法
        public abstract T Create<T>(Transform parent = null);
        
        // 
        public virtual T Get()
        {
            if (Count > 0)
            {
                T obj = _objects.Dequeue() as T;
                return obj;
            }

            return Create<T>();
        }

        public virtual void OnRecycle<T>(T obj) 
        {
            OnRecycle(obj as MonoPoolObject);
        }

        public virtual void OnRecycle(MonoPoolObject obj)
        {
            _objects.Enqueue(obj); 
        }


        public int Count => _objects.Count;

        public virtual void Dispose()
        {
            T obj;
            while (_objects.Count > 0)
            {
                obj = _objects.Dequeue() as T;
                Object.Destroy(obj);
            }
            _objects.Clear();
        }

    }
    
    
    /// <summary>
    /// 池化对象
    /// </summary>
    public abstract class MonoPoolObject : MonoBehaviour
    {

        public Action<MonoPoolObject> OnRecycle;
        public virtual void Recycle()
        {
            OnRecycle?.Invoke(this as MonoPoolObject);
        }
    }
    
}