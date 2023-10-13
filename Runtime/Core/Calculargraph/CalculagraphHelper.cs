using System.Collections.Generic;
using UnityEngine;
using System;
namespace GameBox
{
    /// <summary>
    /// 计时器
    /// </summary>
    public class CalculagraphHelper : GMonoSingleton<CalculagraphHelper>
    {
        /// <summary> 计时元素列表 </summary>
        private List<CalculagraphItem> calculagraph_list;

        /// <summary> 需移除计时元素的ID集合 </summary>
        private List<string> remove_calculapraph_id_list;

        protected override void Init()
        {
            base.Init();
            calculagraph_list = new List<CalculagraphItem>();
            remove_calculapraph_id_list = new List<string>();
        }
        /// <summary>
        /// 添加计时元素(单次计时)
        /// </summary>
        /// <param name="ID">计时ID</param>
        /// <param name="Interval">计时间隔</param>
        /// <param name="EndAction">结束事件</param>
        public void AddCalulagraph(string ID, double Interval, Action EndAction)
        {
            calculagraph_list.Add(new CalculagraphItem(ID, Interval, EndAction));
        }

        /// <summary>
        /// 添加计时元素
        /// </summary>
        /// <param name="ID">计时ID</param>
        /// <param name="Repeat">间隔时间循环次数</param>
        /// <param name="Interval">事件触发间隔</param>
        /// <param name="IntervalAction">间隔事件</param>
        public void AddCalulagraph(string ID, long Repeat, double Interval, Action IntervalAction)
        {
            calculagraph_list.Add(new CalculagraphItem(ID, Repeat, Interval, IntervalAction));
        }

        private void Update()
        {
            if (calculagraph_list == null || calculagraph_list.Count == 0)
                return;

            for (int i = 0; i < calculagraph_list.Count; i++)
            {
                CalculagraphItem item = calculagraph_list[i];
                if (item == null || item.pause)
                    continue;

                item.duration += Time.deltaTime;

                //触发间隔事件
                if (item.duration >= item.interval_timer)
                {
                    item.duration = 0;

                    if (item.repeat_count > 0)
                        item.repeat_count--;

                    item.interval_action?.Invoke();

                    if (item.repeat_count == 0)
                        remove_calculapraph_id_list.Add(item.id);

                }
            }

            if (remove_calculapraph_id_list.Count > 0)
            {
                remove_calculapraph_id_list.ForEach(remove_id => RemoveCalculapraph(remove_id));
                remove_calculapraph_id_list.Clear();
            }
        }

        /// <summary>
        /// 移除计时元素
        /// </summary>
        /// <param name="ID">计时ID</param>
        /// <param name="InvokeEndAction">是否触发结束事件</param>
        public void RemoveCalculapraph(string ID)
        {
            if (calculagraph_list != null && calculagraph_list.Count > 0)
            {
                CalculagraphItem item = calculagraph_list.Find(c => c.id == ID);
                if (item != null)
                    calculagraph_list.Remove(item);
            }
        }

        /// <summary>
        /// 暂停/恢复 指定计时元素
        /// </summary>
        /// <param name="ID">指定计时元素ID</param>
        /// <param name="pause">暂停/恢复</param>
        public void PauseCalculapraph(string ID, bool pause)
        {
            if (calculagraph_list != null && calculagraph_list.Count > 0)
            {
                CalculagraphItem item = calculagraph_list.Find(c => c.id == ID);
                if (item != null)
                    item.pause = pause;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            calculagraph_list.Clear();
            remove_calculapraph_id_list.Clear();
        }
    }
}
