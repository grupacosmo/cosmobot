using UnityEngine;
using UnityEngine.Events;

namespace Cosmobot
{
    [RequireComponent(typeof(Collider))]
    public class SimpleEventInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private string prompt = "Press to interact";

        [SerializeField]
        private UnityEvent onUse;

        public string Prompt => prompt;

        public void Use()
        {
            onUse?.Invoke();
        }
    }
}
