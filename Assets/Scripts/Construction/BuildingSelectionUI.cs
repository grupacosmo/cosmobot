using System.Collections.Generic;
using System.Linq;
using Cosmobot.BuildingSystem;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cosmobot
{
    public class BuildingSelectionUI : MonoBehaviour
    {
        [SerializeField]
        private string[] buildingInfoDirectory = { };

        [SerializeField]
        private SerializableDictionary<string, BuildingInfo> buildingInfoFiles;

        [SerializeField]
        private SerializableDictionary<string, Button> buttonInfo;

        [SerializeField]
        private GameObject player;

        [SerializeField]
        private Camera mainCamera;

        [SerializeField]
        private Button exitButton;

        [SerializeField]
        private Button buildingButton;

        [SerializeField]
        private GameObject buttonContainer;

        private PlayerCamera playerCamera;
        private PlayerConstructionHandler playerConstructionHandler;

        private void Start()
        {
            LoadBuildings();
            LoadButtons();
            exitButton.onClick.AddListener(Close);
            gameObject.SetActive(false);
            playerCamera = mainCamera.GetComponent<PlayerCamera>();
            playerConstructionHandler = player.GetComponent<PlayerConstructionHandler>();
        }

        private void LateUpdate()
        {
            if (gameObject.activeSelf)
            {
                playerCamera.ChangeLock(true);
            }
        }

        private void OnDestroy()
        {
            exitButton.onClick.RemoveListener(Close);
            foreach (KeyValuePair<string, Button> button in buttonInfo)
            {
                Destroy(button.Value);
            }
        }

        private void LoadBuildings()
        {
            IEnumerable<BuildingInfo> buildings =
                AssetDatabase.FindAssets("t:BuildingInfo", buildingInfoDirectory)
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => AssetDatabase.LoadAssetAtPath<BuildingInfo>(path));

            foreach (BuildingInfo building in buildings)
            {
                buildingInfoFiles[building.name] = building;
                Button button = Instantiate(buildingButton);
                button.name = building.name;
                buttonInfo[building.name] = button;
            }
        }

        private void LoadButtons()
        {
            foreach (KeyValuePair<string, Button> button in buttonInfo)
            {
                button.Value.transform.SetParent(buttonContainer.transform);
                button.Value.onClick.AddListener(() => ButtonClick(button.Value));
                button.Value.GetComponentInChildren<TextMeshProUGUI>().text = button.Key;
            }
        }

        private void ButtonClick(Button button)
        {
            playerConstructionHandler.SetBuilding(buildingInfoFiles.GetValue(button.name));
            Close();
        }

        private void Close()
        {
            playerCamera.ChangeLock(false);
            gameObject.SetActive(false);
        }
    }
}
