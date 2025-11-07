using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Features.Popups
{
    public class PopupAnimScaleIn : MonoBehaviour, IShowPhase, IAnimationInitializable
    {
        public Transform target;
        public float startScale = 0;
        public float endScale = 1;
        public float duration = 0.25f;
        public Ease ease = Ease.InOutQuad;

        public void Initialize(Transform target)
        {
            this.target = target;
            if(this.target == null)
            {
                PopupDebugLogger.LogError($"ScaleAnimator: Target transform is null during initialization.");
                return;
            }

            this.startScale = 0;
            this.endScale = 1;
            this.duration = 0.25f;
            this.ease = Ease.InOutQuad;
        }

        public void OnAfterShowAsync() { }

        public void PreAnimation()
        {
            target.localScale = Vector3.one * startScale;
        }

        public async UniTask PlayAsync()
        {
            await target.DOScale(endScale, duration).ToUniTask();
        }

        public void AfterAnimation() { }
    }

}
