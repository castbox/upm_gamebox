using System;
using UnityEngine;
using DG.Tweening;

namespace GameBox
{
    public partial class WindowBase : PageBase
    {

        public Action<WindowBase> OnWindowClosed;


        protected override void OnClose()
        {
            if (_canvasGroup != null)
            {
                AddDisableRef();
                float time = 0.5f;
                _canvasGroup.DOFade(0f, time)
                    .OnComplete(() =>
                {
                    SubDisableRef();
                    OnFinishClose();
                });
            }
            else
            {
                OnFinishClose();
            }
            
            
        }

        protected override void OnFinishClose(bool withCloseHandle = true)
        {
            FinishCloseHandle?.Invoke();
            if(withCloseHandle) OnWindowClosed?.Invoke(this);
            Dispose();
            Destroy(GameObject);
        }

        /// <summary>
        /// 直接杀掉窗体
        /// </summary>
        /// <param name="withCloseHandle">窗体关闭时是否发出关闭回调</param>
        public virtual void Kill(bool withCloseHandle = true) => OnFinishClose(withCloseHandle);


    }
}