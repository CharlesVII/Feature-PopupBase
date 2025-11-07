using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Features.Popups
{
    public enum PopupState
    {
        None = 0,                // Trạng thái mặc định, không xác định
        Init = 1,                // Popup đang khởi tạo
        Showing = 2,             // Popup đang chạy animation hiển thị
        Shown = 3,               // Popup đã hiển thị và sẵn sàng tương tác
        Hiding = 4,              // Popup đang chạy animation ẩn
        Hidden = 5,              // Popup đã bị ẩn nhưng vẫn tồn tại trong bộ nhớ
        Destroyed = 6            // Popup đã bị hủy và không còn tồn tại
    }

    public abstract class PopupBehaviour : MonoBehaviour
    {
        [SerializeField] protected PopupState popupState = PopupState.None;

        public PopupState PopupState => popupState;

        public virtual void ResetPopupBehaviour() { }

        public void SetStatePopup(PopupState state)
        {
            popupState = state;
        }
    }
}