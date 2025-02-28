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
        [SerializeField] private Canvas canvas;

        void Start()
        {   
            LoadBuildings();
            LoadButtons();
            gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (canvas.enabled == true) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        void OnEnable()
        {
            EnableButtons();       
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
            }
        }


        private void LoadButtons()
        {
            foreach (KeyValuePair<string, Button> button in ButtonInfo)
            {
                button.Value.onClick.AddListener(() => ButtonClick(button.Value));
                button.Value.GetComponentInChildren<TextMeshProUGUI>().text = button.Key;
            }
        }

        public void ButtonClick(Button button)
        {
            Player.GetComponent<PlayerConstructionHandler>().SetBuilding(BuildingInfoFiles.GetValue(button.name));
            DisableButtons();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            gameObject.SetActive(false);
        }

        public void DisableButtons()
        {
            foreach (KeyValuePair<string, Button> button in ButtonInfo)
            {
                button.Value.interactable = false;
            } 
        }

        public void EnableButtons()
        {
            foreach (KeyValuePair<string, Button> button in ButtonInfo)
            {
                button.Value.interactable = true;
            }
        }
    }
}
