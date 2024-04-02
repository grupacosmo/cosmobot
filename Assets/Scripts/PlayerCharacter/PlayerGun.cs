using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmobot
{
    public class PlayerGun : MonoBehaviour, DefaultInputActions.IPlayerGunActions
    {

        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private Transform cameraTransform;

        public void Update()
        {
            GetAimingTarget();
            UpdateModel();
        }

        private void GetAimingTarget()
        {

        }

        private void UpdateModel()
        {
            transform.rotation = playerCamera.transform.rotation;
        }

        public void OnShoot(InputAction.CallbackContext context)
        {

        }

    }
}
