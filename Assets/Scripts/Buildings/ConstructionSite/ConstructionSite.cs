using UnityEngine;
using Cosmobot.BuildingSystem;
using System.Linq;
using Cosmobot.ItemSystem;
using System.Collections.Generic;

namespace Cosmobot
{
    public class ConstructionSite : BaseBuilding
    {    
        [SerializeField] private Transform tempCube;
        [SerializeField] private SerializableDictionary<ItemInfo, GameObject> constructionSiteResources = new();
        [SerializeField] private GameObject[] previewObjects;
        [SerializeField] public GameObject resourcePreview;
        public Transform TempCube => tempCube;
        public SerializableDictionary<ItemInfo, GameObject> ConstructionSiteResources => constructionSiteResources;

        public void SetRequiredResources(SerializableDictionary<ItemInfo, int> requiredResources) {
            int previewCount = 0;
            previewObjects = new GameObject[requiredResources.Count];
            foreach (KeyValuePair<ItemInfo, int> pair in requiredResources) {
                GameObject preview = CreateResourcePreview(requiredResources, previewCount);
                preview.transform.SetParent(transform);
                preview.GetComponent<ResourcePreviewController>().SetRequirement(pair.Value);
                previewObjects.SetValue(preview, previewCount);
                constructionSiteResources[pair.Key] = preview;
                previewCount++;
            }
        }
        
        private GameObject CreateResourcePreview(SerializableDictionary<ItemInfo, int> resources, int previewCount) {
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

        public void DecreaseResourceRequirement(ItemInfo resourceToDecrease, int resourceAmount = 1) {
            GameObject previewToDecrease = constructionSiteResources[resourceToDecrease];
            previewToDecrease.GetComponent<ResourcePreviewController>().DecreaseRequirement(resourceAmount);
        }

        public void Initialize(BuildingInfo newBuildingInfo) {
            buildingInfo = newBuildingInfo;
            tempCube.localScale = new Vector3(buildingInfo.GridSize.x, 0.2f, buildingInfo.GridSize.y);
        }

        public void IsReadyToBuild(GameObject targetBuilding) {
            foreach (GameObject resource in previewObjects)
                if (resource.GetComponent<ResourcePreviewController>().ResourceRequirement > 0) return;
            
            Build(targetBuilding);
        }

        public void FinishConstruction() {
            Instantiate(buildingInfo.Prefab, transform.position, transform.rotation); // TODO: set instantiation parent
            Despawn();
        }

        public void DestroyPreview() {
            foreach (GameObject resource in previewObjects) {
                Destroy(resource);
            }
        }

        public void Build(GameObject targetBuilding) {
            DestroyPreview();
            Instantiate(targetBuilding, transform.position, transform.rotation);
            Despawn();
        }
    }
}
