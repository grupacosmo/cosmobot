using Cosmobot.BuildingSystem;
using UnityEngine;

namespace Cosmobot
{
    public class PlayerConstructionHandler : MonoBehaviour, DefaultInputActions.IBuildingPlacementActions
    {
        [SerializeField] Transform cameraTransform;
        [SerializeField] ConstructionPreview constructionPreview;
        [SerializeField] LayerMask buildTargetingCollisionMask;
        [SerializeField] float maxBuildDistance = 20.0f;
        [SerializeField] float maxTerrainHeight = 100.0f;
        [SerializeField] GameObject constructionSitePrefab;
        [SerializeField] GameObject BuildingSelectionUI;

        private DefaultInputActions actions;
        private Vector3? currentPlacementPosition;
        private BuildingInfo currentBuildingInfo;
        private int currentConstructionRotationSteps = 0; // multiply by 90deg to get actual rotation
        private Quaternion CurrentConstructionRotation => Quaternion.Euler(0, currentConstructionRotationSteps * 90.0f, 0);
        private bool isPlacementActive = false;

        void LateUpdate()
        {
            if (currentBuildingInfo != null)
            {
                InitiatePlacement(currentBuildingInfo);
                ProcessPlacement();
            }
        }

        public void SetBuilding(BuildingInfo buildingInfo) 
        { 
            currentBuildingInfo = buildingInfo; 
        }

        // Select a building and start scanning for placement position
        // TODO: hook this up to a state machine or something so it works with the rest of the player mechanics
        public void InitiatePlacement(BuildingInfo buildingInfo) 
        {
            currentBuildingInfo = buildingInfo;
            isPlacementActive = true;
            constructionPreview.SetBuilding(buildingInfo);
        }
        
        // Place the construction plot
        private void ExecutePlacement() 
        {
            if (isPlacementActive == false){
                Debug.LogWarning("Attempted to place building outside of placement mode!");
                return;
            }
            if (currentPlacementPosition is null) {
                Debug.LogWarning("Attempted to place building in impossible position");
                ExitPlacement();
                return;
            }
            if (IsPlacementPositionValid() == false)
            {
                Debug.LogWarning("Attempted to place building in invalid position!");
                return;
            }

            GameObject newSite = Instantiate(constructionSitePrefab, (Vector3)currentPlacementPosition, CurrentConstructionRotation);
            newSite.GetComponent<ConstructionSite>().Initialize(currentBuildingInfo);

            ExitPlacement();
        }

        // Exit placement mode
        public void ExitPlacement() 
        {
            currentBuildingInfo = null;
            isPlacementActive = false;
            constructionPreview.gameObject.SetActive(false);
        }

        private void ProcessPlacement() 
        {
            if (isPlacementActive == false) return;

            Vector2Int effectiveGridSize = currentBuildingInfo.GetEffectiveGridSize(currentConstructionRotationSteps);
            bool centerSnapX = effectiveGridSize.x % 2 == 1;
            bool centerSnapZ = effectiveGridSize.y % 2 == 1;
            Vector3 snappedBuildPoint = SnapToGrid(GetBuildPoint(), centerSnapX, centerSnapZ);
            currentPlacementPosition = ScanBuildingPlacement(snappedBuildPoint, currentBuildingInfo.GridSize, CurrentConstructionRotation);

            if (currentPlacementPosition is not null) 
            {
                constructionPreview.gameObject.SetActive(true);
                constructionPreview.SetPosition(currentPlacementPosition.Value);
            }
            else 
            {
                constructionPreview.gameObject.SetActive(false);
            }
        }

        private void RotatePlacement(bool reverse = false) 
        {
            currentConstructionRotationSteps = (currentConstructionRotationSteps + (reverse ? -1 : 1)) % 4;
            constructionPreview.SetRotation(CurrentConstructionRotation);
        }

        private Vector3 GetBuildPoint() 
        {
            Ray cameraRay = new Ray(cameraTransform.position, cameraTransform.forward);
            bool cameraRaySuccess = Physics.Raycast(cameraRay, out RaycastHit cameraRayHit, maxBuildDistance * 2, buildTargetingCollisionMask);

            if (cameraRaySuccess && Vector3.ProjectOnPlane(cameraTransform.position - cameraRayHit.point, Vector3.up).magnitude < maxBuildDistance) {
                Vector3 buildPoint = new Vector3(cameraRayHit.point.x, 0, cameraRayHit.point.z);
                constructionPreview.SetGridPosition(new Vector4(buildPoint.x, 0, buildPoint.z));
                return buildPoint;
            }

            Vector3 buildPointDistant = Vector3.ProjectOnPlane(cameraTransform.position, Vector3.up) + Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up) * maxBuildDistance;
            constructionPreview.SetGridPosition(new Vector4(buildPointDistant.x, 0, buildPointDistant.z));
            return buildPointDistant;
        }

        private Vector3 SnapToGrid(Vector3 vec, bool centerX, bool centerZ) 
        {
            vec /= GlobalConstants.GRID_CELL_SIZE;
            Vector3 newVec = new Vector3(Mathf.Round(vec.x), 0, Mathf.Round(vec.z));
            newVec *= GlobalConstants.GRID_CELL_SIZE;

            return newVec + new Vector3(centerX ? GlobalConstants.GRID_CELL_SIZE/2.0f : 0, 0, centerZ ? GlobalConstants.GRID_CELL_SIZE/2.0f : 0);
        }

        // Returns the final placement position of the building, or null if no valid position is found
        private Vector3? ScanBuildingPlacement(Vector3 buildPoint, Vector2Int buildingGridSize, Quaternion buildingRotation) 
        {
            Vector3 boxOrigin = new Vector3(buildPoint.x, maxTerrainHeight+1, buildPoint.z);
            Vector3 boxHalfExtents = new Vector3(buildingGridSize.x * GlobalConstants.GRID_CELL_SIZE * 0.5f, 1, buildingGridSize.y * GlobalConstants.GRID_CELL_SIZE * 0.5f);

            if(Physics.BoxCast(boxOrigin, boxHalfExtents, Vector3.down, out RaycastHit result, buildingRotation, maxTerrainHeight*2, buildTargetingCollisionMask))
            {
                Vector3 finalPlacementPosition = new Vector3(buildPoint.x, result.point.y, buildPoint.z);
                return finalPlacementPosition;
            }

            return null;
        }

        private bool IsPlacementPositionValid()
        {
            if (currentPlacementPosition == null) return false;

            Vector3 validCurrentPlacementPosition = new Vector3(currentPlacementPosition.Value.x, currentPlacementPosition.Value.y + 0.5f, currentPlacementPosition.Value.z);
            Ray objectRay = new Ray(validCurrentPlacementPosition, Vector3.down);
            bool objectRaySuccess = Physics.Raycast(objectRay, out RaycastHit objectRayHit, 1f, buildTargetingCollisionMask);
            if (objectRaySuccess && objectRayHit.transform != null && (objectRayHit.transform.gameObject.name == "Floor element" || objectRayHit.transform.gameObject.name == "Terrain")) return true; // TEMP: should be replaced later by a standard floor element prefab

            return false;
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

        public void OnStartPlacementTemp(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            currentBuildingInfo = null;
            currentPlacementPosition = null;
            constructionPreview.gameObject.SetActive(false);
            BuildingSelectionUI.gameObject.SetActive(true);
        }
    }
}
