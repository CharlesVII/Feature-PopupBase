#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Features.Popups.EditorTools
{
    /// <summary>
    /// 🎨 Popup Animation Tree View (optimized)
    /// </summary>
    public class PopupAnimationWindowView
    {
        private readonly PopupAnimationWindowContext context;
        private readonly PopupAnimationLogic logic;
        private Vector2 scroll;
        private readonly Dictionary<MonoBehaviour, Editor> editorCache = new();
        private readonly Dictionary<Transform, bool> foldoutStates = new();
        public static bool IsDrawingInTool = false;

        public PopupAnimationWindowView(PopupAnimationWindowContext context)
        {
            this.context = context;
            this.logic = new PopupAnimationLogic(context);
        }

        // =========================================================
        // MAIN DRAW
        // =========================================================
        public void DrawGUI()
        {
            IsDrawingInTool = true;
            EditorGUILayout.LabelField("🎬 Popup Animation Tree", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
            if (GUILayout.Button("🔄 Load Animations", GUILayout.Height(24)))
            {
                logic.SyncMissingAnimationsInPrefab(context.CurrentPrefab);
            }

            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("🗑 Clear All (Exclude Root)", GUILayout.Height(24)))
            {
                if (EditorUtility.DisplayDialog(
                    "Clear All Animations",
                    "Are you sure you want to remove ALL Sequences and Animations (except root PopupBase)?",
                    "Yes, Clear", "Cancel"))
                {
                    logic.ClearAllSequencesAndAnimations(context.CurrentPrefab);
                    EditorWindow.GetWindow<PopupAnimationWindow>()?.Repaint();
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            var prefab = (GameObject)EditorGUILayout.ObjectField("Popup Prefab / Instance", context.CurrentPrefab, typeof(GameObject), true);
            if (prefab != context.CurrentPrefab)
                context.BindTo(prefab);

            if (context.CurrentPrefab == null)
            {
                EditorGUILayout.HelpBox("No prefab linked. Open prefab and click the tool button.", MessageType.Info);
                return;
            }

            DrawTree();
            IsDrawingInTool = false;
        }

        // =========================================================
        // TREE
        // =========================================================
        private void DrawTree()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            float totalWidth = EditorGUIUtility.currentViewWidth - 40;
            float colHierarchy = totalWidth * 0.1f;
            float colShow = totalWidth * 0.35f;
            float colHide = totalWidth * 0.35f;

            // Header
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📂 Hierarchy", EditorStyles.boldLabel, GUILayout.Width(colHierarchy));
            EditorGUILayout.LabelField("🟢 Show", EditorStyles.boldLabel, GUILayout.Width(colShow));
            EditorGUILayout.LabelField("🔴 Hide", EditorStyles.boldLabel, GUILayout.Width(colHide));
            EditorGUILayout.EndHorizontal();

            var root = context.CurrentPrefab?.transform;
            if (root != null)
                DrawNodeRecursive(root, 0, colHierarchy, colShow, colHide);

            EditorGUILayout.EndScrollView();
        }

        private void DrawNodeRecursive(Transform node, int depth, float colHierarchy, float colShow, float colHide)
        {
            if (node == null) return;

            bool hasAnim = HasAnimation(node);
            bool hasChild = node.childCount > 0;

            Rect rect = EditorGUILayout.BeginHorizontal();
            if (Event.current.type == EventType.Repaint)
            {
                var bg = (depth % 2 == 0) ? new Color(0.18f, 0.18f, 0.18f, 0.5f) : new Color(0.14f, 0.14f, 0.14f, 0.5f);
                EditorGUI.DrawRect(rect, bg);
            }

            // 📁 Foldout + name
            GUILayout.Space(depth * 16);
            if (hasChild)
            {
                if (!foldoutStates.ContainsKey(node))
                    foldoutStates[node] = true;
                foldoutStates[node] = EditorGUILayout.Foldout(foldoutStates[node], node.name, true, EditorStyles.foldout);
            }
            else
                GUILayout.Label($"↳ {node.name}", EditorStyles.boldLabel, GUILayout.Width(colHierarchy - depth * 16 - 40));

            GUILayout.FlexibleSpace();

            // Buttons
            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("+Seq", GUILayout.Width(55))) logic.ShowAddSequenceMenu(node);
            GUI.backgroundColor = new Color(0.4f, 1f, 0.4f);
            if (GUILayout.Button("+Show", GUILayout.Width(50))) logic.ShowAddMenu(node, true);
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("+Hide", GUILayout.Width(50))) logic.ShowAddMenu(node, false);
            GUI.backgroundColor = Color.white;

            // Animations (show / hide)
            DrawAnimationCell(node, true, colShow);
            DrawAnimationCell(node, false, colHide);

            EditorGUILayout.EndHorizontal();

            // Vẽ con
            if (hasChild && foldoutStates.ContainsKey(node) && foldoutStates[node])
            {
                for (int i = 0; i < node.childCount; i++)
                    DrawNodeRecursive(node.GetChild(i), depth + 1, colHierarchy, colShow, colHide);
            }
        }

        // =========================================================
        // ANIMATION
        // =========================================================
        private void DrawAnimationCell(Transform t, bool isShow, float width)
        {
            var popupBase = FindPopupBase(t);
            if (popupBase == null)
            {
                GUILayout.Label("-", GUILayout.Width(width));
                return;
            }

            var seqField = popupBase.GetType().GetField(isShow ? "showSequence" : "hideSequence",
                BindingFlags.NonPublic | BindingFlags.Instance);
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
                if (t == popupBase.transform)
                {
                    var seqComp = rootSeq as MonoBehaviour;
                    if (seqComp != null)
                    {
                        EditorGUILayout.BeginVertical(GUILayout.Width(width));
                        DrawAnimationNode(seqComp, isShow, width, 0);
                        EditorGUILayout.EndVertical();
                        return;
                    }
                }

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
            if (anim == null) return;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent * 12);

            // Icon + Tên animation
            bool isSequence = anim is PopupShowSequence || anim is PopupHideSequence;
            GUILayout.Label(isSequence ? "📁" : "🎞", GUILayout.Width(20));
            GUILayout.Label(anim.GetType().Name, EditorStyles.miniBoldLabel);

            // 🔁 Change Animation
            GUI.backgroundColor = new Color(0.7f, 0.7f, 1f);
            if (!isSequence && GUILayout.Button("Change", GUILayout.Width(65), GUILayout.Height(18)))
            {
                GenericMenu menu = new GenericMenu();
                var allAnimTypes = logic.GetAllAnimationTypes(isShow);
                foreach (var type in allAnimTypes)
                {
                    if (type == anim.GetType()) continue;
                    menu.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        logic.ReplaceAnimation(anim, type, isShow);
                    });
                }
                menu.ShowAsContext();
            }
            GUI.backgroundColor = Color.white;

            // Nút remove
            GUI.backgroundColor = new Color(0.9f, 0.2f, 0.2f);
            if (GUILayout.Button("✕", GUILayout.Width(22), GUILayout.Height(18)))
            {
                logic.RemoveAnimation(anim, isShow);
                GUIUtility.ExitGUI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            // Mini Inspector
            var ed = GetCachedEditor(anim);
            if (ed != null)
            {
                EditorGUI.BeginChangeCheck();
                ed.OnInspectorGUI();
                if (EditorGUI.EndChangeCheck())
                    EditorUtility.SetDirty(anim);
            }

            EditorGUILayout.EndVertical();
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private bool HasAnimation(Transform node)
        {
            var all = node.GetComponents<MonoBehaviour>();
            foreach (var c in all)
                if (c is IShowPhase || c is IHidePhase || c is PopupShowSequence || c is PopupHideSequence)
                    return true;
            return false;
        }

        private bool HasDescendantWithAnim(Transform parent)
        {
            foreach (Transform child in parent)
                if (HasAnimation(child) || HasDescendantWithAnim(child))
                    return true;
            return false;
        }

        private void CollectAnimationsFromSequence(object sequence, Transform target, List<MonoBehaviour> result)
        {
            if (sequence == null) return;
            var nodesField = sequence.GetType().GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance);
            var nodes = nodesField?.GetValue(sequence) as IList;
            if (nodes == null) return;

            foreach (var n in nodes)
            {
                if (n is MonoBehaviour m)
                {
                    if (m.transform == target)
                        result.Add(m);

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
