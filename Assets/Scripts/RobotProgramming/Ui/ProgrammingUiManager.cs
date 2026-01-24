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

        public void Open()
        {
            ChangeUiState(true);
        }

        public void Close()
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
