using Cosmobot.ItemSystem;
using UnityEngine;

namespace Cosmobot
{
    public class ResourcePreviewController : MonoBehaviour
    {
        [SerializeField]
        private int resourceRequirement;

        public int ResourceRequirement => resourceRequirement;

        public void SetRequirement(int requirement, ItemInfo previewObject)
        {
            resourceRequirement = requirement;
            Instantiate(previewObject.ModelPrefab, transform.position, transform.rotation, transform);
        }

        public void DecreaseRequirement(int amount)
        {
            resourceRequirement -= amount;
            GetComponent<ResourceTextHandler>().UpdateText(resourceRequirement);
        }
    }
}
