using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Popups.Demo
{
    public class PopupExamEmpty : PopupBase<object>
    {
        [SerializeField] private Button btnClosePopup;

        private void Start()
        {
            btnClosePopup.onClick.AddListener(OnClickClosePopup);
        }
        private void OnDestroy()
        {
            btnClosePopup.onClick.RemoveListener(OnClickClosePopup);
        }

        public void OnClickClosePopup()
        {
            PopupManager.Instance.HidePopup<PopupExamEmpty>().Forget();
        }
    }
}