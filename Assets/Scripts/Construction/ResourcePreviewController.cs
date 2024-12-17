using UnityEngine;

namespace Cosmobot
{
    public class ResourcePreviewController : MonoBehaviour
    {
        [SerializeField] private int resourceRequirement;

        public int ResourceRequirement => resourceRequirement;

        public void SetRequirement(int requirement) {
            resourceRequirement = requirement;
        }

        public void DecreaseRequirement(int amount) {
            if (amount <= resourceRequirement && resourceRequirement - amount >= 0) {
                resourceRequirement -= amount;
                GetComponent<ResourceTextHandler>().UpdateText(resourceRequirement);
            } else {
                Debug.LogWarning("Resources exceed requirements");
            }
        }
    }
}
