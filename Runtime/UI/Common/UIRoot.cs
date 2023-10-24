
namespace GameBox
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using System;
    using Object = UnityEngine.Object;
    
    public static class UIUtils
    {
        public static void AddToParent(this GameObject go, Transform parent)
        {
            go.transform.AddToParent(parent);
        }
        
        public static void AddToParent(this Transform trans, Transform parent)
        {
            var scale = trans.localScale;
            trans.SetParent(parent);
            trans.localScale = scale;
            trans.localPosition = Vector3.zero;
        }
    }
    
    /// <summary>
    /// UIRoot 节点
    /// </summary>
    public class UIRoot: MonoBehaviour
    {
        
        #region 属性定义

        private static readonly string InternalNodeName = "[ESSENTIAL]";
        public static readonly int DesignScreenWidth = 1080;
        public static readonly int DesignScreenHeight = 1920;
        
        [SerializeField] private Canvas _rootCanvas;
        [SerializeField] private RectTransform _essentialNode;
        [SerializeField] private RectTransform _root;
        [SerializeField] private CanvasScaler _rootScaler;
        [SerializeField] private Camera _camera;
        
        public Camera Camera => _camera;
        private int _uiLayerId;

        public CanvasScaler Scaler => _rootScaler;
        

        #endregion
        
        #region 生命周期
        
        /// <summary>
        /// 生成整个UIRoot
        /// </summary>
        private void Install()
        {
            gameObject.name = nameof(UIRoot);
            _uiLayerId = LayerMask.NameToLayer("UI"); // UILayer的ID
            
            InitEventSystem();
            InitCamera();
            InitCanvas();
        }
        
        /// <summary>
        /// 初始化根节点Canvas
        /// </summary>
        private void InitCanvas()
        {
            _rootCanvas = gameObject.AddComponent<Canvas>();
            _rootCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            _rootCanvas.worldCamera = Camera;
            
            _rootScaler = gameObject.AddComponent<CanvasScaler>();
            _rootScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _rootScaler.referenceResolution = new Vector2(DesignScreenWidth, DesignScreenHeight);
            _rootScaler.scaleFactor = 0;

            gameObject.AddComponent<GraphicRaycaster>();
            gameObject.layer = _uiLayerId; // 统一设置UI层
        }

        /// <summary>
        /// 初始化摄像机
        /// </summary>
        private void InitCamera()
        {
            _camera = CreateChild<Camera>($"{InternalNodeName}/UICamera", transform);
            _camera.orthographic = true;
            _camera.clearFlags = CameraClearFlags.Depth;
            _camera.orthographicSize = 15;
            _camera.depth = 0;
            _camera.nearClipPlane = -1;
            _camera.farClipPlane = 100;
        }

        /// <summary>
        /// 初始化UI事件系统
        /// </summary>
        private void InitEventSystem()
        {
            var esys = Object.FindObjectOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = CreateChild<EventSystem>($"{InternalNodeName}/EventSystem", transform);
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
            else
            {
                esys.name = "EventSystem";
                esys.gameObject.AddToParent(_essentialNode);
            }
        }

        /// <summary>
        /// 使用Prefab加载后实现初始化
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void Awake()
        {
            name = nameof(UIRoot);
            // DontDestroyOnLoad(gameObject);
            InitEventSystem();
        }

        private void Start()
        {
            ApplySafeArea(Screen.safeArea);
        }

        /// <summary>
        /// 创建一个UIRoot
        /// </summary>
        /// <returns></returns>
        public static UIRoot Create()
        {
            var go = Instantiate(Resources.Load<GameObject>("ui_root"));
            var comp = go.GetComponent<UIRoot>();
            if (null == comp) comp = go.AddComponent<UIRoot>();
            return comp;
        }

        #endregion
        
        #region 屏幕适配

        
        /// <summary>
        /// 应用安全区数据
        /// </summary>
        /// <param name="r"></param>
        private void ApplySafeArea(Rect r)
        {
            Vector2 anchorMin = r.position;
            Vector2 anchorMax = r.position + r.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            
            // 应用于根容器
            if (_root != null)
            {
                _root.anchorMin = anchorMin;
                _root.anchorMax = anchorMax;
            }
        }



        #endregion
        
        #region 工具接口

        public RectTransform CreateChildTrans(string childPath, Transform parent = null)
        {
            var go = CreateChild(childPath, parent);
            if (go != null) return go.GetComponent<RectTransform>();
            return null;
        }
        
        public GameObject CreateChild(string childPath, Transform parent = null)
        {
            if (null == parent)
            {
                parent = transform;
            }

            GameObject child = null;
            var paths = childPath.Split('/');
            var p = parent;
            if (paths.Length > 0)
            {
                int i = 0;
                while (i < paths.Length)
                {
                    var ct = p.Find(paths[i]);
                    if (null == ct)
                    {
                        child = new GameObject(paths[i]) { layer = _uiLayerId };
                        child.AddToParent(p);
                    }
                    else
                    {
                        child = ct.gameObject;
                    }
                    BindRectTransform(child);
                    p = child.transform;
                    i++;
                }
            }
            return child;
        }

        public T CreateChild<T>(string childPath, Transform parent = null) where T : Component
        {
            GameObject child = CreateChild(childPath, parent);
            return child.AddComponent<T>();
        }
        
        public static RectTransform BindRectTransform(GameObject go)
        {
            if (!go.TryGetComponent<RectTransform>(out var rect))
                rect = go.AddComponent<RectTransform>();
            
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.pivot = Vector2.one * 0.5f;
            return rect;
        }

        #endregion
        
    }
    
}