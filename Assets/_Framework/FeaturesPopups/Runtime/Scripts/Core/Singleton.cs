using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Features.Popups
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {

        private static T instance;
        [SerializeField] private bool dontDestroyOnLoad;

        public static T Instance
        {
            get => instance;
        }

        void Awake()
        {
            //DontDestroyOnLoad (gameObject);
            if (instance == null)
            {
                instance = this as T;

                if (dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }

                CustomAwake();

            }
            else
            {
                Destroy(gameObject);
            }
        }
        protected virtual void CustomAwake() { }
    }
}


