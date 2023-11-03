using GameBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBox
{
	/// <summary>
	/// SafeArea工具类
	/// </summary>
	public class SafeAreaHelper : GSingleton<SafeAreaHelper>
	{
		#region 私有字段
		private Vector2 min;
		private Vector2 max;
		#endregion

		#region 初始化
		/// <summary>
		/// 初始化
		/// </summary>
		protected override void OnSingletonInit()
		{
			base.OnSingletonInit();
			SetDefaultSafeArea();
		}
		#endregion

		#region 对外方法
		/// <summary>
		/// 设置RectTransform匹配当前机型的安全区域
		/// </summary>
		/// <param name="rect">需设置的RectTransform</param>
		public void SetRectSafeArea(RectTransform rect)
		{
			rect.anchorMin = min;
			rect.anchorMax = max;
		}
		#endregion

		#region 内部方法
		/// <summary>
		/// 获取当前屏幕默认安全区域数值
		/// </summary>
		private void SetDefaultSafeArea()
		{
			var _lastSafeArea = Screen.safeArea;
			var minX = Screen.width * 0.5f - Mathf.Max(_lastSafeArea.xMin, 0);
			var maxX = Mathf.Min(_lastSafeArea.xMax, Screen.width) - Screen.width * 0.5f;
			var minY = Screen.height * 0.5f - Mathf.Max(_lastSafeArea.yMin, 0);
			var maxY = Mathf.Min(_lastSafeArea.yMax, Screen.height) - Screen.height * 0.5f;

			var m_minX = Screen.width * 0.5f - Mathf.Min(minX, maxX);
			var m_maxX = Screen.width * 0.5f + Mathf.Min(minX, maxX);

			var m_minY = Screen.height * 0.5f - Mathf.Min(minY, maxY);
			var m_maxY = Screen.height * 0.5f + Mathf.Min(minY, maxY);

			_lastSafeArea = new Rect(m_minX, m_minY, m_maxX - m_minX, m_maxY - m_minY);
			min = _lastSafeArea.position;
			max = _lastSafeArea.position + _lastSafeArea.size;
			min.x /= Screen.width;
			min.y /= Screen.height;
			max.x /= Screen.width;
			max.y /= Screen.height;
		}
	}
	#endregion
}
