using UnityEngine;

namespace Cosmobot
{
    public class Camera : MonoBehaviour
    {
        public float sensX;
        public float sensY;
        public float cameraChangeY;
        public float cameraChangeZ;
        public KeyCode cameraChangeKey;
        private float _xRotation;
        private float _yRotation;
        private bool _isFirstPerson = true;
        public Transform playerObject;
        public Transform cameraHolder;
        
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            transform.position = cameraHolder.position;
        }

        private void Update()
        {
            var mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
            var mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

            _yRotation += mouseX;
            _xRotation -= mouseY;

            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            if (_isFirstPerson)
            {
                transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
            }
            else
            {
                cameraHolder.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
            }
            playerObject.rotation = Quaternion.Euler(0, _yRotation, 0);

            if (Input.GetKeyDown(cameraChangeKey))
            {
                SwitchCameraPosition();
            }
        }
        
        private void SwitchCameraPosition()
        {
            if (_isFirstPerson)
            {
                transform.position += new Vector3(0, cameraChangeY, -cameraChangeZ);
                transform.LookAt(playerObject);
            }
            else
            {
                cameraHolder.rotation = Quaternion.Euler(0, 0, 0);
                transform.position = cameraHolder.position;
            }
            
            _isFirstPerson = !_isFirstPerson;
        }
    }
}