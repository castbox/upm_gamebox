using System;

namespace GameBox
{
    /// <summary>
    /// 计时元素
    /// </summary>
    public class CalculagraphItem
    {
        /// <summary> 计时ID </summary>
        public string id;
        /// <summary> 事件触发间隔 </summary>
        public double interval_timer;
        /// <summary> 间隔事件 </summary>
        public Action interval_action;
        /// <summary> 循环次数 </summary>
        public long repeat_count;
        /// <summary> 计时运行时长 </summary>
        public double duration;
        /// <summary> 暂停 </summary>
        public bool pause;

        /// <summary>
        /// 只包含结束函数
        /// </summary>
        /// <param name="ID">计时ID</param>
        /// <param name="Interval">计时间隔</param>
        /// <param name="IntervalAction">间隔事件</param>
        public CalculagraphItem(string ID, double Interval, Action IntervalAction)
        {
            id = ID;
            repeat_count = 1;
            interval_timer = Interval;
            duration = 0;
            pause = false;
            interval_action = IntervalAction;
        }

        /// <summary>
        /// 包含间隔函数和结束函数
        /// </summary>
        /// <param name="ID">计时ID</param>
        /// <param name="Repeat">间隔时间循环次数</param>
        /// <param name="Interval">事件触发间隔</param>
        /// <param name="IntervalAction">间隔事件</param>
        public CalculagraphItem(string ID, long Repeat, double Interval, Action IntervalAction) : this(ID, Interval, IntervalAction)
        {
            repeat_count = Repeat;
            interval_timer = Interval;
        }
    }
}

