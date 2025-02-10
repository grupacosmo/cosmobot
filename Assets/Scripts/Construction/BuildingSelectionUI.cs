using System.Collections.Generic;
using System.Linq;
using Cosmobot.BuildingSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cosmobot
{
    public class BuildingSelectionUI : MonoBehaviour
    {
        [SerializeField] private string[] BuildingInfoDirectory = { };
        [SerializeField] private SerializableDictionary<string, BuildingInfo> BuildingInfoFiles;
        [SerializeField] private Button Button1x1;
        [SerializeField] private Button Button1x2;
        [SerializeField] private Button Button2x2;
        [SerializeField] private Button Button3x3;
        [SerializeField] private GameObject Player;
        [SerializeField] private Canvas canvas;

        void Start()
        {   
            LoadBuildings();

            Button1x1.onClick.AddListener(() => ButtonClick(Button1x1));
            Button1x2.onClick.AddListener(() => ButtonClick(Button1x2));
            Button2x2.onClick.AddListener(() => ButtonClick(Button2x2));
            Button3x3.onClick.AddListener(() => ButtonClick(Button3x3));
            canvas.enabled = false;
        }

        void LateUpdate()
        {
            if (canvas.enabled == true){
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
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
            }
        }

        public void ButtonClick(Button button)
        {
            Debug.Log(BuildingInfoFiles.GetValue(button.name));
            Player.GetComponent<PlayerConstructionHandler>().SetBuilding(BuildingInfoFiles.GetValue(button.name));
            canvas.enabled = false;
        }
    }
}
