using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

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

        private readonly string jsFilesSaveFolder = @"/home/milosz/cosmobot/JsFiles/"; // TEMP

        public ProgrammingUiFileEntry CreateNewFile(string filename)
        {
            CreateFile(filename);
            return CreateNewFileEntry(filename);
        }

        private ProgrammingUiFileEntry CreateNewFileEntry(string filename)
        {
            GameObject uiInstance = Instantiate(uiFileEntryPrefab, transform);
            ProgrammingUiFileEntry entry = uiInstance.GetComponent<ProgrammingUiFileEntry>();
            entry.SetFile(openFileGroup, activeFileGroup, filename, "no stats...");
            entry.OnOpenFile += HandleOpenFile;
            files.Add(entry);
            return entry;
        }

        public void OnEnable()
        {
            string[] jsFiles = Directory.GetFiles(jsFilesSaveFolder, "*.js");
            foreach(string filePath in jsFiles)
            {
                LoadFileFromDisk(Path.GetFileName(filePath));
            }
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
            SaveTextToFile(openFile.filename);
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

        private void CreateFile(string filename)
        {
            File.Create(jsFilesSaveFolder + filename).Dispose();
        }

        public void HandleOpenFile(ProgrammingUiFileEntry entry)
        {
            LoadFileOnOpen(entry);
        }

        private void LoadFileOnOpen(ProgrammingUiFileEntry entry)
        {
            programmingUI.Code = File.ReadAllText(jsFilesSaveFolder + entry.filename, Encoding.UTF8);
        }

        private void LoadFileFromDisk(string filename)
        {
            CreateNewFileEntry(filename);
        }

        private void SaveTextToFile(string filename)
        {
            File.WriteAllText(jsFilesSaveFolder + filename, programmingUI.Code, Encoding.UTF8);
        }
    }
}
