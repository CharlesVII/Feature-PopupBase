#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace Features.Popups.EditorTools
{
    [InitializeOnLoad]
    public class PopupAnimationHierarchyEditor
    {
        private static PopupAnimationLogic logic = new PopupAnimationLogic(null);

        static PopupAnimationHierarchyEditor()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }
        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null) return;

            // ✅ Chỉ hiển thị nếu object nằm trong popup (child)
            var popupBase = FindPopupBase(go.transform);
            if (popupBase == null) return;
            if (go.GetComponent(popupBase.GetType()) != null)
                return;

            float btnWidth = 30;
            float spacing = 2f;
            float totalWidth = (btnWidth + spacing) * 5; // thêm 1 nút remove
            float startX = selectionRect.xMax - totalWidth - 10f;
            float y = selectionRect.y + 1f;
            float h = selectionRect.height - 2f;

            GUIStyle mini = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleCenter
            };

            // ----------------------
            // + Show Sequence
            // ----------------------
            GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
            if (GUI.Button(new Rect(startX, y, btnWidth, h), "SqS", mini))
                logic.ShowAddSequenceMenu(go.transform);

            // ----------------------
            // + Hide Sequence
            // ----------------------
            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
            if (GUI.Button(new Rect(startX + (btnWidth + spacing), y, btnWidth, h), "SqH", mini))
                logic.ShowAddSequenceMenu(go.transform);

            // ----------------------
            // + Show Animation
            // ----------------------
            GUI.backgroundColor = new Color(0.6f, 0.9f, 1f);
            if (GUI.Button(new Rect(startX + (btnWidth + spacing) * 2, y, btnWidth, h), "aS", mini))
                logic.ShowAddMenu(go.transform, true);

            // ----------------------
            // + Hide Animation
            // ----------------------
            GUI.backgroundColor = new Color(1f, 0.8f, 0.6f);
            if (GUI.Button(new Rect(startX + (btnWidth + spacing) * 3, y, btnWidth, h), "aH", mini))
                logic.ShowAddMenu(go.transform, false);

            // ----------------------
            // 🔁 Change Animation
            // ----------------------
            GUI.backgroundColor = new Color(0.6f, 0.6f, 1f);
            if (GUI.Button(new Rect(startX + (btnWidth + spacing) * 4, y, btnWidth, h), "Ch", mini))
            {
                var goComp = go.GetComponent<MonoBehaviour>();
                if (goComp != null)
                {
                    GenericMenu menu = new GenericMenu();
                    bool isShow = goComp is IShowPhase;
                    var allAnim = logic.GetAllAnimationTypes(isShow);
                    foreach (var type in allAnim)
                    {
                        if (type == goComp.GetType()) continue;
                        menu.AddItem(new GUIContent(type.Name), false, () =>
                        {
                            logic.ReplaceAnimation(goComp, type, isShow);
                            EditorWindow.GetWindow<PopupAnimationWindow>()?.Repaint();
                        });
                    }
                    menu.ShowAsContext();
                }
            }

            // ----------------------
            // 🗑 Remove All
            // ----------------------
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUI.Button(new Rect(startX + (btnWidth + spacing) * 5, y, btnWidth, h), "✕", mini))
            {
                if (EditorUtility.DisplayDialog(
                    "Remove All Animations",
                    $"Remove all Show/Hide animations and sequences under '{go.name}'?",
                    "Yes", "Cancel"))
                {
                    logic.ClearAnimationsForNode(go.transform);
                    EditorWindow.GetWindow<PopupAnimationWindow>()?.Repaint();
                }
            }

            GUI.backgroundColor = Color.white;
        }

        // ===============================================================
        // Helper: tìm PopupBase
        // ===============================================================
        private static MonoBehaviour FindPopupBase(Transform target)
        {
            var monos = target.GetComponentsInParent<MonoBehaviour>(true);
            foreach (var m in monos)
            {
                if (m == null) continue;
                var t = m.GetType();
                while (t != null)
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition().Name.StartsWith("PopupBase"))
                        return m;
                    t = t.BaseType;
                }
            }
            return null;
        }
    }
}
#endif
