using UnityEngine;
using Cosmobot.BuildingSystem;
using System.Linq;

namespace Cosmobot
{
    public class ConstructionSite : BaseBuilding
    {    
        [SerializeField] private Transform tempCube;
        [SerializeField] private SerializableDictionary<string, int> constructionSiteResources = new();
        private GameObject[] previewObjects;
        [SerializeField] public GameObject resourcePreview;
        public Transform TempCube => tempCube;
        public SerializableDictionary<string, int> ConstructionSiteResources => constructionSiteResources;

        public void SetRequiredResources(SerializableDictionary<string, int> requiredResources) {
            constructionSiteResources = requiredResources;
            previewObjects = new GameObject[constructionSiteResources.Count];
        }

        void Start() {
            CreateResourcePreview();
        }
        
        private void CreateResourcePreview() {
            Vector3 sitePosition = TempCube.transform.position;
            float height = 0.5f; // TEMP
            if (constructionSiteResources.Count == 1) {
                GameObject resource = Instantiate(resourcePreview, new Vector3(sitePosition.x, sitePosition.y + height, sitePosition.z), transform.rotation);
                resource.GetComponentInChildren<ResourceTextHandler>().InitializeText(constructionSiteResources.ElementAt(0).Value);
                previewObjects.SetValue(resource, 0);
                return;
            }
            
            float siteWidth = 
            transform.eulerAngles.y == 0 || transform.eulerAngles.y == 180 ? GetComponentInChildren<MeshRenderer>().bounds.size.x : 
            transform.eulerAngles.y == 90 || transform.eulerAngles.y == 270 ? GetComponentInChildren<MeshRenderer>().bounds.size.z : 0;
            float distance = siteWidth / (constructionSiteResources.Count - 1);
            
            for (int i = 0; i < constructionSiteResources.Count; i++) {
                float x = sitePosition.x;
                float z = sitePosition.z;
                if (transform.eulerAngles.y == 0 || transform.eulerAngles.y == 180) {
                    z += -siteWidth / 2 + i * distance;
                } else {  
                    x += -siteWidth / 2 + i * distance;
                }

                GameObject resource = Instantiate(resourcePreview, new Vector3(x, sitePosition.y + height, z), transform.rotation);
                resource.GetComponentInChildren<ResourceTextHandler>().InitializeText(constructionSiteResources.ElementAt(i).Value);
                previewObjects.SetValue(resource, i);
            }
        }

        public void DecreaseResourceRequirement() {
            foreach (var resource in previewObjects) {
                resource.GetComponentInChildren<ResourceTextHandler>().UpdateText();
            }
        }

        public void Initialize(BuildingInfo newBuildingInfo) {
            buildingInfo = newBuildingInfo;
            tempCube.localScale = new Vector3(buildingInfo.GridSize.x, 0.2f, buildingInfo.GridSize.y);
        }

        public void FinishConstruction() {
            Instantiate(buildingInfo.Prefab, transform.position, transform.rotation); // TODO: set instantiation parent
            Despawn();
        }

        public void DestroyPreview() {
            foreach (var resource in previewObjects) {
                Destroy(resource);
            }
        }
    }
}
