using Cosmobot.BuildingSystem;
using UnityEngine;

namespace Cosmobot
{
    public class ConstructionPreview : MonoBehaviour
    {
        [SerializeField] MeshRenderer gridDisplayRenderer;
        [SerializeField] Transform gridDisplayTransform;
        [SerializeField] LayerMask collisionMask;
        [SerializeField] GameObject FloorObject;

        private bool isPlacementPositionValid = true;
        public bool IsPlacementPositionValid => isPlacementPositionValid;

        void LateUpdate()
        {
            IsPositionValid();
                
        }

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

        public void SetGridPosition(Vector4 newPosition)
        {
            gridDisplayRenderer.material.SetVector("_Center", newPosition);
            gridDisplayTransform.position = newPosition;
        }

        private void IsPositionValid()
        {
            Ray objectRay = new Ray(transform.position, Vector3.down);
            bool objectRaySuccess = Physics.Raycast(objectRay, out RaycastHit objectRayHit, 1f, collisionMask);
            if (objectRaySuccess && objectRayHit.transform != null && 
                (objectRayHit.transform.gameObject.name == "Floor element" || objectRayHit.transform.gameObject.name == "Terrain")) // should be replaced later by a standard floor element prefab
            {
                isPlacementPositionValid = true;
                return;
            }
            isPlacementPositionValid = false;
        }
    }
}
