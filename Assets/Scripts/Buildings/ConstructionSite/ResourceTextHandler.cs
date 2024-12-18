using TMPro;
using UnityEngine;

namespace Cosmobot
{
    public class ResourceTextHandler : MonoBehaviour
    {
        [SerializeField] private Transform text;
        [SerializeField] private TextMeshPro displayText;
        private new Camera camera;
        void Start()
        {
            camera = Camera.main;
        }

        void Update()
        {
            text.LookAt(camera.transform);
            text.rotation = Quaternion.LookRotation(camera.transform.forward);
        }

        public void InitializeText(int resourceAmount) {
            displayText.text = resourceAmount.ToString();
            UpdateText(resourceAmount);
        }

        public void UpdateText(int newAmount) {
            displayText.text = $"{newAmount}";
        }
    }
}
