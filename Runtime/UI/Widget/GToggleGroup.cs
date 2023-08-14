using System;
using System.Collections.Generic;
using GameBox;
using UnityEngine;

namespace Guru.UI
{
    public class GToggleGroup: MonoBehaviour
    {

        private int _toggleIndex = 0;

        private List<IGToggle> _toggles;

        public Action<int> OnItemSelected;

        #region 生命周期

        private void Awake()
        {
            _toggleIndex = 0;
            _toggles = new List<IGToggle>(10);
        }

        #endregion
        
        /// <summary>
        /// 注册选框
        /// </summary>
        /// <param name="toggle"></param>
        public void RegisterToggle(IGToggle toggle)
        {
            toggle.Id = _toggleIndex;
            _toggleIndex++;
            _toggles.Add(toggle);
        }

        /// <summary>
        /// 当选框被选中
        /// </summary>
        /// <param name="id"></param>
        public void OnToggleSelected(IGToggle toggle)
        {
            if (_toggles.Count == 0) return;
            
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i].Value = (_toggles[i] == toggle);
            }
            OnItemSelected?.Invoke(toggle.Id);
        }

    }
}