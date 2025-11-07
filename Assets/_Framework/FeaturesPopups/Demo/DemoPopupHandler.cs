using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Features.Popups.Demo
{
    public class DemoPopupHandler : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DemoPopupManager.Instance.ShowPopup<PopupExamEmpty, object>(null).Forget();
            }
        }
    }
}