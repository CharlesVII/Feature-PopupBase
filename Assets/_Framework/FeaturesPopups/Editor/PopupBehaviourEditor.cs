#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
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

            // Lấy tham chiếu đến PopupBehaviour hiện tại
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

            // === 🔹 Thêm nút Remove All Sequences ngay dưới ===
            EditorGUILayout.Space(4);
            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
            if (GUILayout.Button("🗑 Remove Sequences & Animations", GUILayout.Height(26)))
            {
                if (EditorUtility.DisplayDialog("Remove All?",
                    "This will remove all Show/Hide sequences and animation components.\nAre you sure?",
                    "Yes, remove", "Cancel"))
                {
                    RemoveAllSequences(popup);
                }
            }
            GUI.backgroundColor = Color.white;
        } // <-- 🔒 ĐÓNG khối OnInspectorGUI tại đây


        private void RemoveAllSequences(MonoBehaviour popupBase)
        {
            var popupType = popupBase.GetType();
            var showField = popupType.GetField("showSequence", BindingFlags.NonPublic | BindingFlags.Instance);
            var hideField = popupType.GetField("hideSequence", BindingFlags.NonPublic | BindingFlags.Instance);

            var showSeq = showField?.GetValue(popupBase) as MonoBehaviour;
            var hideSeq = hideField?.GetValue(popupBase) as MonoBehaviour;

            var toRemove = new List<MonoBehaviour>();

            void Collect(MonoBehaviour seq)
            {
                if (seq == null) return;

                var nodesField = seq.GetType().GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance);
                if (nodesField?.GetValue(seq) is System.Collections.IList nodes)
                {
                    foreach (var n in nodes)
                    {
                        if (n is MonoBehaviour m)
                        {
                            if (!toRemove.Contains(m))
                                toRemove.Add(m);

                            if (m is PopupShowSequence || m is PopupHideSequence)
                                Collect(m);
                        }
                    }
                }

                if (!toRemove.Contains(seq))
                    toRemove.Add(seq);
            }

            // 🧩 Thu thập các sequence con
            Collect(showSeq);
            Collect(hideSeq);

            // 🧩 Thu thập thêm các PopupBase kế thừa
            var allComps = popupBase.GetComponents<MonoBehaviour>();
            foreach (var comp in allComps)
            {
                if (comp == null) continue;

                var type = comp.GetType();
                while (type != null)
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition().Name.StartsWith("PopupBase"))
                    {
                        if (!toRemove.Contains(comp))
                            toRemove.Add(comp);
                        break;
                    }
                    type = type.BaseType;
                }
            }

            // 🧩 Ghi nhận Undo cho popupBase
            Undo.RegisterCompleteObjectUndo(popupBase.gameObject, "Remove Popup Sequences");

            // 🧩 Ghi nhận Undo cho từng component trước khi xoá
            foreach (var comp in toRemove)
            {
                if (comp != null)
                    Undo.RegisterCompleteObjectUndo(comp, "Remove Popup Component");
            }

            // 🧩 Xoá ngược để tránh dependency
            for (int i = toRemove.Count - 1; i >= 0; i--)
            {
                if (toRemove[i] != null)
                    Undo.DestroyObjectImmediate(toRemove[i]);
            }

            // 🧩 Clear reference trong PopupBase
            showField?.SetValue(popupBase, null);
            hideField?.SetValue(popupBase, null);

            EditorUtility.SetDirty(popupBase);
            Debug.Log($"🧹 Removed {toRemove.Count} components (Undo available) from '{popupBase.name}'");
        }
    }
}
#endif
