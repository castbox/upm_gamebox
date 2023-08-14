using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GameBox
{
    public abstract class ViewBase : UIContainerBase
    {

        #region 管理器引用

        /// <summary>
        /// UI引用
        /// </summary>
        protected UIManager UI => UIManager.Instance;


        #endregion

        #region 属性设置

        /// <summary>
        /// 包名配置
        /// </summary>
        public string BundleName { get; set; } = "";

        #endregion

        #region 事件管理
        
        /// <summary>
        /// 注册按钮事件
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="callback"></param>
        public void AddButton(Button btn, UnityAction callback)
        {
            btn.onClick.AddListener(callback);
        }

        public void RemoveButton(Button btn, UnityAction callback)
        {
            btn.onClick.RemoveListener(callback);
        }

        #endregion

        #region 音效

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="key"></param>
        public virtual void PlaySound(string key)
        {

        }

        #endregion

        #region UI 行为

        /// <summary>
        /// 创建视图
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="parent"></param>
        /// <param name="bundleName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateView<T>(string assetName, Transform parent = null, string bundleName = "") where T : ViewBase
        {
            if (string.IsNullOrEmpty(bundleName)) bundleName = BundleName;
            var view = UI.CreateView<T>(assetName, bundleName, parent);
            AddChild(view);
            return view;
        }
        
        /// <summary>
        /// 打开窗体
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="parent"></param>
        /// <param name="bundleName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T OpenWindow<T>(string assetName, Transform parent = null, string bundleName = "") where T : WindowBase
        {
            if (string.IsNullOrEmpty(bundleName)) bundleName = BundleName;
            return UI.OpenWindow<T>(assetName, bundleName, parent);
        }

        /// <summary>
        /// 打开弹窗
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="parent"></param>
        /// <param name="bundleName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T OpenPopup<T>(string assetName, Transform parent = null, string bundleName = "") where T : PopupBase
        {
            if (string.IsNullOrEmpty(bundleName)) bundleName = BundleName;
            return UI.OpenPopup<T>(assetName, bundleName, parent);
        }

        #endregion

        #region 销毁回调

        protected override void OnDestroy()
        {
            Dispose();
        }

        #endregion
        
    }
}