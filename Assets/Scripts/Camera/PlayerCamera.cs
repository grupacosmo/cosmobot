using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmobot
{
    [RequireComponent(typeof(Camera))]
    public class PlayerCamera : MonoBehaviour, DefaultInputActions.IPlayerCameraActions
    {
        public bool isFirstPerson { get; private set; } = true;
        public bool isZoomed { get; private set; } = false;

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

        private Camera camera;
        private DefaultInputActions actions;
        private float xRotation;
        private float yRotation;
        private float xInput;
        private float yInput;
        private Vector3 cameraOffset = Vector3.zero;
        private float defaultFov;
        private float zoomFov;

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
            if (context.performed) isZoomed = true;
            if (context.canceled) isZoomed = false;
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
            HandleInput();
            UpdateFov();
        }

        private void LateUpdate() => UpdateTransform();


        private void HandleInput()
        {
            Vector2 rotationDelta = new Vector2(yInput * Time.deltaTime * sensitivity.y, xInput * Time.deltaTime * sensitivity.x);
            if (isZoomed) rotationDelta *= zoomSensitivityMultiplier;
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
            isFirstPerson = !isFirstPerson;
            cameraOffset = isFirstPerson ? Vector3.zero : thirdPersonCameraOffset;
        }

        private void UpdateFov()
        {
            float targetFov = isZoomed ? zoomFov : defaultFov;
            camera.fieldOfView = Mathf.MoveTowards(camera.fieldOfView, targetFov, zoomSpeed * Time.deltaTime);
        }

    }
}