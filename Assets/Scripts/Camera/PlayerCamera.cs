using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmobot
{
    [RequireComponent(typeof(Camera))]
    public class PlayerCamera : MonoBehaviour, DefaultInputActions.IPlayerCameraActions
    {
        public bool IsFirstPerson { get; private set; } = true;
        public bool IsZoomed { get; private set; }

        [SerializeField] private Vector2 sensitivity;
        [SerializeField] private float zoomSensitivityMultiplier;
        [SerializeField] private Vector3 thirdPersonCameraOffset;
        [SerializeField] private float rotationClampTop;
        [SerializeField] private float rotationClampBottom;
        [SerializeField] private float wallCollisionOffset;
        [SerializeField] private float zoomMagnification;
        [SerializeField] private float zoomSpeed;
        [SerializeField] private LayerMask cameraCollisionLayer;

        [SerializeField] private Transform cameraOrigin;
        [SerializeField] private GameObject playerModel;

        private new Camera camera;
        private DefaultInputActions actions;
        private float xRotation;
        private float yRotation;
        private float xInput;
        private float yInput;
        private Vector3 cameraOffset = Vector3.zero;
        private float defaultFov;
        private float zoomFov;
        private bool isLocked = false;

        public void ChangeLock(bool locked)
        {
            isLocked = locked;
            Cursor.visible = locked;
            if (isLocked == false)
            {
                Cursor.lockState = CursorLockMode.Locked;
            } else {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private void OnEnable()
        {
            if (actions is null)
            {
                actions = new DefaultInputActions();
                actions.PlayerCamera.SetCallbacks(this);
            }

            actions.PlayerCamera.Enable();
        }

        private void OnDisable()
        {
            actions.PlayerCamera.Disable();
        }

        public void OnCamera(InputAction.CallbackContext context)
        {
            Vector2 inputVector = context.ReadValue<Vector2>();
            xInput = inputVector.x;
            yInput = inputVector.y;
        }

        public void OnSwitchView(InputAction.CallbackContext context)
        {
            SwitchCameraView();
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            if (context.performed) IsZoomed = true;
            if (context.canceled) IsZoomed = false;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            transform.position = cameraOrigin.position;

            camera = GetComponent<Camera>();
            defaultFov = camera.fieldOfView;
            zoomFov = defaultFov / zoomMagnification;

            SwitchCameraView();
        }

        private void Update()
        {
            if (isLocked == false)
            {
                HandleInput();
                UpdateFov();
            }
        }

        private void LateUpdate() => UpdateTransform();


        private void HandleInput()
        {
            Vector2 rotationDelta = new Vector2(yInput * sensitivity.y, xInput * sensitivity.x) * Time.deltaTime;
            if (IsZoomed) rotationDelta *= zoomSensitivityMultiplier;
            yRotation += rotationDelta.y;
            xRotation -= rotationDelta.x;
            xRotation = Mathf.Clamp(xRotation, rotationClampBottom, rotationClampTop);
        }

        private void UpdateTransform()
        {
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

            var cameraRotationCenterPosition = cameraOrigin.position;
            var rayDirection = transform.rotation * cameraOffset;
            var cameraDistance = cameraOffset.magnitude;
            var mask = cameraCollisionLayer;
            transform.position =
                Physics.SphereCast(cameraRotationCenterPosition, wallCollisionOffset, 
                        rayDirection, out var hit, cameraDistance + wallCollisionOffset, mask)
                    ? hit.point + hit.normal * wallCollisionOffset
                    : rayDirection + cameraRotationCenterPosition;
        }

        private void SwitchCameraView()
        {
            IsFirstPerson = !IsFirstPerson;
            if (IsFirstPerson)
            {
                cameraOffset = Vector3.zero;
                playerModel.SetActive(false);
            }
            else
            {
                cameraOffset = thirdPersonCameraOffset;
                playerModel.SetActive(true);
            }
        }

        private void UpdateFov()
        {
            float targetFov = IsZoomed ? zoomFov : defaultFov;
            camera.fieldOfView = Mathf.MoveTowards(camera.fieldOfView, targetFov, zoomSpeed * Time.deltaTime);
        }

    }
}