#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Features.Popups.EditorTools
{
    /// <summary>
    /// 🎨 View: chỉ chịu trách nhiệm hiển thị UI
    /// </summary>
    public class PopupAnimationWindowView
    {
        private readonly PopupAnimationWindowContext context;
        private readonly PopupAnimationLogic logic;
        private Vector2 scroll;
        private readonly Dictionary<MonoBehaviour, Editor> editorCache = new();

        public PopupAnimationWindowView(PopupAnimationWindowContext context)
        {
            this.context = context;
            this.logic = new PopupAnimationLogic(context);
        }

        public void DrawGUI()
        {
            EditorGUILayout.LabelField("🎬 Popup Animation Tree", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.8f, 0.9f, 1f);
            if (GUILayout.Button("🧩 Sync Missing", GUILayout.Height(24)))
                logic.SyncMissingAnimationsInPrefab(context.CurrentPrefab);

            //GUI.backgroundColor = new Color(1f, 0.85f, 0.85f);
            //if (GUILayout.Button("🗑 Clear All", GUILayout.Height(24)))
            //    logic.ClearAllSequencesAndAnimations(context.CurrentPrefab);

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            var prefab = (GameObject)EditorGUILayout.ObjectField("Popup Prefab / Instance", context.CurrentPrefab, typeof(GameObject), true);
            if (prefab != context.CurrentPrefab)
                context.BindTo(prefab);

            if (context.CurrentPrefab == null)
            {
                EditorGUILayout.HelpBox("No prefab linked. Open prefab and click the tool button.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(8);
            DrawTree();
        }

        private void DrawTree()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            var totalWidth = EditorGUIUtility.currentViewWidth - 60;
            var colHierarchy = totalWidth * 0.15f;
            var colShow = totalWidth * 0.38f;
            var colHide = totalWidth * 0.38f;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📂 Hierarchy", EditorStyles.boldLabel, GUILayout.Width(colHierarchy));
            EditorGUILayout.LabelField("🟢 Show Animations", EditorStyles.boldLabel, GUILayout.Width(colShow));
            EditorGUILayout.LabelField("🔴 Hide Animations", EditorStyles.boldLabel, GUILayout.Width(colHide));
            EditorGUILayout.EndHorizontal();

            var root = context.CurrentPrefab?.transform;
            if (root != null)
                DrawNodeRecursive(root, 0, colHierarchy, colShow, colHide);

            EditorGUILayout.EndScrollView();
        }

        private void DrawNodeRecursive(Transform node, int depth, float colHierarchy, float colShow, float colHide)
        {
            if (node == null) return;
            EditorGUILayout.BeginHorizontal("box");

            string prefix = new string(' ', depth * 3);
            GUILayout.Label($"{prefix}{(depth > 0 ? "↳ " : "")}{node.name}", EditorStyles.boldLabel, GUILayout.Width(colHierarchy - 60));

            if (GUILayout.Button("+Seq", GUILayout.Width(60))) logic.ShowAddSequenceMenu(node);
            GUI.backgroundColor = new Color(0.4f, 1f, 0.4f);
            if (GUILayout.Button("+Show", GUILayout.Width(50))) logic.ShowAddMenu(node, true);
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("+Hide", GUILayout.Width(50))) logic.ShowAddMenu(node, false);
            GUI.backgroundColor = Color.white;

            DrawAnimationCell(node, true, colShow);
            DrawAnimationCell(node, false, colHide);

            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < node.childCount; i++)
                DrawNodeRecursive(node.GetChild(i), depth + 1, colHierarchy, colShow, colHide);
        }

        private void DrawAnimationCell(Transform t, bool isShow, float width)
        {
            var popupBase = FindPopupBase(t);
            if (popupBase == null)
            {
                GUILayout.Label("-", GUILayout.Width(width));
                return;
            }

            var seqField = popupBase.GetType().GetField(isShow ? "showSequence" : "hideSequence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var rootSeq = seqField?.GetValue(popupBase);
            if (rootSeq == null)
            {
                GUILayout.Label("-", GUILayout.Width(width));
                return;
            }

            List<MonoBehaviour> localAnims = new();
            CollectAnimationsFromSequence(rootSeq, t, localAnims);
            if (localAnims.Count == 0)
            {
                GUILayout.Label("-", GUILayout.Width(width));
                return;
            }

            EditorGUILayout.BeginVertical(GUILayout.Width(width));
            foreach (var anim in localAnims)
                DrawAnimationNode(anim, isShow, width, 0);
            EditorGUILayout.EndVertical();
        }

        private void DrawAnimationNode(MonoBehaviour anim, bool isShow, float width, int indent)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent * 15);
            GUILayout.Label(anim is PopupShowSequence || anim is PopupHideSequence ? "📁" : "🎞", GUILayout.Width(20));
            GUILayout.Label(anim.GetType().Name, EditorStyles.miniBoldLabel);
            GUI.backgroundColor = new Color(0.9f, 0.2f, 0.2f);
            if (GUILayout.Button("✕", GUILayout.Width(24), GUILayout.Height(22)))
            {
                logic.RemoveAnimation(anim, isShow);
                GUIUtility.ExitGUI();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            var ed = GetCachedEditor(anim);
            if (ed != null)
            {
                EditorGUI.BeginChangeCheck();
                ed.OnInspectorGUI();
                if (EditorGUI.EndChangeCheck())
                    EditorUtility.SetDirty(anim);
            }
            EditorGUILayout.Space(4);
        }

        private void CollectAnimationsFromSequence(object sequence, Transform target, List<MonoBehaviour> result)
        {
            if (sequence == null) return;

            var nodesField = sequence.GetType().GetField("nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nodes = nodesField?.GetValue(sequence) as IList;
            if (nodes == null) return;

            foreach (var n in nodes)
            {
                if (n is MonoBehaviour m)
                {
                    // Nếu đúng Transform → add
                    if (m.transform == target)
                        result.Add(m);

                    // ✅ Duyệt sâu các sequence con (Show hoặc Hide)
                    if (m is PopupShowSequence || m is PopupHideSequence)
                        CollectAnimationsFromSequence(m, target, result);
                }
            }
        }


        private MonoBehaviour FindPopupBase(Transform target)
        {
            var monos = target.GetComponentsInParent<MonoBehaviour>(true);
            foreach (var m in monos)
            {
                if (m == null) continue;
                var type = m.GetType();
                while (type != null)
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition().Name.StartsWith("PopupBase"))
                        return m;
                    type = type.BaseType;
                }
            }
            return null;
        }

        private Editor GetCachedEditor(MonoBehaviour comp)
        {
            if (comp == null) return null;
            if (!editorCache.TryGetValue(comp, out var ed) || ed == null)
            {
                ed = Editor.CreateEditor(comp);
                editorCache[comp] = ed;
            }
            return ed;
        }
    }
}
#endif
