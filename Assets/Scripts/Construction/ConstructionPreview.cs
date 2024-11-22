using Cosmobot.BuildingSystem;
using UnityEngine;

namespace Cosmobot
{
    public class ConstructionPreview : MonoBehaviour
    {

        bool isActive = false;

        public void SetBuilding(BuildingInfo buildingInfo, float gridSize) {
            transform.localScale = new Vector3(buildingInfo.GridDimensions.x * gridSize, 1, buildingInfo.GridDimensions.y * gridSize);
        }

        public void SetPosition(Vector3 newPosition) {
            if (!isActive) return;
            transform.position = newPosition + new Vector3(0, 0.5f, 0);
        }

        public void SetActive(bool newActive) {
            isActive = newActive;
            if (!newActive) {
                transform.position = new Vector3(0, 10000f, 0);
            }
        }
    }
}
