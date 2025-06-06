using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cosmobot.RobotProgramming
{
    public class ProgrammingUI : MonoBehaviour
    {
        private static string[] fileNames = new string[]
        { "Super", "Script", "Src", "Controller", "Main", "Robot", "Program", "Tree", "Visitor", "Util", "Base", "Extension", "Handler", "Manager", "Service", "Factory", "Adapter", "Builder", "Strategy", "Observer", "Command", "Prototype", "Composite", "Decorator", "Facade", "Flyweight", "Proxy", "State", "Template", "Chain", "Mediator", "Iterator", "Visitor", "Bridge", "Memento", "Interpreter", "Singleton", "AbstractFactory", "Data", "Model", "View", "Controller", "Repository", "Service" };

        private static string RandomFileNamePart()
        {
            return fileNames[UnityEngine.Random.Range(0, fileNames.Length)];
        }

        private static string RandomFileName()
        {
            return RandomFileNamePart() + RandomFileNamePart() + ".js";
        }

        private TextField codeField;
        private TextField consoleField;
        private ListView fileList;

        private Label currentFileStatus;
        
        private Button saveButton;
        private Button startButton;
        private Button stopButton;
        
        
        private bool isRunning;
        private List<FileEntry> files = new List<FileEntry>();
        private int mainFileIndex;
        private FileEntry mainFile => files[mainFileIndex];
        private int previosSelectedFileIndex = 0;
        
        private Queue<string> consoleLines  = new Queue<string>();
        private int maxConsoleLines = 200;

        
        private FileEntry SelectedFileEntry => files[fileList.selectedIndex];

        private struct FileEntry
        {
            public int cursorIndex;
            public string fileName;
            public bool isMain;
            public string content;

            public static FileEntry New()
            { 
                return new FileEntry
                {
                    cursorIndex = 0,
                    fileName = RandomFileName(),
                    isMain = false,
                    content = ""
                };
            }
        }


        private class FileListEntry
        {
            int entryIndex;
            Label nameLabel;
            RadioButton isMainRadio;

            public int Index => entryIndex;

            public void SetVisualElement(VisualElement root, EventCallback<ClickEvent> onMainSelectEvent)
            {
                nameLabel = root.Q<Label>("FileName");
                nameLabel.userData = root;
                isMainRadio = root.Q<RadioButton>("IsMainRBt");
                isMainRadio.RegisterCallback(onMainSelectEvent);
                isMainRadio.userData = root;
            }

            public void Bind(int index, FileEntry entry)
            {
                entryIndex = index;
                nameLabel.text = entry.fileName;
                isMainRadio.value = entry.isMain;
            }
        }

        private void OnEnable()
        {
            UIDocument document = GetComponent <UIDocument>();
            
            codeField = document.rootVisualElement.Q<TextField>("CodeInput");
            consoleField = document.rootVisualElement.Q<TextField>("ConsoleOutput");
            fileList = document.rootVisualElement.Q<ListView>("FileList");
            saveButton = document.rootVisualElement.Q<Button>("MenuSaveBt");
            startButton = document.rootVisualElement.Q<Button>("MenuRunBt");
            stopButton = document.rootVisualElement.Q<Button>("MenuStopBt");
            currentFileStatus = document.rootVisualElement.Q<Label>("CodeStats");
            
            saveButton.RegisterCallback<ClickEvent>(OnButtonSave);
            startButton.RegisterCallback<ClickEvent>(OnButtonRun);
            stopButton.RegisterCallback<ClickEvent>(OnButtonStop);

            codeField.RegisterCallback<ChangeEvent<string>>(OnCodeChanged);

            FileEntry defaultEntry = FileEntry.New();
            defaultEntry.isMain = true;
            files.Add(defaultEntry);
            mainFileIndex = 0;


            fileList.makeItem = () =>
            {
                TemplateContainer newEntry = fileList.itemTemplate.Instantiate();
                FileListEntry fileListEntry = new FileListEntry();
                fileListEntry.SetVisualElement(newEntry, OnSelectMainScript);
                newEntry.userData = fileListEntry;
                return newEntry;
            };
            
            fileList.bindItem = (item, index) =>
            {
                (item.userData as FileListEntry)?.Bind(index, files[index]);
            };

            fileList.onAdd = (baseListView) =>
            {
                FileEntry newFile = FileEntry.New();
                files.Add(newFile);
                fileList.RefreshItems();
            };

            fileList.onRemove = (baseListView) =>
            {
                files.RemoveAt(fileList.selectedIndex);
                fileList.RefreshItems();
            };

            fileList.selectedIndicesChanged += OnSelectedFileChanged;
            
            fileList.itemsSource = files;
            fileList.selectedIndex = 0;
        }
        
        private void OnCodeChanged(ChangeEvent<string> changeEvent)
        {
            if (!ValidateSelectedFileIndex()) return;
            
            // Debug.Log("Code changed: " + changeEvent.newValue);
            // Debug.Log(highlightedCode);
            FileEntry entry = files[fileList.selectedIndex];
            entry.content = changeEvent.newValue;
            files[fileList.selectedIndex] = entry;
            
            currentFileStatus.text = FileStatus(entry);
        }
        
        private void OnSelectedFileChanged(IEnumerable<int> selectedIndices)
        {
            FileEntry prevFile = files[previosSelectedFileIndex];
            prevFile.cursorIndex = codeField.cursorIndex;
            files[previosSelectedFileIndex] = prevFile;
            
            int selected = selectedIndices.First();
            Debug.Log("Selected file index changed to: " + selected);
            FileEntry selectedFile = files[selected];
            codeField.SetValueWithoutNotify(selectedFile.content);
            codeField.cursorIndex = selectedFile.cursorIndex;
            currentFileStatus.text = FileStatus(selectedFile);
        }
        
        private void OnSelectMainScript(ClickEvent clickEvent)
        {
            Debug.Log("Main script selected");
            RadioButton target = (clickEvent.target as RadioButton)!;
            VisualElement fileEntryVe = target.userData as VisualElement;
            FileListEntry entry = (fileEntryVe?.userData as FileListEntry)!;
            FileEntry mainFile = files[mainFileIndex];
            mainFile.isMain = false;
            files[mainFileIndex] = mainFile;
            
            FileEntry fileEntry = files[entry.Index]; 
            fileEntry.isMain = true;
            files[entry.Index] = fileEntry;

            mainFileIndex = entry.Index;
            
            fileList.RefreshItems();
        }

        private void OnButtonSave(ClickEvent _)
        {
            if (!ValidateSelectedFileIndex()) return;
            Debug.Log("Save button clicked");
            AddConsoleLine("Saving file: " + SelectedFileEntry.fileName);
        }
        
        private void OnButtonRun(ClickEvent _)
        {
            isRunning = true;
            UpdateRunningState();
        }

        private void OnButtonStop(ClickEvent _)
        {
            isRunning = false;
            UpdateRunningState();
        }
        
        private void UpdateRunningState()
        {
            if (isRunning)
            {
                startButton.SetEnabled(false);
                stopButton.SetEnabled(true);
                currentFileStatus.text = "Running";
                Debug.Log("Start");
                AddConsoleLine("Running file: " + mainFile.fileName);
            }
            else
            {
                startButton.SetEnabled(true);
                stopButton.SetEnabled(false);
                currentFileStatus.text = "Stopped";
                Debug.Log("Stop");
                AddConsoleLine("Stopped script");
            }
        }

        private void PrintToConsole(string text)
        {
            if(text.Contains('\n'))
            {
                string[] lines = text.Split(new[] { '\n' }, StringSplitOptions.None);
                foreach (string line in lines)
                {
                    AddConsoleLine(line);
                }
            }
            else
            {
                AddConsoleLine(text);
            }
        }

        private void AddConsoleLine()
        {
            AddConsoleLine(string.Empty);
        }
        private void AddConsoleLine(string line)
        {
            consoleLines.Enqueue(line);
            if (consoleLines.Count > maxConsoleLines)
            {
                consoleLines.Dequeue();
            }
            consoleField.value = string.Join("\n", consoleLines);
        }

        private string FileStatus(FileEntry entry)
        {
            return $"{entry.fileName} | {entry.content.Length} characters";
        }

        private bool ValidateSelectedFileIndex()
        {
            if (fileList.selectedIndex < 0 || fileList.selectedIndex >= files.Count)
            {
                Debug.LogError("Selected file index is out of range.");
                return false;
            }

            return true;
        }
    }
}