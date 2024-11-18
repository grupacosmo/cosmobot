using Cosmobot.BuildingSystem;
using UnityEngine;

namespace Cosmobot
{
    public class PlayerConstructionHandler : MonoBehaviour
    {

        [SerializeField] Transform cameraTransform;
        [SerializeField] Transform playerTransform;
        [SerializeField] Transform buildPointIndicator;
        [SerializeField] Transform buildPreview;
        [SerializeField] LayerMask buildTargetingCollisionMask;
        [SerializeField] float maxBuildDistance = 20.0f;
        [SerializeField] float maxTerrainHeight = 100.0f;
        [SerializeField] float gridSize = 1.0f;

        [SerializeField] BuildingInfo currentBuildingInfo;


        void LateUpdate()
        {
            if (currentBuildingInfo is not null) {
                ScanBuildingPlacement(GetBuildPoint(), currentBuildingInfo.GridDimensions);
            }
            
        }  

        // Select a building and start scanning for placement position
        // TODO: hook this up to a state machine or something so it works with the rest of the player mechanics
        public void InitiatePlacement(BuildingInfo buildingInfo) {
            currentBuildingInfo = buildingInfo;
        }
        
        // Place the construction plot
        public void ExecutePlacement() {
            ExitPlacement();
        }

        // Exit placement mode
        public void ExitPlacement() {
            currentBuildingInfo = null;
        }

        private Vector3 GetBuildPoint() {
            Ray cameraRay = new Ray(cameraTransform.position, cameraTransform.forward);
            bool cameraRaySuccess = Physics.Raycast(cameraRay, out RaycastHit cameraRayHit, maxBuildDistance * 2, buildTargetingCollisionMask);

            if (cameraRaySuccess && Vector3.ProjectOnPlane(cameraTransform.position - cameraRayHit.point, Vector3.up).magnitude < maxBuildDistance) {
                return new Vector3(cameraRayHit.point.x, 0, cameraRayHit.point.z);
            }

            return Vector3.ProjectOnPlane(cameraTransform.position, Vector3.up) + Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up) * maxBuildDistance;
        }

        private Vector3 SnapToGrid(Vector3 vec, bool centerX, bool centerZ) {
            vec /= gridSize;
            Vector3 newVec = new Vector3(Mathf.Round(vec.x), 0, Mathf.Round(vec.z));
            newVec *= gridSize;

            return newVec + new Vector3(centerX ? gridSize/2.0f : 0, 0, centerZ ? gridSize/2.0f : 0);
        }

        // Returns the ultimate placement position of the building
        private Vector3? ScanBuildingPlacement(Vector3 buildPoint, Vector2Int buildingGridDimensions) {
            Vector3 boxOrigin = new Vector3(buildPoint.x, maxTerrainHeight+1, buildPoint.z);
            Vector3 boxHalfExtents = new Vector3(buildingGridDimensions.x * gridSize * 0.5f, 1, buildingGridDimensions.y * gridSize * 0.5f);

            bool success = Physics.BoxCast(boxOrigin, boxHalfExtents, Vector3.down, out RaycastHit result, Quaternion.identity, maxTerrainHeight*2, buildTargetingCollisionMask);
            if (!success) {
                buildPreview.position = new Vector3(0, 10000, 0);
                return null;
            }

            Vector3 finalPlacementPosition = new Vector3(buildPoint.x, result.point.y, buildPoint.z);

            buildPreview.localScale = new Vector3(buildingGridDimensions.x * gridSize, 1, buildingGridDimensions.y * gridSize);

            buildPreview.position = new Vector3(buildPoint.x, result.point.y+0.5f, buildPoint.z);

            return finalPlacementPosition;
        }
    }
}
