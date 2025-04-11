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
        [SerializeField] private string[] BuildingInfoDirectory = { };
        [SerializeField] private SerializableDictionary<string, BuildingInfo> BuildingInfoFiles;
        [SerializeField] private SerializableDictionary<string, Button> ButtonInfo;
        [SerializeField] private GameObject Player;
        [SerializeField] private Camera Camera;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button buildingButton;
        [SerializeField] private GameObject buttonContainer;

        private PlayerCamera playerCamera;
        private PlayerConstructionHandler playerConstructionHandler;

        void Start()
        {   
            LoadBuildings();
            LoadButtons();
            exitButton.onClick.AddListener(Close);
            gameObject.SetActive(false);
            playerCamera = Camera.GetComponent<PlayerCamera>();
            playerConstructionHandler = Player.GetComponent<PlayerConstructionHandler>();
        }

        void LateUpdate()
        {
            if (gameObject.activeSelf == true) {
                playerCamera.ChangeLock(true);
            }
        }
        
        private void LoadBuildings()
        {
            IEnumerable<BuildingInfo> buildings = 
                AssetDatabase.FindAssets("t:BuildingInfo", BuildingInfoDirectory)
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => AssetDatabase.LoadAssetAtPath<BuildingInfo>(path));

            foreach (BuildingInfo building in buildings)
            {
                BuildingInfoFiles[building.name] = building;
                Button button = Instantiate(buildingButton);
                button.name = building.name;
                ButtonInfo[building.name] = button;
            }
        }

        private void LoadButtons()
        {
            foreach (KeyValuePair<string, Button> button in ButtonInfo)
            {
                button.Value.transform.SetParent(buttonContainer.transform);
                button.Value.onClick.AddListener(() => ButtonClick(button.Value));
                button.Value.GetComponentInChildren<TextMeshProUGUI>().text = button.Key;
            }
        }

        private void ButtonClick(Button button)
        {
            playerConstructionHandler.SetBuilding(BuildingInfoFiles.GetValue(button.name));
            Close();
        }

        private void Close()
        {
            playerCamera.ChangeLock(false);
            gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            exitButton.onClick.RemoveListener(Close);
            foreach (KeyValuePair<string, Button> button in ButtonInfo)
            {
                Destroy(button.Value);
            }
        }
    }
}
