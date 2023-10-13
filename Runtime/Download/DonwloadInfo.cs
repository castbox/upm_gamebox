using System;
using System.Collections.Generic;

namespace GameBox
{
    /// <summary>
    /// 下载任务的相关数据
    /// </summary>
    public class DownloadInfo
    {
        /// <summary> 下载识别ID </summary>
        public string ID;

        /// <summary> 本地地址 </summary>
        public string LocalPath;

        /// <summary> 资源地址 </summary>
        public string StreamingPath;

        /// <summary> 服务器地址 </summary>
        public string NetPath;

        /// <summary> 加载方式 </summary>
        public DownloadStyle Style;

        /// <summary> 下载状态 </summary>
        public DownloadState State;

        /// <summary> 资源类型 </summary>
        public AssetType AssetType;

        /// <summary> 加载成功回调 </summary>
        public Action<object> SuccessAction;

        /// <summary> 加载失败回调 </summary>
        public Action FailedAction;

        /// <summary> 加载到的资源 </summary>
        public Object Asset;

        public DownloadInfo(string id, DownloadStyle style, AssetType asset_type, string local_path = null, string net_path = null, Action<object> action = null, Action fail_action = null)
        {
            ID = id;
            Style = style;
            AssetType = asset_type;
            SuccessAction = action;
            LocalPath = local_path;
            NetPath = net_path;
            FailedAction = fail_action;
        }

        /// <summary>
        /// 暂存加载的资源
        /// </summary>
        /// <param name="asset"></param>
        public void SetAsset(Object asset)
        {
            Asset = asset;
        }
    }
}