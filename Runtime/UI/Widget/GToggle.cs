using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameBox
{
    public class GToggle: MonoBehaviour, IGToggle
    {
        [SerializeField] private Button _btnTrigger;
        [SerializeField] private GameObject _objectOn;
        [SerializeField] private GameObject _objectOff;


        public int Id { get; set; }
        
        private bool _value;
        public bool Value
        {
            get => _value;
            set => SetValue(value);
        }

        public Action<bool> OnValueChanged;

        private void Awake()
        {
            _btnTrigger.onClick.AddListener(OnClickTrigger);
            Init();
        }

        public virtual void Init(bool autoHideTrigger = true)
        {
            Value = false;
            if (autoHideTrigger)
            {
                var img = _btnTrigger.GetComponent<Image>();
                if (null != img) img.color = new Color(1, 1, 1, 0);
            }
        }


        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(bool value)
        {
            if (_value != value) OnValueChanged?.Invoke(value);
            _value = value;
            
            _objectOn.SetActive(value);
            _objectOff.SetActive(!value);
        }

        protected virtual void OnClickTrigger() => Value = !Value;
   
    }
}