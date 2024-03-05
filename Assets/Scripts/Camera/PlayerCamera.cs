using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmobot
{
    public class PlayerCamera : MonoBehaviour, DefaultInputActions.IPlayerCameraActions
    {
        public float sensX;
        public float sensY;
        public float cameraChangeY;
        public float cameraChangeZ;
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
            SwitchCameraPosition();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            transform.position = cameraHolder.position;
        }

        private void Update()
        {
            AssignRotation();
            RotateCamera();
        }

        private void AssignRotation()
        {
            yRotation += xInput * Time.deltaTime * sensX;
            xRotation -= yInput * Time.deltaTime * sensY;
            xRotation = isFirstPerson
                ? Mathf.Clamp(xRotation, rotationClampBottom, rotationClampTop)
                : Mathf.Clamp(xRotation, rotationClampBottom - cameraChangeY / cameraChangeZ * 45f,
                    rotationClampTop - cameraChangeY / cameraChangeZ * 45f);
        }

        private void RotateCamera()
        {
            if (isFirstPerson)
            {
                transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            }
            else
            {
                ChangePositionInThirdPerson();
            }
        }

        private void ChangePositionInThirdPerson()
        {
            var cameraRotationCenterPosition = cameraHolder.position;
            var rayDirection = -transform.forward;
            var cameraDistance = (float)Math.Sqrt(Math.Pow(cameraChangeZ, 2f) + Math.Pow(cameraChangeY, 2f));
            transform.position =
                Physics.Raycast(cameraRotationCenterPosition, rayDirection, out var hit, cameraDistance)
                    ? hit.point - rayDirection.normalized * wallCollisionOffset
                    : rayDirection * cameraDistance + cameraRotationCenterPosition;
            cameraHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }

        private void SwitchCameraPosition()
        {
            if (isFirstPerson)
            {
                transform.position += new Vector3(0, cameraChangeY, -cameraChangeZ);
                transform.LookAt(playerObject);
            }
            else
            {
                cameraHolder.rotation = Quaternion.Euler(0, 0, 0);
                transform.position = cameraHolder.position;
            }

            isFirstPerson = !isFirstPerson;
        }
    }
}