using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameBox
{
    /// <summary>
    /// UI容器基类
    /// </summary>
    public class UIContainerBase : UIElementBase
    {

        protected List<UIElementBase> _children;

        public List<UIElementBase> Children
        {
            get
            {
                if (_children == null) _children = new List<UIElementBase>(10);
                return _children;
            }
        }

        protected CanvasGroup _canvasGroup;


        protected override void OnCreateOver()
        {
            base.OnCreateOver();
            if (Transform.TryGetComponent(out _canvasGroup))
            {
                _canvasGroup.alpha = 1;
            }
            
        }
        
        // public virtual void AddSubUI<T>(string address, Action<T> onComplete, Transform parent = null) where T : UIElementBase
        // {
        //     UIElementFactory.Load<T>(address, (ui) =>
        //     {
        //         AddChild(ui);
        //         onComplete?.Invoke(ui);
        //     }, parent);
        // }

        public virtual T AddSubUI<T>(Component target)where  T : UIElementBase
        {
            return AddSubUI<T>(target.gameObject);
        }
        
        public virtual T AddSubUI<T>(GameObject target)where  T : UIElementBase
        {
            if (target == null) return null;
            return AddSubUI<T>(target.transform);
        }
        
        public virtual T AddSubUI<T>(Transform target)where  T : UIElementBase
        {
            if (target == null || target.gameObject == null) return null;
            T ui = Activator.CreateInstance<T>();
            ui.BindView(target);
            AddChild(ui);
            return ui;
        }

        public void AddChild(UIElementBase ui)
        {
            if (!Children.Contains(ui))
            {
                ui.ParentUI = this;
                Children.Add(ui);
            }
        }

        public bool RemoveChild(UIElementBase ui)
        {
            if (Children.Contains(ui))
            {
                Children.Remove(ui);
                return true;
            }
            return false;
        }

        public bool RemoveChild(string childName)
        {
            var ui = GetChildByName(childName);
            if(null != ui) return RemoveChild(ui);
            return false;
        }

        public void DestroyChild(string childName)
        {
            var ui = GetChildByName(childName);
            if(null != ui) DestroyChild(ui);
        }

        public void DestroyChild(UIElementBase ui)
        {
            RemoveChild(ui);
            ui.Dispose();
        }
        
        public UIElementBase GetChildByName(string childName)
        {
            return Children.Find(c => c.Name == childName);
        }


        public void SetToTop()
        {
            Transform.SetAsLastSibling();
        }


        public void SetToBottom()
        {
            Transform.SetAsFirstSibling();
        }
        
        
        public override void Dispose()
        {
            base.Dispose();
            if (Children.Count > 0)
            {
                foreach (var c in Children)
                {
                    c.Dispose();
                }
                Children.Clear();
            }
        }
    }
}