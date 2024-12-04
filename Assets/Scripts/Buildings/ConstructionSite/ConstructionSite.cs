using UnityEngine;
using Cosmobot.BuildingSystem;

namespace Cosmobot
{
    public class ConstructionSite : BaseBuilding
    {    
        [SerializeField] private Transform tempCube;
        [SerializeField] private SerializableDictionary<string, int> constructionSiteResources = new();
        
        public Transform TempCube => tempCube;
        public void SetRequiredResources(SerializableDictionary<string, int> requiredResources) {
            constructionSiteResources = requiredResources;
        }
        public SerializableDictionary<string, int> ConstructionSiteResources => constructionSiteResources;

        public void Initialize(BuildingInfo newBuildingInfo) {
            buildingInfo = newBuildingInfo;
            tempCube.localScale = new Vector3(buildingInfo.GridDimensions.x, 0.2f, buildingInfo.GridDimensions.y);
        } 

        public void FinishConstruction() {
            Instantiate(buildingInfo.Prefab, transform.position, transform.rotation); // TODO: set instantiation parent
            Despawn();
        }
    }
}
