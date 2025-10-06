using UnityEngine;

namespace Cosmobot
{
    public class AutoClicker : MonoBehaviour
    {
        [SerializeField]
        private float clickInterval = 1f;

        private float currentClickInterval = 1f;
        public IInteractable target { get; set; }

        private void Awake()
        {
            target = GetComponent<IInteractable>();
        }

        private void Update()
        {
            currentClickInterval -= Time.deltaTime;
            if (currentClickInterval <= 0)
            {
                target.Use();
                currentClickInterval = clickInterval;
            }
        }
    }
}
