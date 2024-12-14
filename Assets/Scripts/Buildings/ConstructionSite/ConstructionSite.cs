using UnityEngine;
using Cosmobot.BuildingSystem;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;

namespace Cosmobot
{
    public class ConstructionSite : BaseBuilding
    {    
        [SerializeField] private Transform tempCube;
        [SerializeField] private SerializableDictionary<string, int> constructionSiteResources = new();
        [SerializeField] private GameObject[] previewObjects = new GameObject[2];
        [SerializeField] public GameObject resourcePreview;
        public Transform TempCube => tempCube;
        public SerializableDictionary<string, int> ConstructionSiteResources => constructionSiteResources;

        public void SetRequiredResources(SerializableDictionary<string, int> requiredResources) {
            constructionSiteResources = requiredResources;
        }

        void Start() {
            Vector3 sitePosition = TempCube.transform.position;
            float distance = 0.5f; // TEMP
            float height = 0.5f; // TEMP
            
            // Demo objects of what resources will be needed for the building
            GameObject resourceOne = Instantiate(resourcePreview, new Vector3(
                sitePosition.x + (transform.eulerAngles.y == 90 ? -distance : transform.eulerAngles.y == 270 ? distance : 0), sitePosition.y + height, 
                sitePosition.z + (transform.eulerAngles.y == 0 ? distance : transform.eulerAngles.y == 180 ? -distance : 0)), transform.rotation);
            GameObject resourceTwo = Instantiate(resourcePreview, new Vector3(
                sitePosition.x + (transform.eulerAngles.y == 90 ? distance : transform.eulerAngles.y == 270 ? -distance : 0), sitePosition.y + height, 
                sitePosition.z + (transform.eulerAngles.y == 0 ? -distance : transform.eulerAngles.y == 180 ? distance : 0)), transform.rotation);

            // TEMP, in future should be set based on resource type amount
            resourceOne.GetComponentInChildren<ResourceTextHandler>().InitializeText(constructionSiteResources.ElementAt(0).Value);
            resourceTwo.GetComponentInChildren<ResourceTextHandler>().InitializeText(constructionSiteResources.ElementAt(1).Value);

            previewObjects.SetValue(resourceOne, 0);
            previewObjects.SetValue(resourceTwo, 1);
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
