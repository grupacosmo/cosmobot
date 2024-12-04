using System.Collections.Generic;
using System.Linq;
using Cosmobot.BuildingSystem;
using UnityEngine;

namespace Cosmobot
{
    public class PlayerConstructionHandler : MonoBehaviour, DefaultInputActions.IBuildingPlacementActions, DefaultInputActions.IBuildingInteractionActions
    {

        [SerializeField] Transform cameraTransform;
        [SerializeField] Transform playerTransform;
        [SerializeField] ConstructionPreview constructionPreview;
        [SerializeField] LayerMask buildTargetingCollisionMask;
        [SerializeField] LayerMask constructionSiteCollisionMask;
        [SerializeField] float maxBuildDistance = 20.0f;
        [SerializeField] float maxTerrainHeight = 100.0f;
        [SerializeField] bool isPlacementActive;
        [SerializeField] GameObject constructionSitePrefab;
        [SerializeField] GameObject resourcePrefab; // TEMP
        [SerializeField] GameObject finishedBuildingPrefab;
        [SerializeField] BuildingInfo initialBuilding; // TEMP

        private DefaultInputActions actions;
        private Vector3? currentPlacementPosition;
        private BuildingInfo currentBuildingInfo;
        private Quaternion currentConstructionRotation = Quaternion.identity;

        void Start() {
            InitiatePlacement(initialBuilding); // TEMP
        }

        void LateUpdate()
        {
            if (isPlacementActive) {
                currentPlacementPosition = ScanBuildingPlacement(GetBuildPoint(), currentBuildingInfo.GridDimensions, currentConstructionRotation);
            }
        }  

        // Select a building and start scanning for placement position
        // TODO: hook this up to a state machine or something so it works with the rest of the player mechanics
        public void InitiatePlacement(BuildingInfo buildingInfo) {
            currentBuildingInfo = buildingInfo;
            isPlacementActive = true;
            constructionPreview.SetBuilding(buildingInfo);
            constructionPreview.SetActive(true);
            currentConstructionRotation = Quaternion.identity;
        }
        
        // Place the construction plot
        private void ExecutePlacement() {
            if (!isPlacementActive){
                Debug.LogWarning("Attempted to place building outside of placement mode!");
                return;
            }
            if (currentPlacementPosition is null) {
                Debug.LogWarning("Attempted to place building in impossible position");
                ExitPlacement();
                return;
            }

            GameObject newSite = Instantiate(constructionSitePrefab, (Vector3)currentPlacementPosition, currentConstructionRotation);
            newSite.GetComponent<ConstructionSite>().Initialize(currentBuildingInfo);
            newSite.GetComponent<ConstructionSite>().SetRequiredResources(new SerializableDictionary<string, int>{{"Iron Ore", 0}, {"Stone", 0}}); // TEMP
            
            Vector3 sitePosition = newSite.GetComponent<ConstructionSite>().TempCube.transform.position;
            float distance = 0.5f; // TEMP
            float height = 1.5f; // TEMP

            // Demo objects of what resources will be needed for the building
            GameObject resourceIron = Instantiate(resourcePrefab, new Vector3(
                sitePosition.x + (newSite.transform.eulerAngles.y == 90 ? -distance : newSite.transform.eulerAngles.y == 270 ? distance : 0), sitePosition.y + height, 
                sitePosition.z + (newSite.transform.eulerAngles.y == 0 ? distance : newSite.transform.eulerAngles.y == 180 ? -distance : 0)), newSite.transform.rotation);
            GameObject resourceStone = Instantiate(resourcePrefab, new Vector3(
                sitePosition.x + (newSite.transform.eulerAngles.y == 90 ? distance : newSite.transform.eulerAngles.y == 270 ? -distance : 0), sitePosition.y + height, 
                sitePosition.z + (newSite.transform.eulerAngles.y == 0 ? -distance : newSite.transform.eulerAngles.y == 180 ? distance : 0)), newSite.transform.rotation);
            
            ExitPlacement();
        }

        private void GiveResource() {
            Ray looking = new Ray(cameraTransform.position, cameraTransform.forward);
            bool isLooking = Physics.Raycast(looking, out RaycastHit hit, maxBuildDistance, constructionSiteCollisionMask);

            if (isLooking && hit.collider.gameObject.name == "Cube") { //TEMP
                SerializableDictionary<string, int> targetSiteNeededResources = hit.collider.gameObject.GetComponentInParent<ConstructionSite>().ConstructionSiteResources;
                foreach (KeyValuePair<string, int> resource in targetSiteNeededResources) {
                    Debug.Log($"{resource.Key}: {resource.Value}");
                }
                
                bool isFinished = targetSiteNeededResources.Values.All(value => value == 0);

                if (isFinished) {
                    GameObject finishedSite = Instantiate(finishedBuildingPrefab, hit.collider.gameObject.transform.position, hit.collider.gameObject.transform.rotation);
                    Destroy(hit.collider.gameObject);
                }
            }
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
            vec /= GlobalConstants.GRID_SIZE;
            Vector3 newVec = new Vector3(Mathf.Round(vec.x), 0, Mathf.Round(vec.z));
            newVec *= GlobalConstants.GRID_SIZE;

            return newVec + new Vector3(centerX ? GlobalConstants.GRID_SIZE/2.0f : 0, 0, centerZ ? GlobalConstants.GRID_SIZE/2.0f : 0);
        }

        // Returns the ultimate placement position of the building
        private Vector3? ScanBuildingPlacement(Vector3 buildPoint, Vector2Int buildingGridDimensions, Quaternion buildingRotation) {
            Vector3 boxOrigin = new Vector3(buildPoint.x, maxTerrainHeight+1, buildPoint.z);
            Vector3 boxHalfExtents = new Vector3(buildingGridDimensions.x * GlobalConstants.GRID_SIZE * 0.5f, 1, buildingGridDimensions.y * GlobalConstants.GRID_SIZE * 0.5f);

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

        public void OnGiveResource(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.performed)
                GiveResource();
        }
    }
}
