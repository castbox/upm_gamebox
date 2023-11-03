using UnityEngine;

namespace GameBox
{
    public interface IPageItem
    {
        /// <summary> 组件 </summary>
        Transform transform { get; set; }
        /// <summary> 激活效果（选中） </summary>
        void OnSelect();

        /// <summary> 隐藏效果（取消选中） </summary>
        void UnSelect();

        /// <summary> 被点击时候 </summary>
        void JoinClick();
    }
}
