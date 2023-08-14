using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameBox
{
    
    public class UIClickable: MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {

        public Action OnClicked;
        public Action OnTouchDown;
        public Action OnTouchUp;

        private Graphic _graphic;

        private void Awake()
        {
            _graphic = transform.GetComponent<Graphic>();
            if (_graphic == null)
            {
                _graphic = transform.GetComponentInChildren<Graphic>();
            }

            if (_graphic == null)
            {
                // 添加依赖
                _graphic = gameObject.AddComponent<Image>();
                _graphic.color = new Color(0, 0, 0, 0);
            }

            if (_graphic == null)
            {
                Debug.LogError("UIClickable 找不到宿主对象, 确保已经其添加到<Graphic>或者<Image>对象上!");
                return;
            }
            
            _graphic.raycastTarget = true;
            
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnTouchDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnTouchUp?.Invoke();
        }
    }
}