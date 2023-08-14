using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;


namespace GameBox
{
    /// <summary>
    /// 基于UGUI的视图元素基类
    /// </summary>
    public partial class UIElementBase
    {

        #region UI基础属性

        public string Name
        {
            get => _transform.name;
            set => _transform.name = value;
        }
        
        public Vector2 LocalPosition
        {
            get => _transform.localPosition;
            set => _transform.localPosition = value;
        }

        public Vector3 Position
        {
            get => _transform.position;
            set => _transform.position = value;
        }

        public Vector2 AnchoredPos
        {
            get => RectTransform.anchoredPosition;
            set => RectTransform.anchoredPosition = value;
        } 

        public float Scale
        {
            get => _transform.localScale.x;
            set => _transform.localScale = Vector3.one * value;
        }

        public bool Active
        {
            get => GameObject.activeSelf;
            set => GameObject.SetActive(value);
        }
        
        
        private UIElementBase _parentUI;
        public UIElementBase ParentUI
        {
            get => _parentUI;
            set => _parentUI = value;
        }

        public GameObject GameObject => _gameObject;

        private RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform;

        private GameObject _gameObject;
        
        private Transform _transform;
        public Transform Transform => _transform;

        private Dictionary<string, Action<object>> _eventDict;

        public Action OnLoaded = () => { };
        public Action OnBeforeDispose = () => { };

        protected int _disableRefCount;   // 引用计数标志位

        protected Animator _animator; // 动画控制器
        public Animator Animator => _animator;

        protected UIBinder _binder;  // UI 绑定器

        #endregion

        #region 引用计数锁

        /// <summary>
        /// UI是否可用 (否: 即是当前被事件占用)
        /// </summary>
        public bool UIDIsabled => _disableRefCount != 0;
        /// <summary>
        /// 增加引用计数
        /// </summary>
        public virtual void AddDisableRef() => _disableRefCount++;
        /// <summary>
        /// 减少引用计数
        /// </summary>
        public virtual void SubDisableRef() => _disableRefCount--;
        /// <summary>
        /// 清空引用计数
        /// 请不要轻易调用该方法, 操作需要谨慎 
        /// </summary>
        public virtual void CleanDisableRef() => _disableRefCount = 0; 

        #endregion

        #region 生命周期
        
        
        public void BindView(Transform transform)
        {
            _disableRefCount = 0;

            _transform = transform;
            _gameObject = transform.gameObject;
            _rectTransform = _transform as RectTransform;
            _eventDict = new Dictionary<string, Action<object>>(5);
            if (!_transform.TryGetComponent(out _binder))
            {
                _binder = _transform.gameObject.AddComponent<UIBinder>();
            }

            if (_transform.TryGetComponent(out _animator))
            {
                _binder.OnAnimEvent = OnAnimEvent;
            }
            
            _binder.OnViewStart = OnStart;
            _binder.OnViewEnabled = OnEnabled;
            _binder.OnViewDisabled = OnDisabled;
            _binder.OnViewDestroy = OnDestroy;
            
            OnCreateOver();
            Init();
            OnLoaded?.Invoke();
        }
        
        /// <summary>
        /// 组件初始化阶段
        /// </summary>
        protected virtual void OnCreateOver()
        {
            
        }


        protected virtual void Init()
        {
            
        }

        protected virtual void OnStart()
        {
            
        }

        protected virtual void OnEnabled()
        {
            
        }


        protected virtual void OnDisabled()
        {
            
        }

        protected virtual void OnDestroy()
        {
            
        }
        
        public void BindView(GameObject gameObject)
        {
            BindView(gameObject.transform);
        }

        /// <summary>
        /// 查找绑定的UI元素
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindUI<T>(string name) where T : Component
        {
            if (_binder != null)
            {
                return _binder.variables.Get<T>(name);
            }
            return default;
        }
        
        /// <summary>
        /// 查找绑定的GameObject元素
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindObj<T>(string name)
        {
            if (_binder != null)
            {
                return _binder.variables.Get<T>(name);
            }
            return default;
        }
        
        
        /// <summary>
        /// 查找子节点上的UI元素
        /// </summary>
        /// <param name="childPath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Find<T>(string childPath) where T : Component
        {
            T comp = default;
            Transform t = Find(childPath);
            if (t != null)
            {
                t.TryGetComponent(out comp);
            }
            return comp;
        }
        
        /// <summary>
        /// 查找子节点
        /// </summary>
        /// <param name="childPath"></param>
        /// <returns></returns>
        public Transform Find(string childPath)
        {
            return _transform.Find(childPath);
        }
        
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            ReleaseAllEvents(); // 自动释放所有的注册事件
        }


        #endregion
        
        #region 工具接口

        
        public T AddComponent<T>() where T : Component
        {
            return _gameObject.AddComponent<T>();
        }

        public T GetComponent<T>() where T : Component
        {
            return _transform.GetComponent<T>();
        }


        public void Delay(float time, Action callback)
        {
            _binder.OnDelay(time, callback);
        }
        
        /// <summary>
        /// 动画事件
        /// </summary>
        /// <param name="eventName"></param>
        protected virtual void OnAnimEvent(string eventName)
        {
            
        }


        public Coroutine StartCoroutine(IEnumerator corountine) => _binder.StartCoroutine(corountine);
        
        public void StopCoroutine(Coroutine coroutine) => _binder.StopCoroutine(coroutine);

        public void StopAllCoroutines() => _binder.StopAllCoroutines();

        public void Destroy(Object target, float delay= 0f)
        {
            Object.Destroy(target, delay);
        }
        
        #endregion
        
    }
}


