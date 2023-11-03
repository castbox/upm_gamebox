using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameBox
{
    /// <summary>
    /// 屏幕适配器
    /// </summary>
    public class ScreenFitter
    {
        public static float SAFE_TOP_HEIGHT = 80;

        private Canvas _rootCanvas;
        private CanvasScaler _canvasScaler;
        private int _screenWidth;
        private int _screenHeight;
        private float _screenRatio;
        private RectTransform _rootNode;
        private bool _isPad;
        private bool _isLongScreen;
        private float _uiScale;


        private Vector2 _designResolution;
        public Vector2 DesignSize => _designResolution;

        public bool IsPad
        {
            get => _isPad;
            set => SetPadMode(value);
        }

        public bool IsLongScreen
        {
            get => _isLongScreen;
            set => _isLongScreen = value;
        }

        public float ScreenRatio
        {
            get => this._screenRatio;
        }


        public float UIScale
        {
            get
            {
                float scX = (float) DesignSize.x / Screen.width;
                float scY = (float) DesignSize.y / Screen.height;
                if (_canvasScaler.matchWidthOrHeight < 1)
                {
                    return scX;
                }
                return scY;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(RectTransform root)
        {
            _designResolution = new Vector2(1080, 1920);// 默认竖屏
            
            _rootCanvas = root.GetComponent<Canvas>();
            _canvasScaler = root.GetComponent<CanvasScaler>();
            _designResolution = _canvasScaler.referenceResolution;
            _rootNode = root;
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
            _screenRatio = (float)Screen.width / Screen.height;
            _isLongScreen = false;
            
            if (_screenRatio >= 0.65f) // 包含新版的iPad
            {
                SetPadMode(true);
            }
            else
            {
                SetPadMode(false);
                if (_screenRatio < 0.48f)
                {
                    _isLongScreen = true;
                    // SetTopSafeArea();
                }
            }

        }
        
        #region Pad 平板适配模式

        private void SetPadMode(bool value)
        {
            _isPad = value;
            // TODO: 更新Pad屏幕适配
            if (value)
            {
                _canvasScaler.matchWidthOrHeight = 1;    // PAD模式下定义卡高
            }
            else
            {
                _canvasScaler.matchWidthOrHeight = 0;  // Phone模式下定义卡宽
            }
            
        }
        

        #endregion
        

    }
}