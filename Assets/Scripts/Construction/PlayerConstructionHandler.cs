using System.Collections.Generic;
using System.Linq;
using Cosmobot.BuildingSystem;
using Cosmobot.ItemSystem;
using UnityEngine;

namespace Cosmobot
{
    public class PlayerConstructionHandler : MonoBehaviour, DefaultInputActions.IBuildingPlacementActions, DefaultInputActions.IBuildingInteractionActions
    {
        [SerializeField] Transform cameraTransform;
        [SerializeField] ConstructionPreview constructionPreview;
        [SerializeField] LayerMask buildTargetingCollisionMask;
        [SerializeField] LayerMask constructionSiteCollisionMask;
        [SerializeField] float maxBuildDistance = 20.0f;
        [SerializeField] float maxTerrainHeight = 100.0f;
        [SerializeField] GameObject constructionSitePrefab;
        [SerializeField] GameObject finishedBuildingPrefab;
        [SerializeField] BuildingInfo initialBuilding; // TEMP

        private DefaultInputActions actions;
        private Vector3? currentPlacementPosition;
        private BuildingInfo currentBuildingInfo;
        private int currentConstructionRotationSteps = 0; // multiply by 90deg to get actual rotation
        private Quaternion CurrentConstructionRotation => Quaternion.Euler(0, currentConstructionRotationSteps * 90.0f, 0);
        private bool isPlacementActive = false;

        void LateUpdate()
        {
            ProcessPlacement();
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

            GameObject newSite = Instantiate(constructionSitePrefab, (Vector3)currentPlacementPosition, CurrentConstructionRotation);
            newSite.GetComponent<ConstructionSite>().Initialize(currentBuildingInfo);
            // should be set based on building type
            var requirements = new SerializableDictionary<ItemInfo, int>{
                {ItemManager.Instance.Items.ElementAt(2), 5}, 
                /*{ItemManager.Instance.Items.ElementAt(7), 3},
                {ItemManager.Instance.Items.ElementAt(14), 8}*/
                };
            newSite.GetComponent<ConstructionSite>().SetRequiredResources(requirements);

            ExitPlacement();
        }

        private void GiveResource() {
            Ray looking = new Ray(cameraTransform.position, cameraTransform.forward);
            bool isLooking = Physics.Raycast(looking, out RaycastHit hit, maxBuildDistance, constructionSiteCollisionMask);
            
            if (isLooking && hit.collider.gameObject.name == "Resource(Clone)") { //TEMP
                hit.collider.gameObject.GetComponentInParent<ConstructionSite>().DecreaseResourceRequirement(ItemManager.Instance.Items.ElementAt(2));
                hit.collider.gameObject.GetComponentInParent<ConstructionSite>().IsReadyToBuild(finishedBuildingPrefab);
            }
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
                return new Vector3(cameraRayHit.point.x, 0, cameraRayHit.point.z);
            }

            return Vector3.ProjectOnPlane(cameraTransform.position, Vector3.up) + Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up) * maxBuildDistance;
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

        private void OnEnable()
        {
            if (actions is null)
            {
                actions = new DefaultInputActions();
                actions.BuildingPlacement.SetCallbacks(this);
                actions.BuildingInteraction.SetCallbacks(this);
            }

            actions.BuildingPlacement.Enable();
            actions.BuildingInteraction.Enable();
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
            InitiatePlacement(initialBuilding);
        }

        public void OnGiveResource(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.performed)
                GiveResource();
        }
    }
}
