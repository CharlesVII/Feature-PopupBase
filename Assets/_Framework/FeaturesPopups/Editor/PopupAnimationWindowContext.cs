#if UNITY_EDITOR
using UnityEngine;

namespace Features.Popups.EditorTools
{
    public class PopupAnimationWindowContext
    {
        public GameObject CurrentPrefab { get; private set; }
        public Transform SelectedTransform { get; set; }
        public void BindTo(GameObject prefab)
        {
            CurrentPrefab = prefab;
            SelectedTransform = null;
        }

        public void Unbind()
        {
            CurrentPrefab = null;
            SelectedTransform = null;
        }
    }
}
#endif
