using UnityEngine;
using Cosmobot.BuildingSystem;

namespace Cosmobot
{
    public class ConstructionSite : BaseBuilding
    {    
        [SerializeField] private Transform tempCube;

        public void Initialize(BuildingInfo newBuildingInfo) {
            buildingInfo = newBuildingInfo;
            tempCube.localScale = new Vector3(buildingInfo.GridSize.x, 0.2f, buildingInfo.GridSize.y);
        }

        public void FinishConstruction() {
            Instantiate(buildingInfo.Prefab, transform.position, transform.rotation); // TODO: set instantiation parent
            Despawn();
        }
    }
}
