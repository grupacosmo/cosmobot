using Cosmobot.BuildingSystem;
using UnityEngine;

namespace Cosmobot
{
    public class ConstructionPreview : MonoBehaviour
    {
        [SerializeField] MeshRenderer gridDisplayRenderer;
        [SerializeField] Transform gridDisplayTransform;

        public void SetBuilding(BuildingInfo buildingInfo) 
        {
            transform.localScale = new Vector3(buildingInfo.Prefab.transform.localScale.x * GlobalConstants.GRID_CELL_SIZE, 1, buildingInfo.Prefab.transform.localScale.z * GlobalConstants.GRID_CELL_SIZE);
        }

        public void SetPosition(Vector3 newPosition) 
        {
            transform.position = newPosition + new Vector3(0, 0.5f, 0);
        }

        public void SetRotation(Quaternion currentConstructionRotation)
        {
            transform.rotation = currentConstructionRotation; // TODO: put a tween/animation
        }

        public void SetGridPosition(Vector4 newPosition)
        {
            gridDisplayRenderer.material.SetVector("_Center", newPosition);
            gridDisplayTransform.position = newPosition;
        }
    }
}
