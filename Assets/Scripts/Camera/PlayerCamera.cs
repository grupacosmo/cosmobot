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
        public KeyCode cameraChangeKey;
        public Transform playerObject;
        public Transform cameraHolder;
        private DefaultInputActions actions;
        private float xRotation;
        private float yRotation;
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
            yRotation += context.ReadValue<Vector2>().x * Time.deltaTime * sensX;
            xRotation -= context.ReadValue<Vector2>().y * Time.deltaTime * sensY;
            xRotation = isFirstPerson
                ? Mathf.Clamp(xRotation, -90f, 90f)
                : Mathf.Clamp(xRotation, -90f - cameraChangeY / cameraChangeZ * 45f,
                    90f - cameraChangeY / cameraChangeZ * 45f);
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            transform.position = cameraHolder.position;
        }

        private void Update()
        {
            RotateCamera();
            if (Input.GetKeyDown(cameraChangeKey))
            {
                SwitchCameraPosition();
            }
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

            playerObject.rotation = Quaternion.Euler(0, yRotation, 0);
        }

        private void ChangePositionInThirdPerson()
        {
            var cameraRotationCenterPosition = cameraHolder.position;
            var rayDirection = -transform.forward;
            var cameraDistance = (float)Math.Sqrt(Math.Pow(cameraChangeZ, 2f) + Math.Pow(cameraChangeY, 2f));
            var targetCameraPoint = Physics.Raycast(cameraRotationCenterPosition, rayDirection, out var hit, cameraDistance)
                ? hit.point
                : rayDirection * cameraDistance;
            transform.position = targetCameraPoint + cameraRotationCenterPosition;
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