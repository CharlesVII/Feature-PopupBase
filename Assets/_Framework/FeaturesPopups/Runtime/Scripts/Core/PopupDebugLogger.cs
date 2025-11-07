using UnityEngine;

namespace Features.Popups
{
    public static class PopupDebugLogger
    {
        private static PopupManagerSettings settings;

        // Tải PopupManagerSettings từ Resources
        private static PopupManagerSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = Resources.Load<PopupManagerSettings>("PopupManagerSettings");
                    if (settings == null)
                    {
                        Debug.LogError("[PopupDebugLogger] PopupManagerSettings not found in Resources. Debug logging will be disabled.");
                        settings = ScriptableObject.CreateInstance<PopupManagerSettings>();
                        settings.enableDebug = false; // Mặc định tắt debug nếu không tìm thấy file cấu hình
                    }
                }
                return settings;
            }
        }

        public static void Log(string message, Color color = default)
        {
            if (!Settings.enableDebug) return;

            // Chuyển đổi Color thành chuỗi hex
            string colorHex = ColorUtility.ToHtmlStringRGB(color == default ? Color.yellow : color);

            Debug.Log($"<color=#{colorHex}>[PopupDebug] {message}</color>");
        }

        public static void LogWarning(string message)
        {
            if (!Settings.enableDebug) return;
            Debug.LogWarning($"<color=#FFA500>[PopupDebug WARNING] {message}</color>");
        }

        public static void LogError(string message)
        {
            if (!Settings.enableDebug) return;
            Debug.LogError($"<color=#FF0000>[PopupDebug ERROR] {message}</color>");
        }
    }
}