using DG.Tweening;
using UnityEngine;

namespace GameBox
{
    /// <summary>
    /// 弹窗基类
    /// </summary>
    public partial class PopupBase: WindowBase
    {
        protected float popupTime = 0.5f;

        /// <summary>
        /// 打开特效
        /// </summary>
        protected override void OnOpen()
        {
            if (Animator != null)
            {
                AddDisableRef();
                Animator.SetTrigger("Open");
            }
            else
            {
                base.OnOpen();
            }
            
        }

        /// <summary>
        /// 关闭特效
        /// </summary>
        protected override void OnClose()
        {
            if (Animator != null)
            {
                AddDisableRef();
                Animator.SetTrigger("Close");
            }
            else
            {
                base.OnClose();
            }
        }

        /// <summary>
        /// 动画事件
        /// </summary>
        /// <param name="eventName"></param>
        protected override void OnAnimEvent(string eventName)
        {
           
            switch (eventName)
            {
                case "open_end":
                    SubDisableRef();
                    OnFinishOpen();
                    break;
                
                case "close_end":
                    SubDisableRef();
                    OnFinishClose();
                    break;
            }
        }

        /// <summary>
        /// 直接关闭弹窗
        /// </summary>
        public virtual void CloseDirectly(bool playCloseSound = false)
        {
            OnFinishClose();
        }
    }
}