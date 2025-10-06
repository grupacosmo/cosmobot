using UnityEngine;

namespace Cosmobot
{
    public class ResourcePreviewController : MonoBehaviour
    {
        [SerializeField]
        private int resourceRequirement;

        public int ResourceRequirement => resourceRequirement;

        public void SetRequirement(int requirement)
        {
            resourceRequirement = requirement;
        }

        public void DecreaseRequirement(int amount)
        {
            resourceRequirement -= amount;
            GetComponent<ResourceTextHandler>().UpdateText(resourceRequirement);
        }
    }
}
