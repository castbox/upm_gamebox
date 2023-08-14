using System;

namespace GameBox
{
    public partial class PageBase: ViewBase
    {
        public Action BeforeOpenHandle;
        public Action FinishOpenHandle;
        public Action BeforeCloseHandle;
        public Action FinishCloseHandle;
        
        /// <summary>
        /// 打开窗口
        /// </summary>
        public void Open()
        {
            OnBeforeOpen();
            // 执行打开的动画逻辑
            OnOpen();
        }

        protected virtual void OnBeforeOpen()
        {
            BeforeOpenHandle?.Invoke();
        }

        protected virtual void OnOpen()
        {
            OnFinishOpen();
        }

        protected virtual void OnFinishOpen()
        {
            FinishOpenHandle?.Invoke();
        }

        public void Close()
        {
            OnBeforeClose();
            
            OnClose();
        }


        protected virtual void OnBeforeClose()
        {
            BeforeCloseHandle?.Invoke();
        }

        protected virtual void OnClose()
        {
            OnFinishClose();
        }

        protected virtual void OnFinishClose(bool withCloseHandle = true)
        {
            FinishCloseHandle?.Invoke();
            Dispose();
            Destroy(GameObject);
        }
    }
}