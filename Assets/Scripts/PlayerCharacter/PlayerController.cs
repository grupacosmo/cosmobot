using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed;
        public float acceleration;

        private Vector3 velocity = Vector3.zero;

        private CharacterController cc;
        private Transform cameraTransform;

        private void Start()
        {
            cc = GetComponent<CharacterController>();
            cameraTransform = Camera.main.transform;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            Debug.Log(1f / Time.deltaTime);
        }

        private void FixedUpdate()
        {
            float input_x = Input.GetAxis("Horizontal");
            float input_z = Input.GetAxis("Vertical");

            Vector3 inputDirection = new Vector3(input_x, 0f, input_z).normalized;
            Vector3 targetVelocity = new Vector3(inputDirection.x, velocity.y, inputDirection.z) * moveSpeed;
            if (inputDirection.sqrMagnitude < 0.1f)
            {
                velocity = new Vector3(0f, velocity.y, 0f);
            }
            else
            {
                velocity = Vector3.MoveTowards(velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }

            cc.Move(velocity * Time.fixedDeltaTime);
        }
    }
}
