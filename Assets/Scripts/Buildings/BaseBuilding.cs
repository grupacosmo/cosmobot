using Cosmobot.BuildingSystem;
using PlasticGui.WorkspaceWindow;
using UnityEngine;

namespace Cosmobot
{
    public class BaseBuilding : MonoBehaviour
    {

        public BuildingInfo buildingInfo {get; protected set;}

        public Vector2Int GridDimensions => buildingInfo.GridDimensions;

        // Remove the building (does not drop resources or anything like that)
        public void Despawn() {
            Destroy(gameObject);
        }
    }
}
