using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Features.Popups.Animations
{
    public class PopupAnimHideMoveRight : PopupAnimMoveBase, IHidePhase, IAnimationInitializable
    {
        private Vector3 origin;

        public void Initialize(Transform target)
        {
            this.target = target as RectTransform;
            if (this.target != null)
                origin = this.target.localPosition;
        }

        public void PreAnimation() { }

        public async UniTask PlayAsync()
        {
            if (target == null) return;
            origin = target.localPosition;
            Vector3 offscreen = GetOffscreenTarget(origin);

            await target.DOLocalMove(offscreen, duration)
                        .SetEase(ease)
                        .SetDelay(delay)
                        .ToUniTask();
        }

        public void AfterAnimation() { }

        protected override Vector3 GetDirection() => Vector3.right;
    }
}
