using UnityEngine;

namespace Features.Popups
{
    [CreateAssetMenu(fileName = "PopupManagerSettings", menuName = "ScriptableObjects/PopupManagerSettings", order = 1)]
    public class PopupManagerSettings : ScriptableObject
    {
        public bool enableDebug;
    }
}