using UnityEngine;

namespace Cosmobot
{
    public class ProgrammingUiManager : MonoBehaviour
    {
        [SerializeField]
        private PlayerCamera playerCamera;
        [SerializeField]
        private ProgrammingUi programmingUi;
        [SerializeField]
        private ProgrammingFileManager fileManager;
        [SerializeField]
        private PlayerController playerController;

        public void Start()
        {
            ChangeUiState(false);
        }

        public void OpenUI(Programmable robot)
        {
            fileManager.currentRobot = robot;
            ChangeUiState(true);
        }

        public void CloseUI()
        {
            ChangeUiState(false);
        }

        private void ChangeUiState(bool isActive)
        {
            programmingUi.gameObject.SetActive(isActive);
            playerCamera.ChangeLock(isActive);
            playerController.LockMovement(isActive);
        }
    }
}
