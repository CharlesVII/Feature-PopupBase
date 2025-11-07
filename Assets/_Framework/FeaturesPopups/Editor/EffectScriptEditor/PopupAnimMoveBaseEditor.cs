#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Features.Popups.Animations;
using DG.Tweening;

namespace Features.Popups.EditorTools
{
    [CustomEditor(typeof(PopupAnimMoveBase), true)]
    public class PopupAnimMoveBaseEditor : BasePopupAnimationEditor<PopupAnimMoveBase>
    {
        protected override void DrawInspectorContent(PopupAnimMoveBase anim, bool horizontal)
        {
            if (horizontal)
                DrawHorizontal(anim);
            else
                DrawVertical(anim);
        }

        // ======================================================
        // 🧱 Layout Tool (nằm ngang)
        // ======================================================
        private void DrawHorizontal(PopupAnimMoveBase anim)
        {
            EditorGUILayoutEx.AutoWidthRow(
                new float[] { 0.7f, 0.6f, 0.9f, 0.7f },
                new System.Action<Rect>[]
                {
            rect => DrawProp(rect, "duration"),
            rect => DrawProp(rect, "delay"),
            rect => DrawProp(rect, "ease"),
            rect => DrawProp(rect, "additionalDistance")
                },
                spacing: 6f
            );
        }


        // ======================================================
        // 🧱 Layout Inspector (nằm dọc)
        // ======================================================
        private void DrawVertical(PopupAnimMoveBase anim)
        {
            DrawPropertyIfExist("target", new GUIContent("Target"));
            DrawPropertyIfExist("duration", new GUIContent("Duration"));
            DrawPropertyIfExist("delay", new GUIContent("Delay"));
            DrawPropertyIfExist("ease", new GUIContent("Ease"));
            DrawPropertyIfExist("additionalDistance", new GUIContent("Extra Distance"));

            EditorGUILayout.Space(5);
            DrawDirectionPreview(anim);
        }

        // ======================================================
        // 🧩 Field Drawer Helper
        // ======================================================
        private void DrawProp(Rect rect, string name)
        {
            var prop = serializedObject.FindProperty(name);
            if (prop != null)
                EditorGUI.PropertyField(rect, prop, GUIContent.none);
        }

        // ======================================================
        // 🧭 Preview hướng di chuyển
        // ======================================================
        private void DrawDirectionPreview(PopupAnimMoveBase anim)
        {
            Rect previewRect = GUILayoutUtility.GetRect(100, 50);
            Handles.BeginGUI();
            Handles.color = new Color(0.4f, 0.8f, 1f);

            Vector3 center = previewRect.center;
            Vector3 dir = Vector3.up;

            if (anim is PopupAnimShowMoveUp || anim is PopupAnimHideMoveUp)
                dir = Vector3.up;
            else if (anim is PopupAnimShowMoveDown || anim is PopupAnimHideMoveDown)
                dir = Vector3.down;
            else if (anim is PopupAnimShowMoveLeft || anim is PopupAnimHideMoveLeft)
                dir = Vector3.left;
            else if (anim is PopupAnimShowMoveRight || anim is PopupAnimHideMoveRight)
                dir = Vector3.right;

            Vector3 end = center + dir * 20f;
            Handles.DrawAAPolyLine(3f, center, end);
            Handles.ArrowHandleCap(0, end, Quaternion.LookRotation(Vector3.forward, dir), 10f, EventType.Repaint);

            Handles.EndGUI();
            EditorGUILayout.HelpBox($"Direction: {dir}", MessageType.None);
        }
    }
}
#endif
