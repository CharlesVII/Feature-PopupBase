#if UNITY_EDITOR
using DG.Tweening;
using System;
using UnityEditor;
using UnityEngine;

namespace Features.Popups.EditorTools
{
    [CustomEditor(typeof(PopupAnimScaleIn))]
    public class PopupAnimScaleInEditor : BasePopupAnimationEditor<PopupAnimScaleIn>
    {
        protected override void DrawInspectorContent(PopupAnimScaleIn anim, bool horizontal)
        {
            if (horizontal)
            {
                EditorGUILayoutEx.AutoWidthRow(
                    new float[] { 0.35f, 0.1f, 0.1f, 0.1f, 0.25f },
                    new Action<Rect>[]
                    {
                        rect => anim.target = (Transform)EditorGUI.ObjectField(rect, anim.target, typeof(Transform), true),
                        rect => anim.startScale = EditorGUI.FloatField(rect, anim.startScale),
                        rect => anim.endScale = EditorGUI.FloatField(rect, anim.endScale),
                        rect => anim.duration = EditorGUI.FloatField(rect, anim.duration),
                        rect => anim.ease = (Ease)EditorGUI.EnumPopup(rect, anim.ease)
                    });
            }
            else
            {
                anim.target = (Transform)EditorGUILayout.ObjectField("Target", anim.target, typeof(Transform), true);
                anim.startScale = EditorGUILayout.FloatField("Start Scale", anim.startScale);
                anim.endScale = EditorGUILayout.FloatField("End Scale", anim.endScale);
                anim.duration = EditorGUILayout.FloatField("Duration", anim.duration);
                anim.ease = (Ease)EditorGUILayout.EnumPopup("Ease", anim.ease);
            }
        }
    }
}
#endif
