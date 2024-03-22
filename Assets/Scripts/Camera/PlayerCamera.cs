using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmobot
{
    public class PlayerCamera : MonoBehaviour, DefaultInputActions.IPlayerCameraActions
    {
        public float sensX;
        public float sensY;
        public float zoomSensitivityMultiplier;
        public Vector3 thirdPersonCameraOffset;
        public float rotationClampTop;
        public float rotationClampBottom;
        public float wallCollisionOffset;
        public float zoomMagnification;
        public float zoomSpeed;
        public Transform playerObject;
        public Transform cameraHolder;

        private Camera cam;
        private DefaultInputActions actions;
        private float xRotation;
        private float yRotation;
        private float xInput;
        private float yInput;
        private Vector3 cameraOffset = Vector3.zero;
        private bool isFirstPerson = true;
        private bool isZoomed = false;
        private float defaultFov;
        private float zoomFov;

        private void OnEnable()
        {
            if (actions == null)
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

            cam = GetComponent<Camera>();
            defaultFov = cam.fieldOfView;
            zoomFov = defaultFov / zoomMagnification;
        }

        private void Update()
        {
            HandleInput();
            UpdateTransform();
            UpdateFov();
        }

        private void HandleInput()
        {
            Vector2 rotationDelta = new Vector2(yInput * Time.deltaTime * sensY, xInput * Time.deltaTime * sensX);
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
            transform.position =
                Physics.SphereCast(cameraRotationCenterPosition, wallCollisionOffset, 
                        rayDirection, out var hit, cameraDistance + wallCollisionOffset)
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
            cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, targetFov, zoomSpeed * Time.deltaTime);
        }

    }
}