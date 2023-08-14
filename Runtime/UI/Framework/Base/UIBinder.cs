using System;
using System.Collections;
using UnityEngine;

namespace GameBox
{
    /// <summary>
    /// UI绑定器
    /// </summary>
    public class UIBinder: MonoBehaviour
    {
        public VariableArray variables;
        public Action OnViewEnabled;
        public Action OnViewDisabled;
        public Action OnViewStart;
        public Action OnViewDestroy;
        public Action<string> OnAnimEvent;
        
        [SerializeField][HideInInspector]
        public string bindScriptPath;


        #region 生命周期

        
        private void Start()
        {
            OnViewStart?.Invoke();
        }
        private void OnEnable()
        {
            OnViewEnabled?.Invoke();
        }
        private void OnDisable()
        {
            OnViewDisabled?.Invoke();
        }

        private void OnDestroy()
        {
            OnViewDestroy?.Invoke();
        }

        #endregion

        #region 动画事件

        /// <summary>
        /// 动画事件
        /// </summary>
        /// <param name="eventName"></param>
        public void OnFrameEvent(string eventName)
        {
            OnAnimEvent?.Invoke(eventName);
        }

        #endregion

        #region 事件支持
        
        /// <summary>
        /// 延时类
        /// </summary>
        /// <param name="time"></param>
        /// <param name="callback"></param>
        public void OnDelay(float time, Action callback)
        {
            StartCoroutine(OnDelayCall(time, callback));
        }

        private IEnumerator OnDelayCall(float time, Action callback)
        {
            if (time > 0)
            {
                yield return new WaitForSeconds(time);
            }
            else
            {
                yield return null;
            }
            callback?.Invoke();
        }

        #endregion

        

        
        
    }
    
    
    
    
}