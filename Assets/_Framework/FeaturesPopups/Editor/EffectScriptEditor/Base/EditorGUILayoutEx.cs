#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

namespace Features.Popups.EditorTools
{
    /// <summary>
    /// 🎨 Helper cho layout ngang tĩnh (vẽ gọn như bảng property trong ToolWindow)
    /// </summary>
    public static class EditorGUILayoutEx
    {
        public static void AutoWidthRow(float[] ratios, Action<Rect>[] draws, float spacing = 4f, float padding = 6f)
        {
            if (draws == null || draws.Length == 0) return;

            if (ratios == null || ratios.Length != draws.Length)
            {
                ratios = new float[draws.Length];
                for (int i = 0; i < ratios.Length; i++)
                    ratios[i] = 1f;
            }

            float totalRatio = 0f;
            foreach (var r in ratios) totalRatio += Mathf.Max(0.01f, r);

            // 🔹 Lấy vùng thực tế của hàng hiện tại
            Rect fullRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
            fullRect.x += padding;
            fullRect.width -= padding * 2;
            fullRect.height = EditorGUIUtility.singleLineHeight;

            float totalSpacing = spacing * (draws.Length - 1);
            float available = fullRect.width - totalSpacing;
            float x = fullRect.x;

            // 🔹 Vẽ thủ công từng ô (EditorGUI, không auto-layout)
            for (int i = 0; i < draws.Length; i++)
            {
                float width = available * (ratios[i] / totalRatio);
                Rect slice = new Rect(x, fullRect.y, width, fullRect.height);
                draws[i]?.Invoke(slice);
                x += width + spacing;
            }

            GUILayout.Space(fullRect.height + 2);
        }

        public static void EvenRow(params Action<Rect>[] draws)
        {
            if (draws == null || draws.Length == 0) return;
            float[] ratios = new float[draws.Length];
            for (int i = 0; i < ratios.Length; i++) ratios[i] = 1f;
            AutoWidthRow(ratios, draws);
        }
    }
}
#endif
