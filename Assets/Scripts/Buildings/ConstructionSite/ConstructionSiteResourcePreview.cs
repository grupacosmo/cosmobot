using System.Linq;
using Cosmobot.ItemSystem;
using UnityEngine;

namespace Cosmobot
{
    public class ConstructionSiteResourcePreview : MonoBehaviour
    {
        [SerializeField] private GameObject[] previewObjects;
        [SerializeField] private GameObject resourcePreview;

        public GameObject[] PreviewObjects => previewObjects;
        public GameObject ResourcePreview => ResourcePreview;

        public void SetPreviewObjects(GameObject[] previewObjects) {
            this.previewObjects = previewObjects;
        } 

        public GameObject CreateResourcePreview(SerializableDictionary<ItemInfo, int> resources, int previewCount) {
            Vector3 sitePosition = transform.position;
            GameObject previewObject;
            float height = 0.5f; // TEMP
            if (resources.Count == 1) {
                previewObject = Instantiate(resourcePreview, new Vector3(sitePosition.x, sitePosition.y + height, sitePosition.z), transform.rotation);
                previewObject.GetComponentInChildren<ResourceTextHandler>().InitializeText(resources.ElementAt(0).Value);
                return previewObject;
            }
            
            float siteWidth = 
                transform.eulerAngles.y == 0 || transform.eulerAngles.y == 180 ? GetComponentInChildren<MeshRenderer>().bounds.size.x : 
                transform.eulerAngles.y == 90 || transform.eulerAngles.y == 270 ? GetComponentInChildren<MeshRenderer>().bounds.size.z : 0;
            float distance = siteWidth / (resources.Count - 1);
            float x = sitePosition.x;
            float z = sitePosition.z;
            if (transform.eulerAngles.y == 0 || transform.eulerAngles.y == 180) {
                z += -siteWidth / 2 + previewCount * distance;
            } else {  
                x += -siteWidth / 2 + previewCount * distance;
            }

            previewObject = Instantiate(resourcePreview, new Vector3(x, sitePosition.y + height, z), transform.rotation);
            previewObject.GetComponentInChildren<ResourceTextHandler>().InitializeText(resources.ElementAt(previewCount).Value);
            return previewObject;
        }
    }
}
