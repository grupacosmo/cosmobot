using TMPro;
using UnityEngine;

namespace Cosmobot
{
    public class ResourceTextHandler : MonoBehaviour
    {
        [SerializeField] private Transform text;
        [SerializeField] private TextMeshPro displyText;
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
            displyText.text = resourceAmount.ToString();
            UpdateText(resourceAmount);
        }

        public void UpdateText(int newAmount) {
            displyText.text = $"{newAmount}";
        }
    }
}
