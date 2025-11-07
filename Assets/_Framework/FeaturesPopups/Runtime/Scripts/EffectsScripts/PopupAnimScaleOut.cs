using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Features.Popups
{
    public class PopupAnimScaleOut : MonoBehaviour, IHidePhase, IAnimationInitializable
    {
        public Transform target;
        public float startScale = 0;
        public float endScale = 1;
        public float duration = 0.25f;
        public Ease ease = Ease.InOutQuad;

        public void Initialize(Transform target)
        {
            this.target = target;
            if (this.target == null)
            {
                PopupDebugLogger.LogError($"SceneAnimatorOut: Target transform is null during initialization.");
                return;
            }
            this.startScale = 1;
            this.endScale = 0;
            this.duration = 0.25f;
            this.ease = Ease.InOutQuad;
        }

        public void PreAnimation() { }

        public async UniTask PlayAsync()
        {
            await target.DOScale(endScale, duration).ToUniTask();
        }

        public void AfterAnimation()
        {
            this.target.localScale = Vector3.one * startScale;
        }
    }
}