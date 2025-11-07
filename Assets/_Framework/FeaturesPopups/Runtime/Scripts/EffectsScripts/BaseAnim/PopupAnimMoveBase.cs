using DG.Tweening;
using UnityEngine;

namespace Features.Popups.Animations
{
    /// <summary>
    /// 🧱 Base cho các hiệu ứng di chuyển (MoveIn / MoveOut)
    /// Dùng chung cho các hướng khác nhau, có hỗ trợ thêm khoảng cách offset.
    /// </summary>
    public abstract class PopupAnimMoveBase : MonoBehaviour
    {
        [Header("Movement Settings")]
        public RectTransform target;
        public float additionalDistance = 0;
        public float duration = 0.25f;
        public float delay = 0f;
        public Ease ease = Ease.InOutQuad;

        protected abstract Vector3 GetDirection();

        protected Vector3 GetOffscreenTarget(Vector3 origin)
        {
            Vector3 dir = GetDirection();
            Vector3 screenOffset = new Vector3(Screen.width * dir.x, Screen.height * dir.y, 0);
            return origin + screenOffset + dir * additionalDistance;
        }
    }
}
