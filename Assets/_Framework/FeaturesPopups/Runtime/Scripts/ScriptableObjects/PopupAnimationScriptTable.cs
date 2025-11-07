using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Features.Popups
{
    [CreateAssetMenu(fileName = "PopupAnimationScriptTable", menuName = "ScriptableObjects/PopupAnimationScriptTable")]
    public class PopupAnimationScriptTable : ScriptableObject
    {
        public List<AnimationEntry> animationEntries = new();
        public List<SequenceEntry> sequenceEntries = new(); // 🆕 Thêm danh sách Sequence riêng

        public List<Type> GetAnimationTypes(bool isShow)
        {
            var result = new List<Type>();
            foreach (var entry in animationEntries)
            {
                if (entry == null || entry.script == null) continue;
                var t = entry.script.GetClass();
                if (t == null) continue;
                if (isShow && typeof(IShowPhase).IsAssignableFrom(t)) result.Add(t);
                else if (!isShow && typeof(IHidePhase).IsAssignableFrom(t)) result.Add(t);
            }
            return result;
        }

        public List<Type> GetSequenceTypes(bool isShow)
        {
            var result = new List<Type>();
            foreach (var entry in sequenceEntries)
            {
                if (entry == null || entry.script == null) continue;
                var t = entry.script.GetClass();
                if (t == null) continue;
                if (isShow && t == typeof(PopupShowSequence)) result.Add(t);
                else if (!isShow && t == typeof(PopupHideSequence)) result.Add(t);
            }
            return result;
        }

#if UNITY_EDITOR
        [ContextMenu("Auto Sync Scripts")]
        private void AutoSync()
        {
            animationEntries.Clear();
            sequenceEntries.Clear();

            var allScripts = AssetDatabase.FindAssets("t:MonoScript");
            foreach (var guid in allScripts)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script == null) continue;

                var type = script.GetClass();
                if (type == null) continue;

                // 🧩 Detect Sequence (base generic: PopupAnimationSequenceBase<>)
                if (type.BaseType != null &&
                    type.BaseType.IsGenericType &&
                    type.BaseType.GetGenericTypeDefinition() == typeof(PopupAnimationSequenceBase<>))
                {
                    sequenceEntries.Add(new SequenceEntry
                    {
                        displayName = type.Name,
                        script = script
                    });
                }
                // 🧩 Detect Animation (implements IAnimationPhase, not interface/abstract)
                else if (typeof(IAnimationPhase).IsAssignableFrom(type)
                         && !type.IsInterface
                         && !type.IsAbstract
                         && typeof(MonoBehaviour).IsAssignableFrom(type))
                {
                    animationEntries.Add(new AnimationEntry
                    {
                        displayName = type.Name,
                        script = script
                    });
                }
            }

            Debug.Log($"✅ Synced {animationEntries.Count} Anim + {sequenceEntries.Count} Seq scripts.");
        }

#endif
    }

    [Serializable]
    public class AnimationEntry
    {
        public string displayName;
        public MonoScript script;
    }

    [Serializable]
    public class SequenceEntry
    {
        public string displayName;
        public MonoScript script;
    }
}
