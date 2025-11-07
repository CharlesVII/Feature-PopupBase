#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Features.Popups.EditorTools
{
    public static class PopupEditorLinker
    {
        private static PopupAnimationWindowContext context;

        public static void Initialize(PopupAnimationWindowContext ctx)
        {
            context = ctx;
            PrefabStage.prefabStageOpened += OnPrefabOpened;
            Selection.selectionChanged += OnSelectionChanged;
        }

        public static void Dispose()
        {
            PrefabStage.prefabStageOpened -= OnPrefabOpened;
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private static void OnPrefabOpened(PrefabStage stage)
        {
            context?.BindTo(stage.prefabContentsRoot);
        }

        private static void OnSelectionChanged()
        {
            if (context == null) return;
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) return;
            var go = Selection.activeGameObject;
            if (go != null)
                context.BindTo(go);
        }
    }
}
#endif
