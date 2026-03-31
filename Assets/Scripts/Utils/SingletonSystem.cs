using UnityEditor;
using UnityEngine;

namespace Cosmobot.Utils
{
    public abstract class SingletonSystem<T> : MonoBehaviour where T : SingletonSystem<T>
    {
        public static T Instance { get; private set; }

        protected void Awake()
        {
            if (Instance is null)
            {
                Instance = (T)this;
                DontDestroyOnLoad(this);
                SystemAwake();
                Debug.Log($"SingletonSystem {typeof(T).Name} registered", this);
                return;
            }

            Debug.Log(
                $"Destroying duplicate SingletonSystem {typeof(T).Name} ({gameObject.name})",
                gameObject);
            Destroy(this);
        }

        protected virtual void OnDisable()
        {
            if (!ReferenceEquals(Instance, this)) return;

#if UNITY_EDITOR
            // prevent log warning (and keeping enabled) when exiting playmode
            if (Application.exitCancellationToken.IsCancellationRequested) return;
#endif
            enabled = true;
            Debug.LogWarning($"SingletonSystem {typeof(T).Name} cannot be disabled", this);
        }

        protected void OnDestroy()
        {
            if (ReferenceEquals(Instance, this))
            {
                SystemOnDestroy();
                Instance = null;
#if UNITY_EDITOR
                // prevent log warning when exiting playmode
                if (Application.exitCancellationToken.IsCancellationRequested) return;
#endif
                Debug.LogWarning($"SingletonSystem {typeof(T).Name} unregistered", this);
            }
        }

        protected virtual void SystemAwake() { }
        protected virtual void SystemOnDestroy() { }
    }
}
