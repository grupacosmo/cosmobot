using System.Linq;
using Cosmobot.ItemSystem;
using Cosmobot.Utils;
using UnityEngine;

namespace Cosmobot
{
    public class ConstructionSiteResourcePreview : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] previewObjects;

        [SerializeField]
        private GameObject resourcePreview;

        public GameObject[] PreviewObjects => previewObjects;
        public GameObject ResourcePreview => ResourcePreview;

        public void SetPreviewObjects(GameObject[] previewObjects)
        {
            this.previewObjects = previewObjects;
        }

        public GameObject CreateResourcePreview(SerializableDictionary<ItemInfo, int> resources, int previewCount)
        {
            Vector3 sitePosition = transform.position;
            float spacing = 0.6f;
            float heightOffset = 0.5f;
            float siteWidth = (resources.Count - 1) * spacing;
            float startPosX = sitePosition.x - siteWidth / 2;
            float startPosZ = sitePosition.z - siteWidth / 2;

            if (Mathf.Approximately(transform.eulerAngles.y, 0) || Mathf.Approximately(transform.eulerAngles.y, 180))
            {
                sitePosition.z = startPosZ + previewCount * spacing;
            }
            else
            {
                sitePosition.x = startPosX + previewCount * spacing;
            }

            GameObject previewObject = Instantiate(resourcePreview,
                new Vector3(sitePosition.x, sitePosition.y + heightOffset, sitePosition.z), transform.rotation);
            previewObject.GetComponentInChildren<ResourceTextHandler>()
                .InitializeText(resources.ElementAt(previewCount).Value);
            return previewObject;
        }
    }
}
