using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed;
        public float acceleration;
        public float jumpForce;
        public float gravity;

        private float input_x = 0f;
        private float input_z = 0f;
        private bool input_jump = false;
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
            input_x = Input.GetAxis("Horizontal");
            input_z = Input.GetAxis("Vertical");
            input_jump = Input.GetKeyDown(KeyCode.Space) || input_jump;

        }

        private void FixedUpdate()
        {

            velocity = cc.velocity;
            // Horizontal
            Vector3 inputDirection = new Vector3(input_x, 0f, input_z).normalized;
            Vector3 targetVelocity = new Vector3(inputDirection.x, 0f, inputDirection.z) * moveSpeed;
            targetVelocity += new Vector3(0, velocity.y, 0);
            if (inputDirection.sqrMagnitude < 0.1f)
            {
                velocity = new Vector3(0f, velocity.y, 0f);
            }
            else
            {
                velocity = Vector3.MoveTowards(velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }

            // Vertical
            velocity.y -= gravity * Time.fixedDeltaTime;
            if (input_jump && cc.isGrounded)
            {
                velocity.y = jumpForce;
            }
            input_jump = false;

            Debug.Log(velocity);
            cc.Move(velocity * Time.fixedDeltaTime);
        }
    }
}
