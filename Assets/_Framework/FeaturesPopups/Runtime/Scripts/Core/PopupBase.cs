using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Features.Popups
{
    public abstract class PopupBase<TData> : PopupBehaviour
    {
        protected TData data;
        [SerializeField] protected PopupShowSequence showSequence;
        [SerializeField] protected PopupHideSequence hideSequence;

        public async UniTask Show(TData data)
        {
            PopupDebugLogger.Log($"[PopupBase] Hàm Show được gọi với dữ liệu: {data}");

            SetStatePopup(PopupState.Init);

            this.data = data;

            SetStatePopup(PopupState.Showing);
            await InternalShowAsync();
            SetStatePopup(PopupState.Shown);
        }

        protected virtual async UniTask InternalShowAsync()
        {
            PopupDebugLogger.Log("[PopupBase] InternalShowAsync bắt đầu.");

            await SetupOnBeforeShow();
            showSequence.PreAnimation();
            await showSequence.PlayAsync();
            showSequence.AfterAnimation();
            await OnShowCompleted();

            PopupDebugLogger.Log("[PopupBase] InternalShowAsync hoàn thành.");
        }

        public async UniTask Hide()
        {
            PopupDebugLogger.Log("[PopupBase] Hàm Hide được gọi.");

            SetStatePopup(PopupState.Hiding);
            await InternalHideAsync();
            SetStatePopup(PopupState.Hidden);

            PopupDebugLogger.Log("[PopupBase] Hàm Hide hoàn thành.");
        }

        protected virtual async UniTask InternalHideAsync()
        {
            PopupDebugLogger.Log("[PopupBase] InternalHideAsync bắt đầu.");

            await SetupOnBeforHide();
            hideSequence.PreAnimation();
            await hideSequence.PlayAsync();
            hideSequence.AfterAnimation();
            await OnHideComplete();

            PopupDebugLogger.Log("[PopupBase] InternalHideAsync hoàn thành.");
        }

        protected virtual UniTask SetupOnBeforeShow() => UniTask.CompletedTask;
        protected virtual UniTask OnShowCompleted() => UniTask.CompletedTask;
        protected virtual UniTask SetupOnBeforHide() => UniTask.CompletedTask;
        protected virtual UniTask OnHideComplete() => UniTask.CompletedTask;
        public virtual UniTask ClosePopup() => UniTask.CompletedTask;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            // Đảm bảo showSequence và hideSequence được thiết lập chính xác
            EnsureAnimationSequence(ref showSequence);
            EnsureAnimationSequence(ref hideSequence);
        }

        private void EnsureAnimationSequence<T>(ref T sequence) where T : Component
        {
            // Nếu đã có component, không cần làm gì thêm
            if (sequence != null) return;

            // Tìm component đầu tiên thuộc kiểu T
            sequence = GetComponent<T>();

            // Nếu không tìm thấy, thêm mới
            if (sequence == null)
            {
                sequence = gameObject.AddComponent<T>();
                Debug.Log($"[PopupBase] Đã thêm component mặc định loại {typeof(T).Name} vào {name}");
            }
        }
#endif

    }
}