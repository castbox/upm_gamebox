using System;
using DG.Tweening;
using UnityEngine;

namespace GameBox
{

    /// <summary>
    /// 页面基类
    /// </summary>
    public partial class PageBase : ViewBase
    {
        // 内置动画标签
        public const string UI_ANIM_FADE_IN = "anim_fade_in";
        public const string UI_ANIM_FADE_OUT = "anim_fade_out";
        public const string UI_ANIM_POP_OPEN = "anim_pop_open";
        public const string UI_ANIM_POP_CLOSE = "anim_pop_close";
        
        private static readonly float FadeAnimTime = 0.5f;
        private static readonly float PopAnimTime = 0.5f;
        
        /// <summary>
        /// 播放UI动画
        /// </summary>
        public virtual bool PlayUIAnim( string name = "", Action callback = null)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(name))
            {
                switch (name)
                {
                    case UI_ANIM_FADE_IN: return PlayUIAnimFade(0, 1,0, callback);  // 淡入动画
                    case UI_ANIM_FADE_OUT: return PlayUIAnimFade(1, 0,0, callback); // 淡出动画
                    case UI_ANIM_POP_OPEN: return PlayUIAnimPopUp(0, callback); // 弹出动画
                    case UI_ANIM_POP_CLOSE: return PlayUIAnimPopOut(0, callback); // 收起动画
                }
            }
            return result;
        }
        
        private CanvasGroup _uiCanvasGroup; // 组件必须具备 CanvasGroup
        private CanvasGroup FadeCanvasGroup
        {
            get
            {
                if (_uiCanvasGroup == null)
                {
                    _uiCanvasGroup = Transform.GetComponentInChildren<CanvasGroup>(); // 先找子节点是否有CG
                    if(_uiCanvasGroup == null) _uiCanvasGroup = GetComponent<CanvasGroup>();
                }
                return _uiCanvasGroup;
            }
        }
        
        /// <summary>
        /// 淡入动画
        /// </summary>
        /// <param name="callback"></param>
        protected bool PlayUIAnimFade(float start, float end, float time = 0, Action callback = null)
        {
            if (time == 0) time = FadeAnimTime;
            if (FadeCanvasGroup != null)
            {
                FadeCanvasGroup.alpha = start;
                FadeCanvasGroup.DOFade(end, time)
                    .OnComplete(() => callback?.Invoke());
                return true;
            }
            return false;
        }

        
        
        private Transform _scaleNode; // 组件必须具备 CanvasGroup
        private Transform ScaleNode
        {
            get
            {
                if(_scaleNode == null){
                    _scaleNode = Find("root");
                    if (_scaleNode == null) _scaleNode = Transform;
                }
                return _scaleNode;;
            }
        }
        
        /// <summary>
        /// 播放弹出动画
        /// 组件在层级上最好具备 root 节点
        /// </summary>
        /// <param name="isOpen"></param>
        /// <param name="time"></param>
        /// <param name="callback"></param>
        protected bool PlayUIAnimPopUp(float time = 0, Action callback = null)
        {
            if (time == 0) time = PopAnimTime;
            ScaleNode.localScale = Vector3.zero;
            Transform.DOScale(1, time)
                .SetEase(Ease.OutBack)
                .OnComplete(() => callback?.Invoke());
            return true;
        }
        
        /// <summary>
        /// 播放UI收起动画
        /// </summary>
        /// <param name="time"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected bool PlayUIAnimPopOut(float time = 0, Action callback = null)
        {
            if (time == 0) time = PopAnimTime;
            ScaleNode.localScale = Vector3.one;
            Transform.DOScale(0, time)
                .SetEase(Ease.Linear)
                .OnComplete(() => callback?.Invoke());
            return true;
        }
        
        #region 动画控制面板
        
        /// <summary>
        /// 打开动画
        /// </summary>
        /// <param name="animName"></param>
        /// <param name="callback"></param>
        public virtual void OpenWithAnim(string animName = "", Action callback = null)
        {
            OnBeforeOpen();
            // 执行打开的动画逻辑
            PlayUIAnim(animName, callback);
            OnOpen();
        }

        /// <summary>
        /// 关闭动画
        /// </summary>
        /// <param name="animName"></param>
        /// <param name="callback"></param>
        public virtual void CloseWithAnim(string animName = "", Action callback = null)
        {
            OnBeforeClose();
            PlayUIAnim(animName, () =>
            {
                callback?.Invoke();
                OnClose();
            });
        }
        
        #endregion
        
    }
    
}