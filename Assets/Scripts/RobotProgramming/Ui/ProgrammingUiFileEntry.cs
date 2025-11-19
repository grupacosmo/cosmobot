using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cosmobot
{
    // ui only functionality 
    public class ProgrammingUiFileEntry : MonoBehaviour
    {
        public delegate void FileEntryEvent(ProgrammingUiFileEntry entry);

        [SerializeField]
        private Toggle openFileToggle;
        [SerializeField]
        private Toggle activeFileToggle;
        [SerializeField]
        private TMP_Text fileNameText;
        [SerializeField]
        private TMP_Text fileStatsText;

        public event FileEntryEvent OnOpenFile;
        public event FileEntryEvent OnActivateFile;

        public bool IsOpen { get => openFileToggle.isOn; set => openFileToggle.isOn = value; }
        public bool IsActive { get => activeFileToggle.isOn; set => activeFileToggle.isOn = value; }

        public void SetFile(ToggleGroup openFileGroup, ToggleGroup activeFileGroup, string filename, string stats)
        {
            fileNameText.text = filename;
            fileStatsText.text = stats;

            openFileToggle.isOn = false;
            activeFileToggle.isOn = false;
            openFileToggle.group = openFileGroup;
            activeFileToggle.group = activeFileGroup;
        }

        private void OnEnable()
        {
            openFileToggle.onValueChanged.AddListener(OnOpenToggle);
            activeFileToggle.onValueChanged.AddListener(OnActiveToggle);
        }

        private void OnDisable()
        {
            activeFileToggle.onValueChanged.RemoveListener(OnActiveToggle);
            openFileToggle.onValueChanged.RemoveListener(OnOpenToggle);
        }

        private void OnOpenToggle(bool value)
        {
            if (value)
                OnOpenFile?.Invoke(this);
        }

        private void OnActiveToggle(bool value)
        {
            if (value)
                OnActivateFile?.Invoke(this);
        }
    }
}
