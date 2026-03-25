using UnityEngine;

namespace Cosmobot
{
    public class ProgrammingUiInteractionHandler : MonoBehaviour
    {

        [SerializeField]
        private ProgrammingFileManager fileManager;
        [SerializeField]
        private ProgrammingUiLogManager logManager;
        [SerializeField]
        private ProgrammingUiManager uiManager;

        public void CreateNewFile()
        {
            fileManager.CreateNewFile("file" + fileManager.GetFileCount() + ".js"); // TEMP
        }

        public void ConfirmRemoveOpenFile()
        {
            fileManager.ConfirmRemoveOpenFile();
        }

        public void RemoveOpenFile()
        {
            if (IsUiInteractionInvalid("Found no files to remove")) { return; }
            fileManager.RemoveOpenFile();
        }

        public void SaveFile()
        {
            if (IsUiInteractionInvalid("Found no files to save")) { return; }
            fileManager.SaveFile();
        }

        public void RunActiveFile()
        {
            if (IsUiInteractionInvalid("Found no files to execute")) { return; }
            fileManager.RunActiveFile();
        }

        public void StopActiveFile()
        {
            if (IsUiInteractionInvalid("Found no files to stop")) { return; }
            fileManager.StopActiveFile();
        }

        private bool IsUiInteractionInvalid(string logMessage)
        {
            if (fileManager.GetFileCount() != 0) { return false; }
            
            logManager.CreateLog(LogLevel.Warn, logMessage);
            return true;
        }
    }
}
