#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Features.Popups.EditorTools
{
    public class PopupAnimationWindow : EditorWindow
    {
        public PopupAnimationWindowContext Context { get; private set; }
        private PopupAnimationWindowView view;

        [MenuItem("Tools/Popup Animation Editor")]
        public static PopupAnimationWindow OpenWindow()
        {
            var window = GetWindow<PopupAnimationWindow>("Popup Animation Editor");
            window.Init();
            return window;
        }

        private void Init()
        {
            Context = new PopupAnimationWindowContext();
            view = new PopupAnimationWindowView(Context);
            PopupEditorLinker.Initialize(Context);
        }

        private void OnGUI()
        {
            view?.DrawGUI();
        }

        private void OnDisable()
        {
            PopupEditorLinker.Dispose();
        }
    }
}
#endif

