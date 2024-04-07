using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmobot
{
    [RequireComponent(typeof(Camera))]
    public class PlayerCamera : MonoBehaviour, DefaultInputActions.IPlayerCameraActions
    {
        public Vector2 sensitivity;
        public float zoomSensitivityMultiplier;
        public Vector3 thirdPersonCameraOffset;
        public float rotationClampTop;
        public float rotationClampBottom;
        public float wallCollisionOffset;
        public float zoomMagnification;
        public float zoomSpeed;
        public bool isFirstPerson { get; private set; } = true;
        public bool isZoomed { get; private set; } = false;

        [SerializeField] private Transform playerObject;
        [SerializeField] private Transform cameraHolder;
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
            xInput = context.ReadValue<Vector2>().x;
            yInput = context.ReadValue<Vector2>().y;
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
            transform.position = cameraHolder.position;

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
            var cameraRotationCenterPosition = cameraHolder.position;
            var rayDirection = transform.rotation * cameraOffset;
            var cameraDistance = cameraOffset.magnitude;
            var mask = LayerMask.GetMask("Default");
            transform.position =
                Physics.SphereCast(cameraRotationCenterPosition, wallCollisionOffset, 
                        rayDirection, out var hit, cameraDistance + wallCollisionOffset, mask)
                    ? hit.point + hit.normal * wallCollisionOffset
                    : rayDirection + cameraRotationCenterPosition;
            cameraHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
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