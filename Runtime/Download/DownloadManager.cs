using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GameBox
{
    /// <summary>
    /// 下载管理器
    /// </summary>
    public class DownloadManager : GMonoSingleton<DownloadManager>
    {
        /// <summary> 下载列表 </summary>
        private List<DownloadInfo> _downloadInfos;

        protected override void Init()
        {
            base.Init();
            _downloadInfos = new List<DownloadInfo>();
        }

        /// <summary>
        /// 添加下载任务
        /// </summary>
        /// <param name="download_info"></param>
        public void AddDownload(DownloadInfo download_info)
        {
            if (!string.IsNullOrEmpty(download_info.LocalPath))
            {
                download_info.StreamingPath = download_info.LocalPath.ToString();
                download_info.LocalPath = Path.Combine(Application.persistentDataPath, download_info.LocalPath, download_info.ID);
                download_info.StreamingPath = Path.Combine(Application.streamingAssetsPath, download_info.StreamingPath, download_info.ID);
            }
            AddDownloadInfo(download_info);
        }

        /// <summary>
        /// 添加下载任务
        /// </summary>
        /// <param name="download_info"></param>
        private void AddDownloadInfo(DownloadInfo download_info)
        {
            DownloadInfo download = _downloadInfos.Find(d => d.ID == download_info.ID && d.AssetType == download_info.AssetType && d.Style == download_info.Style);
            if (download == null)
            {
                download_info.State = DownloadState.Ready;
                download_info.SuccessAction += (asset) => Run();
                _downloadInfos.Add(download_info);
            }
            else if (download.State == DownloadState.Completed)
                download_info.SuccessAction?.Invoke(download.Asset);

            Run();
        }

        /// <summary>
        /// 运行任务
        /// </summary>
        public void Run()
        {
            if (_downloadInfos.Where(d => d.State == DownloadState.Running).Count() < 5)
            {
                DownloadInfo current_download = _downloadInfos.Find(d => d.State == DownloadState.Ready);
                if (current_download != null)
                    StartDownload(current_download);
            }
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="info"></param>
        private void StartDownload(DownloadInfo info)
        {
            info.State = DownloadState.Running;
            IDownloadTask task = null;
            switch (info.AssetType)
            {
                case AssetType.AssetBundle:
                    task = new AssetBundleTask(info);
                    break;
                case AssetType.Texture:
                    task = new TextureTask(info);
                    break;
                case AssetType.Content:
                    task = new ContentTask(info);
                    break;
                case AssetType.ZipFile:
                    task = new ZipFileTask(info);
                    break;
            }

            switch (info.Style)
            {
                case DownloadStyle.ReadOrDownload:
                    if (info.FailedAction == null)
                        info.FailedAction = () => StartCoroutine(task.DownloadFromAzure());
                    else
                        info.FailedAction = () => _downloadInfos.Remove(info);

                    StartCoroutine(task.ReadFromLocal());
                    break;
                case DownloadStyle.Download:
                    if (info.FailedAction == null)
                        info.FailedAction = () => StartCoroutine(task.DownloadFromAzure());
                    else
                        info.FailedAction = () => _downloadInfos.Remove(info);

                    StartCoroutine(task.DownloadFromAzure());
                    break;
                case DownloadStyle.Read:
                    if (info.FailedAction == null)
                        info.FailedAction = () => StartCoroutine(task.ReadFromLocal());
                    else
                        info.FailedAction = () => _downloadInfos.Remove(info);

                    StartCoroutine(task.ReadFromLocal());
                    break;
            }
        }
    }
}