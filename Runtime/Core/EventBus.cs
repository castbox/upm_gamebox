using UnityEngine;

namespace GameBox
{
    using System;
    using System.Collections.Generic;
    
    public static class EventBus
    {
        private static Dictionary<string, List<Delegate>> _events = new Dictionary<string, List<Delegate>>();

        public static void Register<T>(Action<T> callback) where T : struct
        {
            var t = typeof(T).Name;
            if (!_events.ContainsKey(t))
            {
                _events[t] = new List<Delegate>();
            }
            _events[t].Add(callback);
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        public static void Unregister<T>(Action<T> callback) where T : struct
        {
            var t = typeof(T).Name;
            if (_events.ContainsKey(t))
            {
                _events[t].Remove(callback);
            }
        }

        /// <summary>
        /// 移除回调
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        public static void Unregister(string key, Delegate callback)
        {
            if (_events.ContainsKey(key))
            {
                _events[key].Remove(callback);
            }
        }
        
        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="evt"></param>
        /// <typeparam name="T"></typeparam>
        public static void Trigger<T>(T evt) where T : struct
        {
            var t = typeof(T).Name;
            if (_events.ContainsKey(t))
            {
                var list = _events[t];
                for (int i = 0; i < list.Count; i++)
                {
                    try
                    {
                        if (null != list[i]) ((Action<T>)list[i])?.Invoke(evt);
                    }
                    catch (Exception exp)
                    {
                        Debug.LogError($"Error when trigger event<{t}> : { exp.ToString() }");
                    }

                }
            }
        }

        /// <summary>
        /// 释放所有事件
        /// </summary>
        public static void ReleaseAll()
        {
            foreach (var k in _events.Keys)
            {
                _events[k]?.Clear();
            }
            _events.Clear();
        }

    }

}