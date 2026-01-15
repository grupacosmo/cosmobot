using System.Collections.Generic;
using System.IO;
using Acornima.Ast;
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
        [SerializeField]
        private GameObject confirmRemoveBt;
        [SerializeField]
        private ProgrammingUi programmingUI;

        [Header("Canvas UI Prefabs")]
        [SerializeField]
        private GameObject uiFileEntryPrefab;

        private readonly List<ProgrammingUiFileEntry> files = new();

        public ProgrammingUiFileEntry CreateNewFile(string filename)
        {
            GameObject uiInstance = Instantiate(uiFileEntryPrefab, transform);
            ProgrammingUiFileEntry entry = uiInstance.GetComponent<ProgrammingUiFileEntry>();
            entry.SetFile(openFileGroup, activeFileGroup, filename, "no stats...");
            files.Add(entry);
            return entry;
        }

        public void ConfirmRemoveOpenFile()
        {
            if (files.Count == 0) { return; }

            int openFileIndex = GetOpenFileIndex();
            ProgrammingUiFileEntry entry = files[openFileIndex];
            Destroy(entry.gameObject);
            files.RemoveAt(openFileIndex);

            if (files.Count > 0 && files[0])
            {
                files[0].IsOpen = true;
            }

            confirmRemoveBt.SetActive(false);
        }

        public void RemoveOpenFile()
        {
            if (true) // TEMP, later check if file is not empty
            {
                confirmRemoveBt.SetActive(!confirmRemoveBt.activeSelf);
            } 
            else
            {
                ConfirmRemoveOpenFile();
            }
        }

        public void SaveFile()
        {
            ProgrammingUiFileEntry openFile = files[GetOpenFileIndex()];
            openFile.UpdateStats(programmingUI.Code.Length + " characters");
            Debug.Log(programmingUI.Code.Length);
        }

        private int GetOpenFileIndex()
        {
            for (int i = 0; i < files.Count; i++)
            {
                ProgrammingUiFileEntry entry = files[i];
                if (entry.IsOpen)
                {
                    return i;
                }
            }

            return 0;
        }
    }
}
