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
        private PlayerController playerController;

        public void Start()
        {
            ChangeUiState(false);
        }

        public void Open(Programmable robot)
        {
            programmingUi.activeRobot = robot;
            ChangeUiState(true);
        }

        public void Close()
        {
            programmingUi.activeRobot = null;
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
