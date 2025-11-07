using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Features.Popups.Animations
{
    public class PopupAnimShowMoveRight : PopupAnimMoveBase, IShowPhase, IAnimationInitializable
    {
        private Vector3 origin;

        public void Initialize(Transform target)
        {
            this.target = target as RectTransform;
            if (this.target != null)
                origin = this.target.localPosition;
        }

        public void PreAnimation()
        {
            if (target == null) return;
            origin = target.localPosition;
            target.localPosition = GetOffscreenTarget(origin) * -1f;
        }

        public async UniTask PlayAsync()
        {
            if (target == null) return;

            await target.DOLocalMove(origin, duration)
                        .SetEase(ease)
                        .SetDelay(delay)
                        .ToUniTask();
        }

        public void AfterAnimation() { }

        protected override Vector3 GetDirection() => Vector3.right;
    }
}
