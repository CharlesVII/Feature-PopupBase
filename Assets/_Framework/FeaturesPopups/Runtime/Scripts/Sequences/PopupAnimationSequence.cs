using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Features.Popups
{
    public abstract class PopupAnimationSequenceBase<TPhase> : MonoBehaviour, IAnimationPhase
      where TPhase : IAnimationPhase
    {
        [Header("Sequence Settings")]
        [SerializeField] protected bool runSequentially = true;
        [SerializeField] protected float delayBefore = 0f;
        [SerializeField] protected float delayBetween = 0f;
        [SerializeField] protected float delayAfter = 0f;
        [SerializeField] protected List<MonoBehaviour> nodes = new();

        protected virtual IEnumerable<MonoBehaviour> OrderedNodes => nodes;

        protected async UniTask DelaySafe(float seconds)
        {
            if (seconds > 0)
                await UniTask.Delay(TimeSpan.FromSeconds(seconds));
        }

        public void PreAnimation() => HandlePhase(n => n.PreAnimation());
        public async UniTask PlayAsync() => await HandlePlayAsync();
        public void AfterAnimation() => HandlePhase(n => n.AfterAnimation());

        private void HandlePhase(Action<TPhase> phaseAction)
        {
            foreach (var node in OrderedNodes)
            {
                if (node is PopupAnimationSequenceBase<TPhase> subSeq)
                    subSeq.HandlePhase(phaseAction);
                else if (node is TPhase phase)
                    phaseAction(phase);
            }
        }

        private async UniTask HandlePlayAsync()
        {
            await DelaySafe(delayBefore);
            var ordered = OrderedNodes.ToList();

            if (runSequentially)
            {
                foreach (var node in ordered)
                {
                    if (node is PopupAnimationSequenceBase<TPhase> subSeq)
                        await subSeq.PlayAsync();
                    else if (node is TPhase phase)
                        await phase.PlayAsync();

                    await DelaySafe(delayBetween);
                }
            }
            else
            {
                var tasks = ordered.Select(node =>
                    node is PopupAnimationSequenceBase<TPhase> subSeq
                        ? subSeq.PlayAsync()
                        : (node is TPhase phase ? phase.PlayAsync() : UniTask.CompletedTask));

                await UniTask.WhenAll(tasks);
            }

            await DelaySafe(delayAfter);
        }

#if UNITY_EDITOR
        public void AddNode(MonoBehaviour node)
        {
            if (node == null) return;
            if (!nodes.Contains(node))
            {
                Undo.RecordObject(this, "Add Node");
                nodes.Add(node);
                EditorUtility.SetDirty(this);
                Debug.Log($"[PopupAnimationSequenceBase] Added node {node.name} → {name}");
            }
        }

        public void RemoveNode(MonoBehaviour node)
        {
            if (node == null) return;
            if (nodes.Contains(node))
            {
                Undo.RecordObject(this, "Remove Node");
                nodes.Remove(node);
                EditorUtility.SetDirty(this);
                Debug.Log($"[PopupAnimationSequenceBase] Removed node {node.name} → {name}");
            }
        }
#endif
    }
}