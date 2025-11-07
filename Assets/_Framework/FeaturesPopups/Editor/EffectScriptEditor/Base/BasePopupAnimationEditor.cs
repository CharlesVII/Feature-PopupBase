#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Features.Popups.EditorTools
{
    /// <summary>
    /// ⚙️ Base Editor cho mọi loại animation
    /// - Không giả định có field cụ thể nào (an toàn, tái sử dụng)
    /// - Cho phép layout ngang khi ở ToolView, dọc ở Inspector
    /// - Subclass có thể tự chọn field, tự vẽ preview hoặc không
    /// </summary>
    public abstract class BasePopupAnimationEditor<T> : Editor where T : MonoBehaviour
    {
        /// <summary>True nếu đang được vẽ trong ToolView thay vì Inspector gốc.</summary>
        protected bool InTool => PopupAnimationWindowView.IsDrawingInTool;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (InTool)
                DrawHorizontalLayout((T)target);
            else
                DrawVerticalLayout((T)target);

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }

        // ============================================================
        // 🧱 Layout mặc định — subclass override nếu cần
        // ============================================================
        protected virtual void DrawHorizontalLayout(T anim)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            DrawInspectorContent(anim, true);
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawVerticalLayout(T anim)
        {
            EditorGUILayout.Space(2);
            DrawInspectorContent(anim, false);
        }

        // ============================================================
        // 🧩 Nơi subclass vẽ nội dung
        // ============================================================
        /// <param name="horizontal">True nếu đang vẽ trong ToolView</param>
        protected abstract void DrawInspectorContent(T anim, bool horizontal);

        // ============================================================
        // 🔹 Tiện ích vẽ field nếu có
        // ============================================================
        protected void DrawPropertyIfExist(string name, GUIContent label, params GUILayoutOption[] options)
        {
            var prop = serializedObject.FindProperty(name);
            if (prop != null)
                EditorGUILayout.PropertyField(prop, label, options);
        }

        protected void DrawPropertyIfExist(string name, params GUILayoutOption[] options)
        {
            DrawPropertyIfExist(name, GUIContent.none, options);
        }
    }
}
#endif
