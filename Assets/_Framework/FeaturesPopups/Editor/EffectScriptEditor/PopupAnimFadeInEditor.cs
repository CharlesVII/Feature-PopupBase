#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Features.Popups.EditorTools
{
    [CustomEditor(typeof(PopupAnimFadeIn))]
    public class PopupAnimFadeInEditor : BasePopupAnimationEditor<PopupAnimFadeIn>
    {
        protected override void DrawInspectorContent(PopupAnimFadeIn anim, bool horizontal)
        {
            if (horizontal)
            {
                EditorGUILayoutEx.AutoWidthRow(
                    new float[] { 0.3f, 0.1f, 0.1f, 0.1f, 0.25f, 0.15f },
                    new System.Action<Rect>[]
                    {
                        rect => anim.targetCanvasGroup = (CanvasGroup)EditorGUI.ObjectField(rect, anim.targetCanvasGroup, typeof(CanvasGroup), true),
                        rect => anim.startAlpha = EditorGUI.FloatField(rect, anim.startAlpha),
                        rect => anim.endAlpha = EditorGUI.FloatField(rect, anim.endAlpha),
                        rect => anim.duration = EditorGUI.FloatField(rect, anim.duration),
                        rect => anim.ease = (DG.Tweening.Ease)EditorGUI.EnumPopup(rect, anim.ease),
                        rect => GUI.Label(rect, "Fade In", EditorStyles.miniLabel)
                    });
            }
            else
            {
                anim.targetCanvasGroup = (CanvasGroup)EditorGUILayout.ObjectField("Target", anim.targetCanvasGroup, typeof(CanvasGroup), true);
                anim.startAlpha = EditorGUILayout.FloatField("Start Alpha", anim.startAlpha);
                anim.endAlpha = EditorGUILayout.FloatField("End Alpha", anim.endAlpha);
                anim.duration = EditorGUILayout.FloatField("Duration", anim.duration);
                anim.ease = (DG.Tweening.Ease)EditorGUILayout.EnumPopup("Ease", anim.ease);
            }
        }
    }
}
#endif
