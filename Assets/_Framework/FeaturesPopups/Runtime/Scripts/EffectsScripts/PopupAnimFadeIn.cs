using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Features.Popups
{
    [System.Serializable]
    public class PopupAnimFadeIn : MonoBehaviour, IShowPhase, IAnimationInitializable
    {
        public CanvasGroup targetCanvasGroup;
        public float startAlpha = 0f;
        public float endAlpha = 1f;
        public float duration = 0.25f;
        public Ease ease = Ease.InOutQuad;

        public void Initialize(Transform target)
        {
            targetCanvasGroup = target.GetComponent<CanvasGroup>();
            if (targetCanvasGroup == null)
            {
                PopupDebugLogger.LogError($"CanvasGroupFadeIn: No CanvasGroup found on target {target.name}");
                return;
            }
            this.startAlpha = 0f;
            this.endAlpha = 1f;
            this.duration = 0.25f;
            this.ease = Ease.InOutQuad;
        }

        public void PreAnimation()
        {
            targetCanvasGroup.alpha = startAlpha;
        }

        public async UniTask PlayAsync()
        {
            await targetCanvasGroup.DOFade(endAlpha, duration).ToUniTask();
        }

        public void AfterAnimation() { }
    }

}