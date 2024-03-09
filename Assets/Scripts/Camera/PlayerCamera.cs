using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmobot
{
    public class PlayerCamera : MonoBehaviour, DefaultInputActions.IPlayerCameraActions
    {
        public float sensX;
        public float sensY;
        public Vector3 thirdPersonCameraOffset;
        public float rotationClampTop;
        public float rotationClampBottom;
        public float wallCollisionOffset;
        public Transform playerObject;
        public Transform cameraHolder;

        private DefaultInputActions actions;
        private float xRotation;
        private float yRotation;
        private float xInput;
        private float yInput;
        private Vector3 cameraOffset = Vector3.zero;
        private bool isFirstPerson = true;

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

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            transform.position = cameraHolder.position;
        }

        private void Update()
        {
            HandleInput();
            UpdateTransform();
        }

        private void HandleInput()
        {
            yRotation += xInput * Time.deltaTime * sensX;
            xRotation -= yInput * Time.deltaTime * sensY;
            xRotation = Mathf.Clamp(xRotation, rotationClampBottom, rotationClampTop);
        }

        private void UpdateTransform()
        {
            var cameraRotationCenterPosition = cameraHolder.position;
            var rayDirection = transform.rotation * cameraOffset;
            var cameraDistance = cameraOffset.magnitude;
            transform.position =
                Physics.Raycast(cameraRotationCenterPosition, rayDirection, out var hit, cameraDistance)
                    ? hit.point - rayDirection.normalized * wallCollisionOffset
                    : rayDirection * cameraDistance + cameraRotationCenterPosition;
            cameraHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }

        private void SwitchCameraView()
        {
            isFirstPerson = !isFirstPerson;
            cameraOffset = isFirstPerson ? Vector3.zero : thirdPersonCameraOffset;
        }
    }
}