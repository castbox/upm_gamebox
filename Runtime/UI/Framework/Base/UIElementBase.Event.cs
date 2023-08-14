using System;
using System.Collections.Generic;
using System.Linq;

namespace GameBox
{
    /// <summary>
    /// 事件管理模块
    /// 可将当前的EventBus事件管理替换为任意等效的事件系统
    /// </summary>
    public partial class UIElementBase
    {
        
        
        #region 事件管理
        
        private Dictionary<string, HashSet<Delegate>> _eventCache = new Dictionary<string, HashSet<Delegate>>(20);

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="callback"></param>
        public virtual void AddEvent<T>(Action<T> callback) where T : struct
        {
            var t = typeof(T).Name;
            if(!_eventCache.ContainsKey(t)) _eventCache[t] = new HashSet<Delegate>();
            _eventCache[t].Add(callback);
            EventBus.Register(callback);
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="e"></param>
        public virtual void SendEvent<T>(T e = default) where T : struct
        {
            EventBus.Trigger(e);
        }
        
        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        public virtual void RemoveEvent<T>(Action<T> callback) where T : struct
        {
            var t = typeof(T).Name;
            if (_eventCache.ContainsKey(t)) _eventCache[t].Remove(callback);
            EventBus.Unregister(callback);
        }

        /// <summary>
        /// 释放所有事件
        /// </summary>
        public virtual void ReleaseAllEvents()
        {
            if (_eventCache != null && _eventCache.Count > 0)
            {
                foreach (var key in _eventCache.Keys)
                {
                    foreach (var callback in _eventCache[key])
                    {
                        EventBus.Unregister(key, callback);
                    }
                }
                
                _eventCache.Clear();
                _eventCache = null;
            }
        }
        

        #endregion
    }
}