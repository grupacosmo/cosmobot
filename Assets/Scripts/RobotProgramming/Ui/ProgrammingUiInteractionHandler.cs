using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

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
            fileManager.RemoveOpenFile();
        }

        public void SaveFile()
        {
            fileManager.SaveFile();
        }

        public void RunActiveFile()
        {
            fileManager.RunActiveFile();
        }

        public void StopActiveFile()
        {
            fileManager.StopActiveFile();
        }
    }
}
