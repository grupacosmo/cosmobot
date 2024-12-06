using TMPro;
using UnityEngine;

namespace Cosmobot
{
    public class ResourceTextHandler : MonoBehaviour
    {
        [SerializeField] private Transform text;
        [SerializeField] private TextMeshPro displyText;
        [SerializeField] private int requiredResource;
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
            requiredResource = resourceAmount;
            displyText.text = resourceAmount.ToString();
            UpdateText();
        }

        public void UpdateText() {
            if (requiredResource >= 0)
                displyText.text = $"{requiredResource--}";
        }
    }
}
