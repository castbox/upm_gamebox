using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBox
{
    /// <summary>
    /// 下载任务接口
    /// </summary>
    public interface IDownloadTask
    {
        /// <summary> 从本地读取 </summary>
        IEnumerator ReadFromLocal();
        /// <summary> 从服务器读取 </summary>
        IEnumerator DownloadFromAzure();
    }
}
