#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

namespace Features.Popups.EditorTools
{
    /// <summary>
    /// 🎨 Hiển thị nút mở Popup Animation Tool trong Inspector.
    /// Dành cho các popup không phụ thuộc vào data (PopupBehaviour).
    /// </summary>
    [CustomEditor(typeof(PopupBehaviour), true)]
    public class PopupBehaviourEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Vẽ phần Inspector mặc định
            DrawDefaultInspector();

            //Lấy tham chiếu đến PopupBehaviour hiện tại
            var popup = (PopupBehaviour)target;
            bool inPrefabMode = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null;

            // Vẽ phần giao diện tùy chỉnh
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Nút mở Popup Animation Tool
            using (new EditorGUI.DisabledScope(!inPrefabMode))
            {
                GUI.backgroundColor = inPrefabMode ? new Color(0.25f, 0.75f, 1f) : Color.gray;

                if (GUILayout.Button("🔧 Open Popup Animation Tool", GUILayout.Height(30)))
                {
                    var window = PopupAnimationWindow.OpenWindow();
                    if (popup != null)
                    {
                        window.Context.BindTo(popup.gameObject);
                        window.Focus();
                    }
                }

                GUI.backgroundColor = Color.white;
            }

            if (!inPrefabMode)
                EditorGUILayout.HelpBox("Open this prefab in Prefab Mode to edit animations.", MessageType.Info);
        }
    }
}
#endif
