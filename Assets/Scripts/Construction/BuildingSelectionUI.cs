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
        [SerializeField] private Button menuButton;

        private PlayerCamera playerCamera;

        void Start()
        {   
            LoadBuildings();
            LoadButtons();
            exitButton.onClick.AddListener(() => Close());
            gameObject.SetActive(false);
            playerCamera = Camera.GetComponent<PlayerCamera>();
        }

        void LateUpdate()
        {
            if (gameObject.activeSelf == true) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                playerCamera.ChangeLock(true);
            }
        }
        
        private void LoadBuildings()
        {
            List<BuildingInfo> buildings = 
                AssetDatabase.FindAssets("t:BuildingInfo", BuildingInfoDirectory)
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => AssetDatabase.LoadAssetAtPath<BuildingInfo>(path))
                    .ToList();

            foreach (BuildingInfo building in buildings)
            {
                BuildingInfoFiles[building.name] = building;
                Button button = Instantiate(menuButton);
                button.name = building.name;
                ButtonInfo[building.name] = button;
            }
        }

        private void LoadButtons()
        {
            Transform buttonParent = gameObject.GetComponent<Image>().transform;
            int buttonCount = 0;
            float spacing = 80f;
            float width = (buttonCount - 1) * spacing;
            float startHeight = (buttonParent.transform.position.y - width) / 2;
            foreach (KeyValuePair<string, Button> button in ButtonInfo)
            {
                float buttonHeight = startHeight + buttonCount * spacing;
                button.Value.transform.SetParent(buttonParent);
                button.Value.transform.position = new Vector3(buttonParent.transform.position.x, buttonHeight, buttonParent.transform.position.z);
                button.Value.onClick.AddListener(() => ButtonClick(button.Value));
                button.Value.GetComponentInChildren<TextMeshProUGUI>().text = button.Key;
                buttonCount++;
            }
        }

        private void ButtonClick(Button button)
        {
            Player.GetComponent<PlayerConstructionHandler>().SetBuilding(BuildingInfoFiles.GetValue(button.name));
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerCamera.ChangeLock(false);
            gameObject.SetActive(false);
        }

        private void Close()
        {
            playerCamera.ChangeLock(false);
            gameObject.SetActive(false);
        }
    }
}
