using Cosmobot.BuildingSystem;
using UnityEngine;

namespace Cosmobot
{
    public class ConstructionPreview : MonoBehaviour
    {
        public void SetBuilding(BuildingInfo buildingInfo) 
        {
            transform.localScale = new Vector3(buildingInfo.GridSize.x * GlobalConstants.GRID_CELL_SIZE, 1, buildingInfo.GridSize.y * GlobalConstants.GRID_CELL_SIZE);
        }

        public void SetPosition(Vector3 newPosition) 
        {
            transform.position = newPosition + new Vector3(0, 0.5f, 0);
        }

        public void SetRotation(Quaternion currentConstructionRotation)
        {
            transform.rotation = currentConstructionRotation; // TODO: put a tween/animation
        }   
    }
}
