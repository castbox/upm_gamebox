using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Linq;

namespace GameBox
{
    public class UIPageView : ScrollRect
    {
        public class UnityEventInt : UnityEvent<int> { }
        private const float StopDeltaX = 200f;
        private const float RefDistance = 140;
        private bool _isUnderInertia;
        /// <summary>页面更改</summary>
        public UnityEventInt PageChanged = new UnityEventInt();
        /// <summary>当前的编号</summary>
        public int PageIndex { get; private set; }
        /// <summary>子节点</summary>
        public List<IPageItem> _childs = new List<IPageItem>();
        private float _dragEndTime;

        private bool IsRTL;
        private void Update()
        {
            if (_isUnderInertia)
            {
                if (Mathf.Abs(velocity.x) < StopDeltaX)
                {
                    _isUnderInertia = false;
                    StopMovement();
                    OnMovementStopped();
                }
            }
        }
        /// <summary>
        /// 开始滑动
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnBeginDrag(PointerEventData eventData)
        {
            DOTween.Kill(GetInstanceID());
            foreach (var child in _childs)
            {
                child.UnSelect();
            }
            base.OnBeginDrag(eventData);
        }
        /// <summary>
        /// 结束滑动
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnEndDrag(PointerEventData eventData)
        {
            _dragEndTime = Time.realtimeSinceStartup;
            if (velocity.x < -StopDeltaX)
            {
                _isUnderInertia = false;
                StopMovement();
                NextPage();
            }
            else if (velocity.x > StopDeltaX)
            {
                _isUnderInertia = false;
                StopMovement();
                PreviousPage();
            }
            else
            {
                _isUnderInertia = true;
            }
            base.OnEndDrag(eventData);
        }

        private void OnMovementStopped()
        {
            DOTween.Kill(GetInstanceID());
            if (_childs.Count > 1)
            {
                var pageIndex = _childs.IndexOf(_childs.OrderBy(e => Mathf.Abs(transform.parent.InverseTransformPoint(e.transform.position).x)).First());

                if (IsRTL)
                    pageIndex = _childs.Count - 1 - pageIndex;

                SetIndex(pageIndex, true, () =>
                {
                    for (var dex = 0; dex < _childs.Count; dex++)
                    {
                        if (dex == pageIndex)
                            _childs[dex].OnSelect();
                        else
                            _childs[dex].UnSelect();
                    }
                });
            }
        }
        public float GetNormalizedX(int index)
        {
            return 1f / (_childs.Count - 1) * index;
        }
        /// <summary>
        /// 下一页
        /// </summary>
        /// <param name="callback">回调方法</param>
        public bool NextPage(UnityAction callback = null)
        {
            return SetIndex(PageIndex + 1);
        }
        /// <summary>
        /// 上一页
        /// </summary>
        /// <param name="callback">回调方法</param>
        public bool PreviousPage(UnityAction callback = null)
        {
            return SetIndex(Mathf.Max(0, PageIndex - 1));
        }
        /// <summary>
        /// 当前页
        /// </summary>
        /// <param name="callback">回调方法</param>
        public bool CurrentPage(UnityAction callback = null)
        {
            return SetIndex(PageIndex);
        }
        /// <summary>
        /// 设置页面
        /// </summary>
        /// <param name="pageIndex">设置页面</param>
        /// <param name="animtor">包含动画</param>
        public bool SetIndex(int pageIndex, bool animtor = true, UnityAction callback = null, float duration = 0.2f)
        {
            if (pageIndex < 0 || pageIndex > _childs.Count - 1)
            {
                return false;
            }
            else
            {
                PageIndex = pageIndex;
                var normalizedPosX = GetNormalizedX(PageIndex);
                DOTween.Kill(GetInstanceID());
                if (!animtor)
                {
                    normalizedPosition = new Vector2(normalizedPosX, 0);
                    PageChanged.Invoke(PageIndex);
                    callback?.Invoke();
                }
                else
                {
                    if (Mathf.Abs(normalizedPosX - normalizedPosition.x) > 0.001f)
                    {
                        this.DOHorizontalNormalizedPos(normalizedPosX, duration).SetId(GetInstanceID()).OnComplete(() =>
                        {
                            PageChanged.Invoke(PageIndex);
                            callback?.Invoke();
                        });
                    }
                    else
                    {
                        PageChanged.Invoke(PageIndex);
                        callback?.Invoke();
                    }
                }
                return true;
            }
        }
        /// <summary>
        /// 获取编号
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public IPageItem GetIndex(int pageIndex)
        {
            if (pageIndex >= 0 && pageIndex < _childs.Count) return _childs[pageIndex];
            else { return null; }
        }
        /// <summary>
        /// 获取子对象数目
        /// </summary>
        /// <returns></returns>
        public int GetChildCount()
        {
            return _childs.Count;
        }

        /// <summary>
        /// 更新(初始化)
        /// </summary>
        public void Init(List<IPageItem> items, int initIndex = 0, bool is_r2l = false)
        {
            IsRTL = is_r2l;
            Canvas.ForceUpdateCanvases();
            _childs = items;
            var n = 0;
            foreach (var child in _childs)
            {
                child.UnSelect();
                var button = child.transform.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    var index = _childs.IndexOf(child);
                    button.onClick.AddListener(() =>
                    {
                        if (PageIndex != index)
                        {
                            SetIndex(index, true, () =>
                            {
                                for (var i = 0; i < _childs.Count; i++)
                                {
                                    if (i == index)
                                        _childs[i].OnSelect();
                                    else
                                        _childs[i].UnSelect();
                                }
                            });
                        }
                        else
                            _childs[index].JoinClick();
                    });
                }

                if (n == initIndex)
                    child.OnSelect();

                n++;
            }
            onValueChanged.RemoveAllListeners();
            PageIndex = initIndex;
            normalizedPosition = new Vector2(GetNormalizedX(PageIndex), 0);
            PageChanged.Invoke(PageIndex);
            //限制拖动范围
            onValueChanged.AddListener(v =>
            {
                verticalNormalizedPosition = Mathf.Clamp(verticalNormalizedPosition, 0, 1);
                horizontalNormalizedPosition = Mathf.Clamp(horizontalNormalizedPosition, 0, 1);
            });
        }
    }
}
