using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace GameBox
{
    /// <summary>
    /// 下载图片[实现了图片缓存]
    /// </summary>
    public class TextureTask : BaseTask<Sprite>
    {
        public TextureTask(DownloadInfo info) : base(info)
        {
        }

        public override IEnumerator ReadFromLocal()
        {
            if (File.Exists(Info.LocalPath))
            {
                Debug.LogFormat("本地读取: {0}", Info.LocalPath);
                var texture = LoadByIo(Info.LocalPath);
                if (texture != null)
                {
                    DownloadComplete(ToSprite(texture));
                    yield return null;
                }
                else
                {
                    yield return GetWaitForSeconds();
                    Info.FailedAction?.Invoke();
                }
            }
            else if (File.Exists(Info.StreamingPath))
            {
                UnityWebRequest Request = null;
                Debug.LogFormat("Streaming下读取: {0}", Info.StreamingPath);
                Request = UnityWebRequestTexture.GetTexture(Info.StreamingPath);
                if (Request.result == UnityWebRequest.Result.Success)
                {
                    Debug.LogFormat("加载成功: {0}", Info.ID);
                    var texture = DownloadHandlerTexture.GetContent(Request);
                    DownloadComplete(ToSprite(texture));
                }
            }
            else
            {
                Debug.Log($"本地不存在:{Info.LocalPath}");
                yield return null;
                Info.FailedAction?.Invoke();
            }
        }

        /// <summary>
        /// 使用IO读取图片
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private Texture2D LoadByIo(string path)
        {
            //创建文件读取流
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            //创建文件长度缓冲区
            byte[] bytes = new byte[fileStream.Length];
            //读取文件
            fileStream.Read(bytes, 0, (int)fileStream.Length);

            //释放文件读取流
            fileStream.Close();
            //释放本机屏幕资源
            fileStream.Dispose();
            fileStream = null;

            //创建Texture
            int width = 1200;
            int height = 1200;
            Texture2D texture = new Texture2D(width, height);
            if (texture.LoadImage(bytes))
                return texture;
            else
                return null;
        }

        public override IEnumerator DownloadFromAzure()
        {
            Debug.LogFormat("从服务器加载: {0}", Info.ID);
            var request = UnityWebRequestTexture.GetTexture(Info.NetPath);
            yield return request.SendWebRequest();
            if (request.isDone)
            {
                if (!(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError))
                {
                    Debug.Log($"下载成功: {Info.ID}");
                    var texture = DownloadHandlerTexture.GetContent(request);
                    //通知客户端
                    DownloadComplete(ToSprite(texture));
                    if (!string.IsNullOrEmpty(Info.LocalPath))
                    {
                        //保存数据
                        try
                        {
                            if (File.Exists(Info.LocalPath))
                            {
                                Debug.Log($"存在对应资源:{Info.LocalPath}");
                                File.Delete(Info.LocalPath);
                            }
                            SaveBytes(Info.LocalPath, texture.EncodeToPNG());
                        }
                        catch (System.Exception ex)
                        {
                            Debug.Log($"存储图片资源失败:{ex.Message}");
                        }
                    }
                }
                else
                {
                    Debug.Log($"下载失败: {request.error}");
                    //销毁
                    request.Dispose();
                    Info.State = DownloadState.Wait;
                    yield return GetWaitForSeconds();
                    Info.State = DownloadState.Running;
                    Info.FailedAction?.Invoke();
                }
            }
        }

        /// <summary>
        /// 转化成图片精灵
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Sprite ToSprite(Texture2D source)
        {
            if (source == null) return null;
            return Sprite.Create(source, new Rect(0, 0, source.width, source.height), new Vector2(0.5f, 0.5f));
        }
    }
}
