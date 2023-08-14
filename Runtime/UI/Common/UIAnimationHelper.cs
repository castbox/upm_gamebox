using System;
using DG.Tweening;
using UnityEngine;

namespace GameBox
{

    public enum UIAnimationType
    {
        RightIn,
        RightOut,
        LeftIn,
        LeftOut,
        BotIn,
        BotOut,
        TopIn,
        TopOut,
    }


    public static class ViewAnimationHelper
    {

        private const float TIME_MOVE_IN = 0.4f;
        private const float TIME_MOVE_OUT = 0.25f;
            
        
        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="root"></param>
        /// <param name="animType"></param>
        /// <param name="callback"></param>
        /// <param name="duration"></param>
        public static void PlayUIAnim(this Transform root, UIAnimationType animType, Action callback = null, float duration = 0)
        {
            root.DOKill();
            
            Vector3 startPos = Vector3.zero;
            Vector3 endPos = Vector3.zero;
            if (duration == 0) duration = 0.45f;
            Ease easeType = Ease.OutSine;

            float t = 0;
            switch (animType)
            {
                
                //----- In Animation --------
                case UIAnimationType.RightIn:
                    startPos.x = Screen.width;
                    t = TIME_MOVE_IN;
                    easeType = Ease.OutSine;
                    break;
                
                case UIAnimationType.LeftIn:
                    startPos.x = -Screen.width;
                    t = TIME_MOVE_IN;
                    easeType = Ease.OutSine;
                    break;
                
                case UIAnimationType.BotIn:
                    startPos.y = -Screen.height;
                    t = TIME_MOVE_IN;
                    easeType = Ease.OutSine;
                    break;
                
                case UIAnimationType.TopIn:
                    startPos.y = Screen.height;
                    t = TIME_MOVE_IN;
                    easeType = Ease.OutSine;
                    break;
                
                //----- Out Animation --------
                case UIAnimationType.RightOut:
                    endPos.x = Screen.width;
                    t = TIME_MOVE_OUT;
                    easeType = Ease.InSine;
                    break;
                
                case UIAnimationType.LeftOut:
                    endPos.x = -Screen.width;
                    t = TIME_MOVE_OUT;
                    easeType = Ease.InSine;
                    break;
                
                case UIAnimationType.BotOut:
                    endPos.y = -Screen.height;
                    t = TIME_MOVE_OUT;
                    easeType = Ease.InSine;
                    break;
                
                case UIAnimationType.TopOut:
                    endPos.y = Screen.height;
                    t = TIME_MOVE_OUT;
                    easeType = Ease.InSine;
                    break;
            }


            if (duration == 0) duration = t;
            root.localPosition = startPos;
            root.DOLocalMove(endPos, duration)
                .SetEase(easeType).OnComplete(() =>
            {
                callback?.Invoke();
            });

        }
    }
}