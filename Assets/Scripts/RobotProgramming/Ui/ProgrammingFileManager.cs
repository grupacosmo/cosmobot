using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        [SerializeField]
        private GameObject dirtyIndicator;

        [Header("Canvas UI Prefabs")]
        [SerializeField]
        private GameObject uiFileEntryPrefab;

        private readonly List<ProgrammingUiFileEntry> files = new();
        private static string jsFilesSaveFolder;
        private ProgrammingUiFileEntry currentEntry;

        public Programmable currentRobot;

        private void Awake()
        {
            jsFilesSaveFolder = Path.Combine(Application.persistentDataPath, "JsFiles");
            if (!Directory.Exists(jsFilesSaveFolder))
            {
                Directory.CreateDirectory(jsFilesSaveFolder);
            }
        }

        private void Start()
        {
            string[] jsFiles = Directory.GetFiles(jsFilesSaveFolder, "*.js");
            foreach (string filePath in jsFiles)
            {
                CreateNewFileEntry(Path.GetFileName(filePath));
            }

            if (files.Count == 0) { return; }

            HandleOpenFile(files[0]);
        }

        private void OnEnable()
        {
            programmingUI.OnCodeChanged += HandleCodeChanged;

            ProgrammingUiFileEntry activeEntry = programmingUI.robotActiveFiles.GetValueOrDefault(currentRobot);
            if (activeEntry is null) { return; }

            activeEntry.IsActive = true;

            HandleOpenFile(activeEntry);
        }

        private void OnDisable()
        {
            programmingUI.OnCodeChanged -= HandleCodeChanged;
        }

        public void CreateNewFile(string filename)
        {
            CreateFile(filename);
            CreateNewFileEntry(filename);
        }

        public void RunActiveFile()
        {
            SaveAllFiles();

            ProgrammingUiFileEntry entry = programmingUI.robotActiveFiles[currentRobot];
            entry.IsOpen = true;
            currentRobot.code = ReadFile(entry);
            currentRobot.RunTask();
        }

        public void StopActiveFile()
        {
            currentRobot.StopTask();
        }

        private void CreateNewFileEntry(string filename)
        {
            GameObject uiInstance = Instantiate(uiFileEntryPrefab, transform);
            ProgrammingUiFileEntry entry = uiInstance.GetComponent<ProgrammingUiFileEntry>();
            entry.SetFile(openFileGroup, activeFileGroup, filename, "no stats...");
            entry.unsavedCode = null;
            entry.OnOpenFile += HandleOpenFile;
            entry.OnActivateFile += HandleActiveFile;
            files.Add(entry);
        }

        public void RemoveOpenFile()
        {
            ProgrammingUiFileEntry entry = files[GetOpenFileIndex()];

            if (string.IsNullOrWhiteSpace(ReadFile(entry)))
            {
                ConfirmRemoveOpenFile();
            }
            else
            {
                confirmRemoveBt.SetActive(!confirmRemoveBt.activeSelf);
            }
        }

        public void ConfirmRemoveOpenFile()
        {
            if (files.Count == 0) { return; }

            int openFileIndex = GetOpenFileIndex();
            ProgrammingUiFileEntry entry = files[openFileIndex];
            Destroy(entry.gameObject);
            RemoveFile(entry.filename);
            files.RemoveAt(openFileIndex);

            if (files.Count > 0 && files[0])
            {
                files[0].IsOpen = true;
            }

            if (files.Count == 0)
            {
                programmingUI.Code = "";
            }

            confirmRemoveBt.SetActive(false);
        }

        public void SaveAllFiles()
        {
            foreach (ProgrammingUiFileEntry entry in files)
            {
                string contentToSave = entry.unsavedCode ?? ReadFile(entry);
                File.WriteAllText(Path.Combine(jsFilesSaveFolder, entry.filename), contentToSave, Encoding.UTF8);

                entry.unsavedCode = null;

                UpdateDirtyIndicator();
            }
        }

        private int GetActiveUsageCount(ProgrammingUiFileEntry entry)
        {
            return programmingUI.robotActiveFiles.Values.Count(e => e == entry);
        }

        private int GetOpenFileIndex(bool getActiveIndex = false)
        {
            for (int i = 0; i < files.Count; i++)
            {
                ProgrammingUiFileEntry entry = files[i];
                if (getActiveIndex && entry.IsActive)
                {
                    return i;
                }
                if (entry.IsOpen)
                {
                    return i;
                }
            }

            return 0;
        }

        private bool HasAnyUnsavedChanges()
        {
            return files.Any(f => !string.IsNullOrWhiteSpace(f.unsavedCode));
        }

        private void UpdateDirtyIndicator()
        {
            bool hasUnsaved = HasAnyUnsavedChanges();

            dirtyIndicator.SetActive(!hasUnsaved);
        }

        private void UpdateAllFileStats()
        {
            foreach (ProgrammingUiFileEntry entry in files)
            {
                int count = GetActiveUsageCount(entry);
                entry.UpdateStats($"{count} robot" + (count == 1 ? "" : "s"));
            }
        }

        private void HandleCodeChanged()
        {
            if (currentEntry == null) return;

            string currentCode = programmingUI.Code ?? "";
            string fileCode = ReadFile(currentEntry);

            currentEntry.unsavedCode = currentCode != fileCode
                ? currentCode
                : null;

            UpdateDirtyIndicator();
        }

        public int GetFileCount()
        {
            return files.Count;
        }

        private static void CreateFile(string filename)
        {
            File.Create(Path.Combine(jsFilesSaveFolder, filename)).Dispose();
        }

        private static void RemoveFile(string filename)
        {
            File.Delete(Path.Combine(jsFilesSaveFolder, filename));
        }

        private static string ReadFile(ProgrammingUiFileEntry entry)
        {
            return File.ReadAllText(Path.Combine(jsFilesSaveFolder, entry.filename), Encoding.UTF8);
        }

        private void HandleOpenFile(ProgrammingUiFileEntry entry)
        {
            if (currentEntry != null)
            {
                string currentCode = programmingUI.Code ?? "";
                string fileCode = ReadFile(currentEntry);

                currentEntry.unsavedCode = currentCode != fileCode
                    ? currentCode
                    : null;

                UpdateDirtyIndicator();
            }

            UpdateDirtyIndicator();
            currentEntry = entry;
            entry.IsOpen = true;

            if (string.IsNullOrWhiteSpace(entry.unsavedCode))
                entry.unsavedCode = null;

            programmingUI.Code = entry.unsavedCode ?? ReadFile(entry);
        }

        private void HandleActiveFile(ProgrammingUiFileEntry entry)
        {
            programmingUI.robotActiveFiles[currentRobot] = entry;
            UpdateAllFileStats();
        }
    }
}
