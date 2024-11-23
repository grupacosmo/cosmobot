using Cosmobot.BuildingSystem;
using UnityEngine;

namespace Cosmobot
{
    public class PlayerConstructionHandler : MonoBehaviour, DefaultInputActions.IBuildingPlacementActions
    {

        [SerializeField] Transform cameraTransform;
        [SerializeField] Transform playerTransform;
        [SerializeField] ConstructionPreview constructionPreview;
        [SerializeField] LayerMask buildTargetingCollisionMask;
        [SerializeField] float maxBuildDistance = 20.0f;
        [SerializeField] float maxTerrainHeight = 100.0f;
        [SerializeField] float gridSize = 1.0f;
        [SerializeField] bool isPlacementActive;

        [SerializeField] BuildingInfo initialBuilding; // TEMP

        private DefaultInputActions actions;

        private BuildingInfo currentBuildingInfo;
        private Quaternion currentConstructionRotation = Quaternion.identity;

        void Start() {
            InitiatePlacement(initialBuilding); // TEMP
        }

        void LateUpdate()
        {
            if (isPlacementActive) {
                ScanBuildingPlacement(GetBuildPoint(), currentBuildingInfo.GridDimensions, currentConstructionRotation);
            }
        }  

        // Select a building and start scanning for placement position
        // TODO: hook this up to a state machine or something so it works with the rest of the player mechanics
        public void InitiatePlacement(BuildingInfo buildingInfo) {
            currentBuildingInfo = buildingInfo;
            isPlacementActive = true;
            constructionPreview.SetBuilding(buildingInfo, gridSize);
            constructionPreview.SetActive(true);
            currentConstructionRotation = Quaternion.identity;
        }
        
        // Place the construction plot
        private void ExecutePlacement() {
            if (!isPlacementActive){
                Debug.LogWarning("Attempted to place building outside of placement mode!");
                return;
            }
            ExitPlacement();
        }

        // Exit placement mode
        public void ExitPlacement() {
            currentBuildingInfo = null;
            isPlacementActive = false;
            constructionPreview.SetActive(false);
        }

        private void RotatePlacement(bool reverse=false) {
            currentConstructionRotation *= Quaternion.Euler(0, reverse ? -90 : 90, 0);
            constructionPreview.SetRotation(currentConstructionRotation);
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
        private Vector3? ScanBuildingPlacement(Vector3 buildPoint, Vector2Int buildingGridDimensions, Quaternion buildingRotation) {
            Vector3 boxOrigin = new Vector3(buildPoint.x, maxTerrainHeight+1, buildPoint.z);
            Vector3 boxHalfExtents = new Vector3(buildingGridDimensions.x * gridSize * 0.5f, 1, buildingGridDimensions.y * gridSize * 0.5f);

            bool success = Physics.BoxCast(boxOrigin, boxHalfExtents, Vector3.down, out RaycastHit result, buildingRotation, maxTerrainHeight*2, buildTargetingCollisionMask);
            if (!success) {
                constructionPreview.SetActive(false);
                return null;
            }
            constructionPreview.SetActive(true);

            Vector3 finalPlacementPosition = new Vector3(buildPoint.x, result.point.y, buildPoint.z);

            constructionPreview.SetPosition(finalPlacementPosition);

            return finalPlacementPosition;
        }

        private void OnEnable()
        {
            if (actions is null)
            {
                actions = new DefaultInputActions();
                actions.BuildingPlacement.SetCallbacks(this);
            }

            actions.BuildingPlacement.Enable();
        }

        public void OnCancelPlacement(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.performed) 
                ExitPlacement();
        }

        public void OnConfirmPlacement(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.performed)
                ExecutePlacement();
        }

        public void OnRotatePlacement(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.performed)
                RotatePlacement();
        }
    }
}
