#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Features.Popups.EditorTools
{
    [CustomEditor(typeof(PopupAnimScaleOut))]
    public class PopupAnimScaleOutEditor : BasePopupAnimationEditor<PopupAnimScaleOut>
    {
        protected override void DrawInspectorContent(PopupAnimScaleOut anim, bool horizontal)
        {
            if (horizontal)
            {
                EditorGUILayoutEx.AutoWidthRow(
                    new float[] { 0.3f, 0.1f, 0.1f, 0.1f, 0.25f, 0.15f },
                    new System.Action<Rect>[]
                    {
                        rect => anim.target = (Transform)EditorGUI.ObjectField(rect, anim.target, typeof(Transform), true),
                        rect => anim.startScale = EditorGUI.FloatField(rect, anim.startScale),
                        rect => anim.endScale = EditorGUI.FloatField(rect, anim.endScale),
                        rect => anim.duration = EditorGUI.FloatField(rect, anim.duration),
                        rect => anim.ease = (DG.Tweening.Ease)EditorGUI.EnumPopup(rect, anim.ease),
                        rect => GUI.Label(rect, "Scale Out", EditorStyles.miniLabel)
                    });
            }
            else
            {
                anim.target = (Transform)EditorGUILayout.ObjectField("Target", anim.target, typeof(Transform), true);
                anim.startScale = EditorGUILayout.FloatField("Start Scale", anim.startScale);
                anim.endScale = EditorGUILayout.FloatField("End Scale", anim.endScale);
                anim.duration = EditorGUILayout.FloatField("Duration", anim.duration);
                anim.ease = (DG.Tweening.Ease)EditorGUILayout.EnumPopup("Ease", anim.ease);
            }
        }
    }
}
#endif
