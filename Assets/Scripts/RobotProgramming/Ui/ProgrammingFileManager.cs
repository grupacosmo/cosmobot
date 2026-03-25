using System.Collections.Generic;
using System.IO;
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

        [Header("Canvas UI Prefabs")]
        [SerializeField]
        private GameObject uiFileEntryPrefab;

        private readonly List<ProgrammingUiFileEntry> files = new();

        private const string JsFilesSaveFolder = @"/home/milosz/cosmobot/JsFiles/"; // TEMP
        private readonly string[] fileNames = new[] { "main.js", "robot.js", "mySuperSystem.js", "api.js" };
        
        public Programmable currentRobot;
        
        private void Start()
        {
            string[] jsFiles = Directory.GetFiles(JsFilesSaveFolder, "*.js");
            foreach (string filePath in jsFiles)
            {

                CreateNewFileEntry(Path.GetFileName(filePath));
            }
        }

        public void CreateNewFile(string filename)
        {
            CreateFile(filename);
            CreateNewFileEntry(filename);
        }

        public void RunActiveFile()
        {
            SaveFile();
            currentRobot.code = ReadFile(files[GetOpenFileIndex(true)]);
            currentRobot.RunTask();
        }

        public void StopActiveFile()
        {
            currentRobot.code = "";
            currentRobot.StopTask();
        }

        public void SetActiveFile(ProgrammingUiFileEntry entry)
        {
            currentRobot.activeFile = files[GetOpenFileIndex(true)];
            programmingUI.robotFiles[currentRobot.activeFile] = currentRobot;
            Debug.Log("Set Active File: " + currentRobot.activeFile + "to Robot: " + currentRobot.name);
        }

        public void LoadActiveFile()
        {
            foreach (ProgrammingUiFileEntry entry in files)
            {
                if (entry == currentRobot.activeFile)
                {
                    entry.IsActive = true;
                }
            }
        }
        
        private void CreateNewFileEntry(string filename)
        {
            GameObject uiInstance = Instantiate(uiFileEntryPrefab, transform);
            ProgrammingUiFileEntry entry = uiInstance.GetComponent<ProgrammingUiFileEntry>();
            entry.SetFile(openFileGroup, activeFileGroup, filename, "no stats...");
            entry.OnOpenFile += HandleOpenFile;
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

        public void SaveFile()
        {
            ProgrammingUiFileEntry openFile = files[GetOpenFileIndex()];
            openFile.UpdateStats(programmingUI.Code.Length + " characters");
            SaveTextToFile(openFile.filename);
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

        public int GetFileCount()
        {
            return files.Count;
        }
        
        private static void CreateFile(string filename)
        {
            File.Create(JsFilesSaveFolder + filename).Dispose();
        }

        private static void RemoveFile(string filename)
        {
            File.Delete(JsFilesSaveFolder + filename);
        }

        private static string ReadFile(ProgrammingUiFileEntry entry)
        {
            return File.ReadAllText(JsFilesSaveFolder + entry.filename, Encoding.UTF8);
        }

        private void HandleOpenFile(ProgrammingUiFileEntry entry)
        {
            programmingUI.Code = ReadFile(entry);
        }

        private void SaveTextToFile(string filename)
        {
            File.WriteAllText(JsFilesSaveFolder + filename, programmingUI.Code, Encoding.UTF8);
        }
    }
}
