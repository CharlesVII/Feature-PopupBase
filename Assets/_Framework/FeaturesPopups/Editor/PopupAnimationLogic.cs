#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Features.Popups.EditorTools
{
    public class PopupAnimationLogic
    {
        private readonly PopupAnimationWindowContext context;
        public PopupAnimationLogic(PopupAnimationWindowContext context) => this.context = context;

        // =========================================================
        // ADD / REMOVE / SYNC
        // =========================================================
        public void ShowAddMenu(Transform target, bool isShow)
        {
            GenericMenu menu = new GenericMenu();
            string prefix = isShow ? "Show" : "Hide";
            var allAnimTypes = GetAllAnimationTypes(isShow);

            foreach (var type in allAnimTypes)
            {
                // ⚡ Nếu target đã có component này thì bỏ qua menu
                if (target.GetComponent(type) != null)
                    continue;

                string displayName = ObjectNames.NicifyVariableName(type.Name);
                menu.AddItem(new GUIContent($"{prefix} / {displayName}"), false,
                    () => AddAnimationToPopup(target, isShow, type));
            }

            menu.AddSeparator($"{prefix}/");
            menu.AddItem(new GUIContent($"{prefix} / Sub Sequence"), false,
                () => AddSubSequence(target, isShow));
            menu.ShowAsContext();
        }

        public void ShowAddSequenceMenu(Transform target)
        {
            GenericMenu menu = new GenericMenu();
            var table = Resources.Load<PopupAnimationScriptTable>("PopupAnimationScriptTable");
            if (table == null)
            {
                Debug.LogWarning("⚠️ Missing PopupAnimationScriptTable in Resources/");
                return;
            }

            var showSeqTypes = table.GetSequenceTypes(true);
            var hideSeqTypes = table.GetSequenceTypes(false);

            menu.AddSeparator("");
            menu.AddDisabledItem(new GUIContent("Sequences"));

            // 🟢 Show Sequences
            foreach (var type in showSeqTypes)
                menu.AddItem(new GUIContent("Show/" + type.Name), false, () => AddSequenceByType(target, type, true));

            // 🔴 Hide Sequences
            foreach (var type in hideSeqTypes)
                menu.AddItem(new GUIContent("Hide/" + type.Name), false, () => AddSequenceByType(target, type, false));

            menu.ShowAsContext();
        }

        private void AddSequenceByType(Transform target, Type seqType, bool isShow)
        {
            if (target == null) return;
            var popupBase = FindPopupBase(target);
            if (popupBase == null) return;

            var newSeq = target.gameObject.AddComponent(seqType) as MonoBehaviour;
            var parentSeq = FindParentSequence(target, isShow, popupBase);

            if (parentSeq == null)
            {
                var seqField = popupBase.GetType().GetField(isShow ? "showSequence" : "hideSequence",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                parentSeq = seqField?.GetValue(popupBase) as MonoBehaviour;
            }

            if (parentSeq == null)
            {
                Debug.LogWarning("⚠️ No parent sequence found.");
                return;
            }

            var addNode = parentSeq.GetType().GetMethod("AddNode", new[] { typeof(MonoBehaviour) });
            addNode?.Invoke(parentSeq, new object[] { newSeq });
            MarkDirtyAll(popupBase, parentSeq, newSeq);
        }

        // =========================================================
        // ADD COMPONENT
        // =========================================================
        private void AddAnimationToPopup(Transform target, bool isShow, Type type)
        {
            if (target == null)
            {
                EditorUtility.DisplayDialog("No Target", "Select a valid target first.", "OK");
                return;
            }

            if (target.GetComponent(type) != null)
            {
                Debug.LogWarning($"⚠️ '{target.name}' already has {type.Name}, skipping add.");
                return;
            }

            var popupBase = FindPopupBase(target);
            if (popupBase == null)
            {
                EditorUtility.DisplayDialog("PopupBase not found", "Cannot find PopupBase in parent hierarchy.", "OK");
                return;
            }

            // 🧩 Thêm animation component trực tiếp
            var newComp = target.gameObject.AddComponent(type) as MonoBehaviour;
            (newComp as IAnimationInitializable)?.Initialize(target);

            // 🧩 Tìm sequence cha gần nhất
            MonoBehaviour targetSequence = FindParentSequence(target, isShow, popupBase);
            if (targetSequence == null)
            {
                var popupType = popupBase.GetType();
                var seqField = popupType.GetField(isShow ? "showSequence" : "hideSequence",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                targetSequence = seqField?.GetValue(popupBase) as MonoBehaviour;
                if (targetSequence == null)
                {
                    var seqType = seqField.FieldType;
                    targetSequence = popupBase.gameObject.AddComponent(seqType) as MonoBehaviour;
                    targetSequence.name = isShow ? "ShowSequence_Auto" : "HideSequence_Auto";
                    seqField.SetValue(popupBase, targetSequence);
                }
            }

            // 🧩 Add vào nodes
            var addNodeMethod = targetSequence.GetType().GetMethod("AddNode", new[] { typeof(MonoBehaviour) });
            addNodeMethod?.Invoke(targetSequence, new object[] { newComp });

            Debug.Log($"[PopupAnimationWindow] Added {newComp.GetType().Name} → {targetSequence.name}");

            MarkDirtyAll(popupBase, target, targetSequence, newComp);
        }

        private void AddSubSequence(Transform target, bool isShow)
        {
            if (target == null)
            {
                EditorUtility.DisplayDialog("No Target", "Select a valid target first.", "OK");
                return;
            }

            var popupBase = FindPopupBase(target);
            if (popupBase == null)
            {
                EditorUtility.DisplayDialog("PopupBase not found", "Cannot find PopupBase in parent hierarchy.", "OK");
                return;
            }

            // ⚡ Nếu đã có sequence cùng loại thì return
            if (isShow && target.GetComponent<PopupShowSequence>() != null)
            {
                Debug.LogWarning($"⚠️ '{target.name}' already has PopupShowSequence.");
                return;
            }
            if (!isShow && target.GetComponent<PopupHideSequence>() != null)
            {
                Debug.LogWarning($"⚠️ '{target.name}' already has PopupHideSequence.");
                return;
            }

            // ➕ Thêm sequence mới
            MonoBehaviour subSequence = isShow
                ? target.gameObject.AddComponent<PopupShowSequence>()
                : target.gameObject.AddComponent<PopupHideSequence>();


            var parentSequence = FindParentSequence(target, isShow, popupBase);
            if (parentSequence == null)
            {
                var popupType = popupBase.GetType();
                var seqField = popupType.GetField(isShow ? "showSequence" : "hideSequence",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                parentSequence = seqField?.GetValue(popupBase) as MonoBehaviour;
                if (parentSequence == null)
                {
                    var seqType = seqField.FieldType;
                    parentSequence = popupBase.gameObject.AddComponent(seqType) as MonoBehaviour;
                    parentSequence.name = isShow ? "ShowSequence_Auto" : "HideSequence_Auto";
                    seqField.SetValue(popupBase, parentSequence);
                }
            }

            var addNodeMethod = parentSequence.GetType().GetMethod("AddNode", new[] { typeof(MonoBehaviour) });
            addNodeMethod?.Invoke(parentSequence, new object[] { subSequence });

            Debug.Log($"[PopupAnimationWindow] Added {subSequence.GetType().Name} → {parentSequence.name}");

            MarkDirtyAll(popupBase, target, parentSequence, subSequence);
        }

        // =========================================================
        // REMOVE
        // =========================================================
        public void RemoveAnimation(MonoBehaviour comp, bool isShow)
        {
            if (comp == null) return;

            var popupBase = FindPopupBase(comp.transform);
            if (popupBase == null) return;

            var popupType = popupBase.GetType();
            var seqField = popupType.GetField(isShow ? "showSequence" : "hideSequence",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var rootSeq = seqField?.GetValue(popupBase);
            if (rootSeq == null) return;

            bool removed = RemoveAnimationFromSequenceTree(rootSeq, comp);

            if (removed)
            {
                UnityEngine.Object.DestroyImmediate(comp);
                Debug.Log($"[PopupAnimationWindow] Removed {comp.GetType().Name}");
                MarkDirtyAll(popupBase);
            }
        }

        // =========================================================
        // SYNC + HELPERS
        // =========================================================
        private static List<Type> GetAllAnimationTypes(bool isShow)
        {
            var table = Resources.Load<PopupAnimationScriptTable>("PopupAnimationScriptTable");
            if (table == null || table.Equals(null))
            {
                Debug.LogWarning("⚠️ Missing PopupAnimationScriptTable in Resources/");
                return new List<Type>();
            }

            return table.GetAnimationTypes(isShow);
        }

        private MonoBehaviour FindParentSequence(Transform current, bool isShow, MonoBehaviour popupBase)
        {
            MonoBehaviour seq = null;
            current = current.parent; // 🩵 Bỏ qua chính thằng hiện tại

            while (current != null && seq == null)
            {
                seq = isShow
                    ? current.GetComponent<PopupShowSequence>()
                    : current.GetComponent<PopupHideSequence>();
                current = current.parent;
            }

            if (seq == null)
            {
                var seqField = popupBase.GetType().GetField(isShow ? "showSequence" : "hideSequence",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                seq = seqField?.GetValue(popupBase) as MonoBehaviour;
            }
            return seq;
        }


        private bool RemoveAnimationFromSequenceTree(object sequence, MonoBehaviour target)
        {
            if (sequence == null) return false;
            var nodesField = sequence.GetType().GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance);
            var nodes = nodesField?.GetValue(sequence) as IList;
            if (nodes == null) return false;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == target)
                {
                    nodes.RemoveAt(i);
                    return true;
                }

                if (node is MonoBehaviour sub && (sub is PopupShowSequence || sub is PopupHideSequence))
                {
                    if (RemoveAnimationFromSequenceTree(sub, target))
                        return true;
                }
            }
            return false;
        }

        private MonoBehaviour FindPopupBase(Transform target)
        {
            var monoList = target.GetComponentsInParent<MonoBehaviour>(true);
            foreach (var mono in monoList)
            {
                if (mono == null) continue;
                Type type = mono.GetType();
                while (type != null)
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition().Name.StartsWith("PopupBase"))
                        return mono;
                    type = type.BaseType;
                }
            }
            return null;
        }

        private void MarkDirtyAll(params UnityEngine.Object[] objs)
        {
            foreach (var obj in objs)
            {
                if (obj != null)
                    EditorUtility.SetDirty(obj);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void ClearAnimationsForNode(Transform target)
        {
            if (target == null) return;

            var popupBase = FindPopupBase(target);
            if (popupBase == null) return;

            int removed = 0;

            // Lấy sequence gốc của popup
            var popupType = popupBase.GetType();
            var showSeqField = popupType.GetField("showSequence", BindingFlags.NonPublic | BindingFlags.Instance);
            var hideSeqField = popupType.GetField("hideSequence", BindingFlags.NonPublic | BindingFlags.Instance);

            var showRoot = showSeqField?.GetValue(popupBase);
            var hideRoot = hideSeqField?.GetValue(popupBase);

            // Tìm các component animation/sequence trên chính GameObject này
            var comps = target.GetComponents<MonoBehaviour>();
            foreach (var comp in comps)
            {
                if (comp == null) continue;

                var t = comp.GetType();
                bool isAnim = typeof(IShowPhase).IsAssignableFrom(t) || typeof(IHidePhase).IsAssignableFrom(t);
                bool isSeq = comp is PopupShowSequence || comp is PopupHideSequence;
                if (!isAnim && !isSeq) continue;

                // ✅ Chỉ remove khỏi list của parent sequence có liên quan
                RemoveFromDirectParentSequence(target, comp, showRoot);
                RemoveFromDirectParentSequence(target, comp, hideRoot);

                Undo.DestroyObjectImmediate(comp);
                removed++;
            }

            if (removed > 0)
            {
                EditorUtility.SetDirty(target);
                EditorUtility.SetDirty(popupBase);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                SceneView.RepaintAll();
                Debug.Log($"🗑 Removed {removed} animation(s)/sequence(s) from '{target.name}' and cleaned direct parent nodes.");
            }
            else
            {
                Debug.Log($"ℹ️ No animation/sequence found on '{target.name}'.");
            }
        }

        private bool RemoveFromDirectParentSequence(Transform target, MonoBehaviour comp, object sequenceRoot)
        {
            if (sequenceRoot == null) return false;

            var nodesField = sequenceRoot.GetType().GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (nodesField == null) return false;
            var nodes = nodesField.GetValue(sequenceRoot) as IList;
            if (nodes == null) return false;

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == comp)
                {
                    nodes.RemoveAt(i);
                    return true;
                }

                // Nếu node là sequence con thì kiểm tra đệ quy nhưng chỉ trong subtree chứa target
                if (nodes[i] is MonoBehaviour subSeq &&
                    (subSeq is PopupShowSequence || subSeq is PopupHideSequence))
                {
                    // Chỉ duyệt vào nếu subSeq là cha trực tiếp của target
                    if (subSeq.transform == target.parent)
                    {
                        if (RemoveFromDirectParentSequence(target, comp, subSeq))
                            return true;
                    }
                }
            }

            return false;
        }


        // =========================================================
        // BULK OPERATIONS (Load / Clear)
        // =========================================================
        public void SyncMissingAnimationsInPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("No Prefab", "Please select or open a popup prefab first.", "OK");
                return;
            }

            int addedCount = 0;

            var allSeqs = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var seq in allSeqs)
            {
                if (!(seq is PopupShowSequence) && !(seq is PopupHideSequence))
                    continue;

                var nodesField = seq.GetType().GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance);
                if (nodesField == null) continue;

                var nodes = nodesField.GetValue(seq) as IList;
                if (nodes == null) continue;

                bool isShow = seq is PopupShowSequence;
                Type phaseType = isShow ? typeof(IShowPhase) : typeof(IHidePhase);

                foreach (Transform child in seq.transform)
                {
                    if (child == null) continue;

                    MonoBehaviour subSeq = isShow
                        ? child.GetComponent<PopupShowSequence>()
                        : child.GetComponent<PopupHideSequence>();

                    if (subSeq != null)
                    {
                        if (!nodes.Contains(subSeq))
                        {
                            Undo.RecordObject(seq, "Sync Missing SubSequence");
                            nodes.Add(subSeq);
                            addedCount++;
                            EditorUtility.SetDirty(seq);
                        }
                        continue;
                    }

                    // Nếu là animation leaf
                    var anims = child.GetComponents<MonoBehaviour>()
                                     .Where(c => phaseType.IsInstanceOfType(c))
                                     .ToList();

                    foreach (var anim in anims)
                    {
                        if (nodes.Contains(anim)) continue;
                        Undo.RecordObject(seq, "Sync Missing Animation Node");
                        nodes.Add(anim);
                        addedCount++;
                        EditorUtility.SetDirty(seq);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SceneView.RepaintAll();
            EditorWindow.GetWindow<PopupAnimationWindow>()?.Repaint();

            if (addedCount > 0)
                Debug.Log($"[PopupAnimationTool] ✅ Synced {addedCount} missing animations in '{prefab.name}'");
            else
                Debug.Log($"[PopupAnimationTool] No missing animations found in '{prefab.name}'");
        }


        public void ClearAllSequencesAndAnimations(GameObject prefab)
        {
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("No Prefab", "Please select or open a popup prefab first.", "OK");
                return;
            }

            int removedCount = 0;

            // 🔹 Lấy ra PopupBase gốc (nếu có)
            var popupBase = prefab.GetComponentInChildren<MonoBehaviour>(true);
            popupBase = FindPopupBase(prefab.transform);
            if (popupBase == null)
            {
                Debug.LogWarning($"⚠️ No PopupBase found in '{prefab.name}'");
                return;
            }

            // 🔹 Lấy 2 root sequence của popupBase để bảo toàn
            var popupType = popupBase.GetType();
            var showSeqField = popupType.GetField("showSequence", BindingFlags.NonPublic | BindingFlags.Instance);
            var hideSeqField = popupType.GetField("hideSequence", BindingFlags.NonPublic | BindingFlags.Instance);
            var showRoot = showSeqField?.GetValue(popupBase) as MonoBehaviour;
            var hideRoot = hideSeqField?.GetValue(popupBase) as MonoBehaviour;

            // 🔹 Lấy toàn bộ các component animation/sequence con
            var allComponents = prefab.GetComponentsInChildren<MonoBehaviour>(true)
                .Where(c => c != null &&
                            c.gameObject != prefab &&
                            c != popupBase &&
                            c != showRoot &&
                            c != hideRoot)
                .ToList();

            foreach (var comp in allComponents)
            {
                Type type = comp.GetType();

                // Bỏ qua các PopupBase khác
                if (IsPopupBaseOrDerived(type))
                    continue;

                bool shouldRemove =
                    comp is PopupShowSequence ||
                    comp is PopupHideSequence ||
                    typeof(IShowPhase).IsAssignableFrom(type) ||
                    typeof(IHidePhase).IsAssignableFrom(type);

                if (!shouldRemove)
                    continue;

                // 🧹 Trước khi xoá → remove khỏi các list nodes cha
                RemoveFromAllSequences(prefab, comp);

                Undo.DestroyObjectImmediate(comp);
                removedCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SceneView.RepaintAll();
            EditorWindow.GetWindow<PopupAnimationWindow>()?.Repaint();

            Debug.Log($"🧹 Cleared {removedCount} sequences/animations (excluded root PopupBase + root show/hide seq) in '{prefab.name}'");
        }

        private void RemoveFromAllSequences(GameObject prefab, MonoBehaviour target)
        {
            if (prefab == null || target == null) return;

            var allSeqs = prefab.GetComponentsInChildren<MonoBehaviour>(true)
                .Where(c => c is PopupShowSequence || c is PopupHideSequence)
                .ToList();

            foreach (var seq in allSeqs)
            {
                var nodesField = seq.GetType().GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance);
                if (nodesField == null) continue;

                var nodes = nodesField.GetValue(seq) as IList;
                if (nodes == null || nodes.Count == 0) continue;

                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    var node = nodes[i] as MonoBehaviour;
                    if (node == null || node == target)
                    {
                        nodes.RemoveAt(i);
                        EditorUtility.SetDirty(seq);
                    }
                }
            }
        }


        private bool IsPopupBaseOrDerived(Type type)
        {
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition().Name.StartsWith("PopupBase"))
                    return true;
                type = type.BaseType;
            }
            return false;
        }

    }
}
#endif