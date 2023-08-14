


using UnityEngine.U2D;
using UnityEngine.UI;

namespace GameBox
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public enum UIType
    {
        Basic = 0,
        Window = 1,
        Popup = 2,
        Toast = 3,
        Tutorial = 4,
    }

    /// <summary>
    /// 自建根节点枚举
    /// </summary>
    public enum RootNode
    {
        Content,
        Popup,
        Toast,
        Tuto,
    }


    /// <summary>
    /// UI管理器
    /// </summary>
    public partial class UIManager : GSingleton<UIManager>
    {
        
        #region 属性设置

        private static Vector2 DesignResolution = new Vector2(1080, 1920);
       
        
        private UIRoot _uiRoot;
        private Transform _content;
        private Transform _popup;
        private Transform _toast;
        private Transform _tuto;

        // private GLoader Res;  

        private ResManager Res => ResManager.Instance; // loader 可以替换为自己项目中的任意加载器
        
        private WindowBase _curWindow; // 当前开启的Window, 同时刻只会存在一个Window

        /// <summary>
        /// 当前被隐藏的 Popup 栈.
        /// 当一个Popup被关闭的时候, 会自动拉起Stack内的Popup
        /// </summary>
        private Stack<PopupBase> _popups;
        private PopupBase _curPopup; // 当前打开的 Popup
        private Vector2 _fixedResolution;
        public Vector2 FixedResolution => _fixedResolution;
        #endregion

        /// <summary> 界面基础放置位置 </summary>
        public UIRoot Root
        {
            get
            {
                if (_uiRoot == null) InitUIRoot();
                return _uiRoot;
            }
        }

        public CanvasScaler RootScaler => Root.Scaler;

        public Action<CanvasScaler> OnRootScalerChanged;

        /// <summary>
        /// 引用分辨率
        /// </summary>
        public Vector2 ReferenceResolution
        {
            get => RootScaler.referenceResolution;
            set
            {
                RootScaler.referenceResolution = value;
                OnRootScalerChanged?.Invoke(RootScaler);
            }
        }

        /// <summary>
        /// 宽高缩放比例
        /// </summary>
        public float MatchWidthOrHeight
        {
            get => RootScaler.matchWidthOrHeight;
            set
            {
                RootScaler.matchWidthOrHeight = value;
                OnRootScalerChanged?.Invoke(RootScaler);
            }
        }

        public float ScaleRatio
        {
            get
            {
                float val = 1;
                if (MatchWidthOrHeight == 0)
                {
                    // width
                    val = ReferenceResolution.x / Screen.width;
                }
                else
                {
                    // height
                    val = ReferenceResolution.y / Screen.height;
                }
                return val;
            }
        }

        #region 生命周期
        
        /// <summary>
        /// 初始化
        /// </summary>
        public UIManager()
        {
            
        }

        protected override void OnSingletonInit()
        {
            _popups = new Stack<PopupBase>(20);
            InitUIRoot();
        }

        public override void Dispose()
        {
            base.Dispose();

            CloseAllPopups();
            if (_curWindow != null)
            {
                _curWindow.Close();
                _curWindow = null;
            }

            Res.Dispose();
        }


        private void InitUIRoot()
        {
            _uiRoot = UIRoot.Create();
            _content = _uiRoot.CreateChildTrans("root/content");
            _popup = _uiRoot.CreateChildTrans("root/popup");
            _toast = _uiRoot.CreateChildTrans("root/toast");
            _tuto = _uiRoot.CreateChildTrans("root/tuto");

            float ratio = 0;
            if (_uiRoot.Scaler != null)
            {
                
                if (_uiRoot.Scaler.matchWidthOrHeight == 0)
                {
                    ratio = _uiRoot.Scaler.referenceResolution.x / Screen.width;
                }
                else
                {
                    ratio = _uiRoot.Scaler.referenceResolution.y / Screen.height;
                }
            }
            else
            {
                ratio = DesignResolution.x / Screen.width;
            }
            
            _fixedResolution.x = Screen.width * ratio;
            _fixedResolution.y = Screen.height * ratio;
        }
        
        #endregion

        #region UI创建逻辑



        /// <summary>
        /// 创建视图
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="bundleName"></param>
        /// <param name="parent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateView<T>(string assetName, string bundleName = "", Transform parent = null) where T : ViewBase
        {
            GameObject go = Res.CloneGameObject(assetName, bundleName, parent);
            if (null != go)
            {
                var idx = assetName.LastIndexOf("/", StringComparison.Ordinal);
                if (idx < 0) idx = 0;
                T v = BindView<T>(go, assetName.Substring(idx));
                if (!string.IsNullOrEmpty(bundleName)) v.BundleName = bundleName; // 设置依赖包名
                return v;
            }
            Debug.Log($"<color=red>Can't create view at [{bundleName}/{assetName}] !</color>");
            return null;
        }

        /// <summary>
        /// 绑定子视图
        /// </summary>
        /// <param name="target"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T BindView<T>(GameObject target, string name = "") where T : ViewBase
        {
            if(!string.IsNullOrEmpty(name)) target.name = name;
            T v = Activator.CreateInstance<T>();
            v.BindView(target);
            return v;
        }



        /// <summary>
        /// 创建窗体
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="bundleName"></param>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateWindow<T>(string assetName, string bundleName = "", params object[] args) where T : WindowBase
        {
            if(null != _curWindow) _curWindow.Close();
            _curWindow = CreateView<T>(assetName, bundleName, _content);
            if( null != _curWindow ) _curWindow.OnWindowClosed = OnWindowClosed;
            return (T)_curWindow;
        }

        public T CreatePage<T>(string assetName, string bundleName = "", params object[] args) where T : PageBase
        {
            var page = CreateView<T>(assetName, bundleName, _content);
            return (T)page;
        }

        /// <summary>
        /// 打开页面
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="bundleName"></param>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T OpenPage<T>(string assetName, string bundleName = "", params object[] args) where T : PageBase
        {
            var page = CreatePage<T>(assetName, bundleName, _content);
            page.Open();
            return page;
        }


        /// <summary>
        /// 直接打开窗体
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="bundleName"></param>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T OpenWindow<T>(string assetName, string bundleName = "", params object[] args) where T : WindowBase
        {
            var window = CreateWindow<T>(assetName, bundleName, _content);
            window.Open();
            return window;
        }



        /// <summary>
        /// 弹窗被创建
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="bundleName"></param>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreatePopup<T>(string assetName, string bundleName = "", params object[] args) where T : PopupBase
        {
            var window = CreateView<T>(assetName, bundleName, _popup);
            window.OnWindowClosed = OnPopupClosed;
            return window;
        }

        /// <summary>
        /// 打开Popup弹窗
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="bundleName"></param>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T OpenPopup<T>(string assetName, string bundleName = "", params object[] args) where T : PopupBase
        {
            var pop = CreatePopup<T>(assetName, bundleName, _popup);
            pop.Open();

            //当前的 Popup 入栈操作
            if (null != _curPopup)
            {
                _curPopup.Active = false;
                _popups.Push(_curPopup);
            }

            _curPopup = pop;
            return pop;
        }

        /// <summary>
        /// 关闭所有的Popup弹窗
        /// </summary>
        public void CloseAllPopups()
        {
            if (_popups.Count > 0)
            {
                while (_popups.Count > 0)
                {
                    var pop = _popups.Pop();
                    pop.Kill(false);
                }
            }

            if (_curPopup != null)
            {
                _curPopup.Kill(false);
                _curPopup = null;
            }
        }

        /// <summary>
        /// 窗体关闭
        /// </summary>
        /// <param name="view"></param>
        private void OnWindowClosed(WindowBase view)
        {
            if (_curWindow == view) _curWindow = null;
        }


        /// <summary>
        /// Popup 关闭逻辑
        /// </summary>
        /// <param name="view"></param>
        private void OnPopupClosed(WindowBase view)
        {
            if (_popups.Count > 0)
            {
                _curPopup = _popups.Pop();
                _curPopup.Active = true;
            }
            else
            {
                _curPopup = null;
            }
        }

        #endregion

        #region 事件响应





        #endregion

        #region UI查询

        /// <summary>
        /// 窗体是否存在 (按类型)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsWindowExist<T>() where T : WindowBase
        {
            Debug.Log($"当前窗体:{_curWindow}");
            return _curWindow is T;
        }

        /// <summary>
        /// 当前窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CurrentWindow<T>() where T : WindowBase
        {
            return _curWindow as T;
        }

        /// <summary>
        /// 弹窗是否存在 (按类型)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsPopupExist<T>() where T : PopupBase
        {
            foreach (var p in _popups)
            {
                if (p is T) return true;
            }
            return _curPopup is T;
        }


        #endregion

        #region 图集管理

        /// <summary>
        /// 加载 ATLAS 图集
        /// </summary>
        /// <param name="atlasName"></param>
        /// <param name="bundleName"></param>
        public SpriteAtlas LoadAtlas(string atlasName, string bundleName = "")
        {
            return Res.LoadAtlas(atlasName, bundleName);
        }
        
        /// <summary>
        /// 从图集里面加载Sprite对象
        /// </summary>
        /// <param name="spriteName"></param>
        /// <param name="atlasName"></param>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public Sprite LoadSpriteFromAtlas(string spriteName, string atlasName, string bundleName = "")
        {
            var atlas = LoadAtlas(atlasName, bundleName);
            return atlas?.GetSprite(spriteName) ?? null;
        }


        #endregion

        #region 安全区
        
        /// <summary>
        /// 获取安全区域
        /// </summary>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public Rect GetDeviceSafeArea(out float top, out float bottom, out float left, out float right)
        {
            var rect = Screen.safeArea;

            /**
            top = 0;
            if (rect.y != 0)
            {
                top = rect.y;
            }
            else if(Screen.height > rect.height)
            {
                top = (Screen.height - rect.height) * 0.5f;
            }
            
            bottom = 0;
            if (top != 0)
            {
                bottom = (Screen.height - rect.height) - top;
            }
            **/
            
            bottom = Screen.safeArea.y * ScaleRatio;
            top = (Screen.height - (Screen.safeArea.y + Screen.safeArea.height)) * ScaleRatio;

            left = rect.x * ScaleRatio;
            right = (Screen.width - rect.x - rect.width) * ScaleRatio;

#if UNITY_EDITOR
            top = 80;
#endif

            return rect;
        }


        #endregion

        #region 层级深度管理

        /// <summary>
        /// 向节点上添加画布对象
        /// </summary>
        /// <param name="node"></param>
        /// <param name="sortingOrder"></param>
        /// <returns></returns>
        public Canvas AddNodeCanvas(RootNode node, int sortingOrder = 0)
        {
            Transform t = _content;
            switch (node)
            {
                case RootNode.Popup: t = _popup; break;
                case RootNode.Toast: t = _toast; break;
                case RootNode.Tuto: t = _tuto; break;
                default: t = _content; break;
            }

            var canvas = t.GetComponent<Canvas>();
            if (null == canvas)
            {
                canvas = t.gameObject.AddComponent<Canvas>();
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }

            if (sortingOrder > 0)
            {
                canvas.overrideSorting  = true;
                canvas.sortingOrder = sortingOrder;
            }

            return canvas;
        }
            
            



        #endregion

    }
}