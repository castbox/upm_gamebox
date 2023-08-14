using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameBox
{
    public class UIButtonBase: Button
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            PlayClickSound();
        }

        /// <summary>
        /// 播放按钮音效
        /// </summary>
        protected virtual void PlayClickSound()
        {
            // TODO 播放按钮音效    
        }
    }
}