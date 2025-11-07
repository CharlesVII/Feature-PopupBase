using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Features.Popups
{
    public class PopupManager : Singleton<PopupManager>
    {
        [SerializeField] private Transform holderRoot;
        [SerializeField] private Transform holderActive;
        [SerializeField] private List<PopupBehaviour> popupPrefabs;

        private Dictionary<Type, PopupBehaviour> popupDictionary;
        private HashSet<PopupBehaviour> popupActiveSet;

        private void Start()
        {
            if (holderRoot == null)
                holderRoot = transform;
            if (holderActive == null)
                holderActive = transform;

            popupDictionary = new();
            popupActiveSet = new();
            InitializePopups();
        }

        private void InitializePopups()
        {
            popupDictionary = new();

            for (int i = 0; i < popupPrefabs.Count; i++)
            {
                PopupBehaviour prefab = popupPrefabs[i];
                if (prefab == null)
                {
                    PopupDebugLogger.LogWarning("[PopupManager] Phát hiện prefab null trong danh sách popupPrefabs. Bỏ qua.");
                    continue;
                }
                Type type = prefab.GetType();

                if (popupDictionary.ContainsKey(type))
                {
                    PopupDebugLogger.LogWarning($"[PopupManager] Phát hiện loại popup trùng lặp: {type.Name}. Bỏ qua khởi tạo.");
                    continue;
                }

                popupDictionary[type] = prefab;
                PopupDebugLogger.Log($"[PopupManager] Đã đăng ký prefab popup loại {type.Name}.");
            }
            PopupDebugLogger.Log($"[PopupManager] Đã khởi tạo {popupDictionary.Count} prefab popup.");
        }

        public T GetPopup<T>() where T : PopupBehaviour
        {
            Type type = typeof(T);
            if (!popupDictionary.TryGetValue(type, out var prefab))
            {
                PopupDebugLogger.LogWarning($"Popup loại {type.Name} chưa được đăng ký.");
                return null;
            }

            foreach (var activePopup in popupActiveSet)
            {
                if (activePopup.GetType() == type)
                    return activePopup as T;
            }

            var instance = Instantiate(prefab, holderRoot);
            popupDictionary[type] = instance;
            return instance as T;
        }

        public async UniTask ShowPopup<T, TData>(TData data) where T : PopupBase<TData>
        {
            var popup = GetPopup<T>();
            if (popup == null)
            {
                PopupDebugLogger.LogError($"[PopupManager] Không tìm thấy popup loại {typeof(T).Name}.");
                return;
            }

            bool isPopupActive = IsPopupActiveState(popup);
            if (isPopupActive)
            {
                PopupDebugLogger.LogWarning($"[PopupManager] Popup loại {typeof(T).Name} đã được kích hoạt.");
                return;
            }

            if (!popupActiveSet.Contains(popup))
                popupActiveSet.Add(popup);

            popup.transform.SetParent(holderActive);
            popup.gameObject.SetActive(true);
            await popup.Show(data);
        }

        public async UniTask HidePopup<T>() where T : PopupBehaviour
        {
            var popup = GetPopup<T>();
            if (popup == null)
            {
                PopupDebugLogger.LogError($"[PopupManager] Không tìm thấy popup loại {typeof(T).Name}.");
                return;
            }

            bool isPopupActive = IsPopupActiveState(popup);   
            if (!isPopupActive)
            {
                PopupDebugLogger.LogWarning($"[PopupManager] Popup loại {typeof(T).Name} không được kích hoạt.");
                return;
            }

            if (popup is PopupBase<object> popupBase)
                await popupBase.Hide();
            else
                await UniTask.CompletedTask;

            popup.gameObject.SetActive(false);
            popup.transform.SetParent(holderRoot);
            popupActiveSet.Remove(popup);
        }

        public bool IsPopupActive<T>() where T : PopupBehaviour
        {
            var popup = GetPopup<T>();
            if (popup == null) return false;

            return true;
        }

        public bool IsAnyPopupActive() => popupActiveSet.Count > 0;

        private bool IsPopupActiveState(PopupBehaviour popup)
        {
            bool isPopupAlreadyActive = popup.PopupState == PopupState.Showing
               || popup.PopupState == PopupState.Shown
               || popup.PopupState == PopupState.Hiding;

            return isPopupAlreadyActive;
        }
    }
}
