using System.Collections.Generic;
using Cosmobot.BuildingSystem;
using Cosmobot.ItemSystem;
using UnityEngine;

namespace Cosmobot
{
    public class ConstructionSite : BaseBuilding
    {
        [SerializeField]
        private Transform tempCube;

        [SerializeField]
        private SerializableDictionary<ItemInfo, GameObject> constructionSiteResources = new();

        [SerializeField]
        private BuildingInfo buildingData;

        private readonly float colliderOffset = 0.01f;
        public Transform TempCube => tempCube;
        public BuildingInfo BuildingData => buildingData;
        public SerializableDictionary<ItemInfo, GameObject> ConstructionSiteResources => constructionSiteResources;

        private void Start()
        {
            BoxCollider prefabCollider = buildingData.Prefab.GetComponentInChildren<BoxCollider>();
            prefabCollider.size = new Vector3(prefabCollider.size.x - colliderOffset, prefabCollider.size.y,
                prefabCollider.size.z - colliderOffset);
            tempCube.localScale = new Vector3(buildingData.Prefab.transform.localScale.x, 0.2f,
                buildingData.Prefab.transform.localScale.z);
            SetRequiredResources(buildingData.ResourceRequirements);
        }

        public void SetRequiredResources(IReadOnlyDictionary<ItemInfo, int> requiredResources)
        {
            int previewCount = 0;
            GameObject[] previewObjects = new GameObject[requiredResources.Count];
            foreach (KeyValuePair<ItemInfo, int> pair in requiredResources)
            {
                GameObject preview = GetComponent<ConstructionSiteResourcePreview>()
                    .CreateResourcePreview((SerializableDictionary<ItemInfo, int>)requiredResources, previewCount);
                preview.transform.SetParent(transform);
                preview.GetComponent<ResourcePreviewController>().SetRequirement(pair.Value);
                previewObjects.SetValue(preview, previewCount);
                constructionSiteResources[pair.Key] = preview;
                previewCount++;
            }

            GetComponent<ConstructionSiteResourcePreview>().SetPreviewObjects(previewObjects);
        }

        public void DecreaseResourceRequirement(ItemInfo resourceToDecrease, int resourceAmount = 1)
        {
            GameObject previewToDecrease = constructionSiteResources[resourceToDecrease];
            ResourcePreviewController resourcePreviewController =
                previewToDecrease.GetComponent<ResourcePreviewController>();
            if (resourcePreviewController.ResourceRequirement > 0)
            {
                resourcePreviewController.DecreaseRequirement(resourceAmount);
                IsReadyToBuild(buildingData.Prefab);
            }
        }

        public void Initialize(BuildingInfo newbuildingData)
        {
            buildingData = newbuildingData;
            tempCube.localScale = new Vector3(buildingData.GridSize.x, 0.2f, buildingData.GridSize.y);
            BoxCollider buildingCollider = tempCube.GetComponent<BoxCollider>();
            buildingCollider.size = new Vector3(buildingCollider.size.x - colliderOffset, buildingCollider.size.y,
                buildingCollider.size.z - colliderOffset);
        }

        public void FinishConstruction()
        {
            Instantiate(buildingData.Prefab, transform.position, transform.rotation); // TODO: set instantiation parent
            Despawn();
        }

        public void IsReadyToBuild(GameObject targetBuilding)
        {
            foreach (GameObject resource in GetComponent<ConstructionSiteResourcePreview>().PreviewObjects)
                if (resource.GetComponent<ResourcePreviewController>().ResourceRequirement > 0)
                    return;

            Build(targetBuilding);
        }

        public void Build(GameObject targetBuilding)
        {
            Instantiate(targetBuilding, transform.position, transform.rotation);
            Despawn();
        }
    }
}
