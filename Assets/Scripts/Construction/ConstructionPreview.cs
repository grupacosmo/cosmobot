using Cosmobot.BuildingSystem;
using UnityEngine;

namespace Cosmobot
{
    public class ConstructionPreview : MonoBehaviour
    {

        bool isActive = false;

        public void SetBuilding(BuildingInfo buildingInfo) {
            transform.localScale = new Vector3(buildingInfo.GridDimensions.x * GlobalConstants.GRID_SIZE, 1, buildingInfo.GridDimensions.y * GlobalConstants.GRID_SIZE);
        }

        public void SetPosition(Vector3 newPosition) {
            if (!isActive) return;
            transform.position = newPosition + new Vector3(0, 0.5f, 0);
        }
        public void SetRotation(Quaternion currentConstructionRotation)
        {
            transform.rotation = currentConstructionRotation; // TODO: put a tween/animation
        }
        public void SetActive(bool newActive) {
            isActive = newActive;
            if (!newActive) {
                transform.position = new Vector3(0, 10000f, 0);
            }
        }

        
    }
}
