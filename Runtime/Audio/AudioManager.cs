using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBox
{
    /// <summary>
    /// 声音管理器
    /// </summary>
    public class AudioManager: GMonoSingleton<AudioManager>
    {
        private AudioSource _musicSource; // 音乐音源
        private ResManager Res => ResManager.Instance;
        private AudioListener _listener;

        private Queue<SFXSource> _sfxPool;
        private Dictionary<string, AudioClip> _clips;

        #region 开关

        /// <summary>
        /// 声音开关(全)
        /// </summary>
        public bool SoundEnabled
        {
            get => PlayerPrefs.GetInt(nameof(SoundEnabled), 1) == 1;
            set
            {
                PlayerPrefs.SetInt(nameof(SoundEnabled), value ? 1 : 0);
                SFXEnabled = value;
                MusicEnabled = value;
            }
        }
        
        /// <summary>
        /// 音效开关
        /// </summary>
        public bool SFXEnabled
        {
            get => PlayerPrefs.GetInt(nameof(SFXEnabled), 1) == 1;
            set
            {
                PlayerPrefs.SetInt(nameof(SFXEnabled), value ? 1 : 0);
            }
        }

        /// <summary>
        /// 音乐开关
        /// </summary>
        public bool MusicEnabled
        {
            get => PlayerPrefs.GetInt(nameof(MusicEnabled), 1) == 1;
            set
            {
                PlayerPrefs.SetInt(nameof(MusicEnabled), value ? 1 : 0);
                _musicSource.volume = value ? 1 : 0;
                if(value && !_musicSource.isPlaying) _musicSource.Play();
            }
        }

        #endregion
        
        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        protected override void Init()
        {
            _clips = new Dictionary<string, AudioClip>(10);
            _musicSource = gameObject.AddComponent<AudioSource>();

            _listener = GameObject.FindObjectOfType<AudioListener>();
            if (null == _listener) _listener = gameObject.AddComponent<AudioListener>();

            // 预填充
            int count = 3;
            _sfxPool = new Queue<SFXSource>(count);
            for (int i = 0; i < count; i++)
            {
                var ss = CreateSFXSource();
                OnSourceOver(ss);
            }
        }
        
        /// <summary>
        /// 创建音源
        /// </summary>
        /// <returns></returns>
        private SFXSource CreateSFXSource()
        {
            var ss = SFXSource.Create(transform);
            ss.OnPlayOver = OnSourceOver;
            return ss;
        }

        /// <summary>
        /// 获取音源
        /// </summary>
        /// <returns></returns>
        private SFXSource GetSource()
        {
            
            if (_sfxPool.Count > 0)
            {
                var ss = _sfxPool.Dequeue();
                return ss;
            }

            return CreateSFXSource();
        }

        /// <summary>
        /// 音效结束
        /// </summary>
        /// <param name="source"></param>
        private void OnSourceOver(SFXSource source)
        {
            _sfxPool.Enqueue(source);
        }


        #endregion

        #region 资源加载

        /// <summary>
        /// 预加载资源
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        private bool PreLoadBundle(string bundleName)
        {
            var ab = Res.PreLoadBundle(bundleName);
            return ab != null;
        }
        
        /// <summary>
        /// 获取音效组件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        protected virtual AudioClip GetAudioClip(string name, string bundleName = "")
        {
            if (_clips.ContainsKey(name)) return _clips[name];
            
            if (!string.IsNullOrEmpty(bundleName) && !Res.HasBundle(bundleName))
            {
                PreLoadBundle(bundleName);
            }
            
            var clip = Res.LoadAsset<AudioClip>(name, bundleName);
            _clips[name] = clip;
            return clip;
        }

        #endregion
        
        #region 公开接口

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="name"></param>
        /// <param name="bundleName"></param>
        public virtual void PlayMusic(string name, string bundleName = "")
        {
            // 播放音乐前先保存clip
            var clip = GetAudioClip(name, bundleName);
            if (clip == null)
            {
                Debug.Log($"[AM] Load clip fail: {name}:{bundleName}");
                return; 
            }

            StopMusic();
            
            _musicSource.clip = clip;
            _musicSource.loop = true;
            
            if (!MusicEnabled) return;
            _musicSource.Play();
        }

        /// <summary>
        /// 停止音乐
        /// </summary>
        public virtual void StopMusic()
        {
            if (_musicSource.isPlaying)
            {
                _musicSource.Stop();
            }
        }


        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="name"></param>
        /// <param name="bundleName"></param>
        public virtual void PlaySound(string name, string bundleName = "")
        {
            if (!SFXEnabled) return;
            
            var clip = GetAudioClip(name, bundleName);
            if (clip == null)
            {
                Debug.Log($"[AM] Load clip fail: {name}:{bundleName}");
                return; 
            }
            var ss = GetSource();
            ss.Play(clip);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            _clips.Clear();
            _clips = null;

            while (_sfxPool.Count > 0)
            {
                var sfx = _sfxPool.Dequeue();
                Destroy(sfx.gameObject);
            }
            _sfxPool = null;
            
        }
        #endregion

    }

    #region 音效音源
    /// <summary>
    /// 内部SFX音源对象
    /// </summary>
    internal class SFXSource: MonoBehaviour
    {

        private static int _childIndex = 1;
        
        public static SFXSource Create(Transform parent = null)
        {
            var go = new GameObject($"sfx_source_{_childIndex}");
            _childIndex++;
            if(parent) go.transform.SetParent(parent);
            var ss = go.AddComponent<SFXSource>();
            ss.Init();
            return ss;
        }


        public Action<SFXSource> OnPlayOver;
        
        private AudioSource _source;
        public AudioClip Clip
        {
            get => _source.clip;
            set => _source.clip = value;
        }


        public float Volume
        {
            get => _source.volume;
            set => _source.volume = value;
        }

        public bool Active
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        public void Init()
        {
            _source = gameObject.AddComponent<AudioSource>();
        }

        public void Play(AudioClip clip, float delay = 0)
        {
            StartCoroutine(OnPlayClip(clip, delay));
        }

        private IEnumerator OnPlayClip(AudioClip clip, float delay = 0)
        {
            // Active = true;
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            _source.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
            // Active = false;
            OnPlayOver?.Invoke(this);
        }



    }
    #endregion
}