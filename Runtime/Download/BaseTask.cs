using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace GameBox
{
    /// <summary>
    /// 任务
    /// </summary>
    public abstract class BaseTask<T> : IDownloadTask
    {
        /// <summary> 下载任务的相关数据 </summary>
        protected DownloadInfo Info;

        /// <summary> 加载到的资源 </summary>
        protected T DefaultT;

        /// <summary> 下载错误次数 </summary>
        private int DelayNumber;

        public BaseTask(DownloadInfo info)
        {
            Info = info;
        }

        /// <summary>
        /// 获取重新下载的时间间隔
        /// </summary>
        /// <returns></returns>
        public WaitForSeconds GetWaitForSeconds()
        {
            DownloadManager.Instance.Run();
            var delay = Mathf.Min(Mathf.Pow(3, ++DelayNumber) + 12, 300);
            Debug.LogFormat("当前下载出错, 将在等待{0}秒后重新下载...", delay);
            return new WaitForSeconds(delay);
        }

        public virtual IEnumerator ReadFromLocal()
        {
            yield return null;
        }

        public virtual IEnumerator DownloadFromAzure()
        {
            yield return null;
        }

        /// <summary>
        /// 激活订阅事件
        /// </summary>
        /// <param name="asset"></param>
        protected void DownloadComplete(T asset)
        {
            if (asset != null)
            {
                DelayNumber = 0;
                Info.State = DownloadState.Completed;
                Info.SuccessAction?.Invoke(asset);
                Info.SetAsset(asset);
            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        protected void SaveBytes(string path, byte[] bytes)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            if (File.Exists(path)) File.Delete(path);
            File.WriteAllBytes(path, bytes);
        }
    }
}
