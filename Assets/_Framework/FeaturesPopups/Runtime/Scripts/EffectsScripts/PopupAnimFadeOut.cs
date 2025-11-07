using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Features.Popups
{
    public class PopupAnimFadeOut : MonoBehaviour, IHidePhase, IAnimationInitializable
    {
        public CanvasGroup targetCanvasGroup;
        public float startAlpha = 1;
        public float endAlpha = 0;
        public float duration = 0.25f;
        public Ease ease = Ease.InOutQuad;

        public void Initialize(Transform target)
        {
            targetCanvasGroup = target.GetComponent<CanvasGroup>();
            if (targetCanvasGroup == null)
            {
                PopupDebugLogger.LogError($"CanvasGroupFadeOut: No CanvasGroup found on target {target.name}");
                return;
            }

            this.startAlpha = targetCanvasGroup.alpha;
            this.endAlpha = 0;
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
