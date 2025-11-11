using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cosmobot
{
    // File and File UI management 
    public class ProgrammingFileManager : MonoBehaviour
    {
        [SerializeField]
        private ToggleGroup openFileGroup;
        [SerializeField]
        private ToggleGroup activeFileGroup;

        [Header("Canvas UI Prefabs")]
        [SerializeField]
        private GameObject uiFileEntryPrefab;

        private List<ProgrammingUiFileEntry> files = new();

        public ProgrammingUiFileEntry CreateNewFile(string filename)
        {
            GameObject uiInstance = Instantiate(uiFileEntryPrefab, transform);
            ProgrammingUiFileEntry entry = uiInstance.GetComponent<ProgrammingUiFileEntry>();
            entry.SetFile(openFileGroup, activeFileGroup, filename, "no stats...");
            files.Add(entry);
            return entry;
        }

        public void RemoveOpenFile()
        {
            bool removed = false;
            for (int i = 0; i < files.Count; i++)
            {
                ProgrammingUiFileEntry entry = files[i];
                if (entry.IsOpen)
                {
                    Destroy(entry.gameObject);
                    files.RemoveAt(i);
                    removed = true;
                    break;
                }
            }

            if (removed && files.Count > 0)
            {
                files[0].IsOpen = true;
            }
        }
    }
}
