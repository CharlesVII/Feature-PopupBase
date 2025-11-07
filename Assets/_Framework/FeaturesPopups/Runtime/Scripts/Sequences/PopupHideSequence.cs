using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Features.Popups
{
    [AddComponentMenu("Features/Popups/PopupHideSequence")]
    public class PopupHideSequence : PopupAnimationSequenceBase<IHidePhase>
    {
        [Tooltip("Nếu bật, các node sẽ chạy theo thứ tự đảo ngược (child → root).")]
        [SerializeField] private bool reverseOrder = true;

        protected override IEnumerable<MonoBehaviour> OrderedNodes
            => reverseOrder ? nodes.AsEnumerable().Reverse() : nodes;
    
    }
}
